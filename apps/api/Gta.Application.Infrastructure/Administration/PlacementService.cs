using System.Data;
using Gta.Application.Application.Administration;
using Gta.Application.Contracts.Administration;
using Gta.Application.Domain.Applications;
using Gta.Application.Domain.Auditing;
using Gta.Application.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Gta.Application.Application.Notifications;

namespace Gta.Application.Infrastructure.Administration;

public sealed class PlacementService(GtaDbContext db, IEmailOutbox emailOutbox) : IPlacementService
{
    public async Task<IReadOnlyCollection<PlacementCandidateResponse>> GetCandidatesAsync(CancellationToken token)
    {
        var choices = await db.ApplicationChoices.AsNoTracking()
            .Where(choice => db.FacultyReviewActions.Any(action => action.ApplicationChoiceId == choice.Id && action.Type == ReviewActionType.HireRecommendation && action.IsActive))
            .Include(choice => choice.Application).ThenInclude(application => application.ApplicantUser)
            .Include(choice => choice.CourseSection).ThenInclude(section => section.Course)
            .Include(choice => choice.CourseSection).ThenInclude(section => section.AcademicTerm)
            .OrderBy(choice => choice.Application.ApplicantUser.DisplayName)
            .ThenBy(choice => choice.PreferenceOrder)
            .AsSplitQuery().ToListAsync(token);

        var applicationIds = choices.Select(x => x.ApplicationId).Distinct().ToArray();
        var sectionIds = choices.Select(x => x.CourseSectionId).Distinct().ToArray();
        var placementCounts = await db.Placements.AsNoTracking().Where(x => x.IsActive && applicationIds.Contains(x.ApplicationChoice.ApplicationId)).GroupBy(x => x.ApplicationChoice.ApplicationId).Select(x => new { ApplicationId = x.Key, Count = x.Count() }).ToDictionaryAsync(x => x.ApplicationId, x => x.Count, token);
        var filledCounts = await db.Placements.AsNoTracking().Where(x => x.IsActive && sectionIds.Contains(x.CourseSectionId)).GroupBy(x => x.CourseSectionId).Select(x => new { SectionId = x.Key, Count = x.Count() }).ToDictionaryAsync(x => x.SectionId, x => x.Count, token);
        var placedChoiceIds = await db.Placements.AsNoTracking().Where(x => x.IsActive && applicationIds.Contains(x.ApplicationChoice.ApplicationId)).Select(x => x.ApplicationChoiceId).ToHashSetAsync(token);

        return choices.Select(choice =>
        {
            var active = placementCounts.GetValueOrDefault(choice.ApplicationId);
            var maximum = PlacementPolicy.MaximumAssignments(choice.Application.EmploymentBasis);
            return new PlacementCandidateResponse(choice.Id, choice.ApplicationId, choice.Application.Reference, choice.Application.ApplicantUser.DisplayName,
                choice.Application.EmploymentBasis.ToString(), AssignmentState(active, maximum), active, maximum, choice.CourseSectionId,
                $"{choice.CourseSection.Course.SubjectCode} {choice.CourseSection.Course.CatalogNumber}", choice.CourseSection.SectionNumber,
                choice.CourseSection.AcademicTerm.Name, choice.CourseSection.AvailablePositions, filledCounts.GetValueOrDefault(choice.CourseSectionId), placedChoiceIds.Contains(choice.Id));
        }).ToArray();
    }

    public async Task<PlacementActionResponse?> UpdateAsync(Guid choiceId, UpdatePlacementRequest request, Guid actorId, string correlationId, CancellationToken token)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, token);
            var choice = await db.ApplicationChoices.Include(x => x.Application).ThenInclude(x => x.ApplicantUser).Include(x => x.CourseSection).SingleOrDefaultAsync(x => x.Id == choiceId, token);
            if (choice is null) return null;
            var now = DateTimeOffset.UtcNow;
            var activePlacement = await db.Placements.SingleOrDefaultAsync(x => x.ApplicationChoiceId == choiceId && x.IsActive, token);
            var activeCount = await db.Placements.CountAsync(x => x.IsActive && x.ApplicationChoice.ApplicationId == choice.ApplicationId, token);
            var maximum = PlacementPolicy.MaximumAssignments(choice.Application.EmploymentBasis);

            if (request.Active)
            {
                if (activePlacement is not null) throw new AdministrationConflictException("The applicant is already placed in this section.");
                if (!choice.CourseSection.IsActive) throw new AdministrationConflictException("The selected section is inactive.");
                if (!await db.FacultyReviewActions.AnyAsync(x => x.ApplicationChoiceId == choiceId && x.Type == ReviewActionType.HireRecommendation && x.IsActive, token)) throw new AdministrationConflictException("An active hire recommendation is required before placement.");
                if (!PlacementPolicy.CanAddAssignment(choice.Application.EmploymentBasis, activeCount)) throw new AdministrationConflictException("The applicant has reached the placement limit for the selected employment basis.");
                var filled = await db.Placements.CountAsync(x => x.CourseSectionId == choice.CourseSectionId && x.IsActive, token);
                if (choice.CourseSection.AvailablePositions.HasValue && filled >= choice.CourseSection.AvailablePositions.Value) throw new AdministrationConflictException("The section has no available positions.");
                db.Placements.Add(new Placement { ApplicationChoiceId = choiceId, CourseSectionId = choice.CourseSectionId, IsActive = true, CreatedAtUtc = now, UpdatedAtUtc = now });
                activeCount++;
                emailOutbox.Queue(choice.Application.ApplicantUser.Email, "GTA placement update", $"You have been selected for a GTA placement for application {choice.Application.Reference}. Sign in to review your status.", correlationId);
            }
            else
            {
                if (activePlacement is null) throw new AdministrationConflictException("The applicant is not actively placed in this section.");
                activePlacement.IsActive = false; activePlacement.UpdatedAtUtc = now; activeCount--;
            }

            var targetState = activeCount >= maximum ? ApplicationState.Selected : ApplicationState.Interview;
            if (choice.Application.State != targetState)
            {
                db.ApplicationStatusHistory.Add(new ApplicationStatusHistory { ApplicationId = choice.ApplicationId, FromState = choice.Application.State, ToState = targetState, ChangedAtUtc = now, ChangedByUserId = actorId, Reason = request.Active ? "Administrator assigned applicant to a section." : "Administrator removed applicant placement." });
                choice.Application.State = targetState; choice.Application.UpdatedAtUtc = now;
            }
            db.AuditLogs.Add(new AuditLog { ActorUserId = actorId, OccurredAtUtc = now, Action = request.Active ? "PlacementCreated" : "PlacementRemoved", EntityType = "ApplicationChoice", EntityReference = choiceId.ToString(), Result = "Succeeded", CorrelationId = correlationId });
            await db.SaveChangesAsync(token);
            await transaction.CommitAsync(token);
            return new PlacementActionResponse(choiceId, request.Active, AssignmentState(activeCount, maximum), activeCount, maximum, now);
        });
    }

    private static string AssignmentState(int active, int maximum) => active == 0 ? "Unassigned" : active < maximum ? "PartiallyAssigned" : "FullyAssigned";
}
