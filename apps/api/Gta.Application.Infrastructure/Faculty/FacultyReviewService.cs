using System.Data;
using Gta.Application.Application.Documents;
using Gta.Application.Application.Faculty;
using Gta.Application.Contracts.Documents;
using Gta.Application.Contracts.Faculty;
using Gta.Application.Contracts.Profiles;
using Gta.Application.Domain.Applications;
using Gta.Application.Domain.Documents;
using Gta.Application.Domain.Profiles;
using Gta.Application.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Gta.Application.Application.Notifications;

namespace Gta.Application.Infrastructure.Faculty;

public sealed class FacultyReviewService(GtaDbContext dbContext, IDocumentStorage storage, IEmailOutbox emailOutbox) : IFacultyReviewService
{
    public async Task<IReadOnlyCollection<FacultySectionResponse>> GetSectionsAsync(Guid facultyUserId, CancellationToken cancellationToken) =>
        await dbContext.FacultySectionAssignments.AsNoTracking()
            .Where(assignment => assignment.FacultyUserId == facultyUserId && assignment.IsActive)
            .OrderBy(assignment => assignment.CourseSection.Course.SubjectCode)
            .Select(assignment => new FacultySectionResponse(
                assignment.CourseSectionId,
                assignment.CourseSection.Course.SubjectCode + " " + assignment.CourseSection.Course.CatalogNumber,
                assignment.CourseSection.SectionNumber,
                assignment.CourseSection.Course.Title,
                assignment.CourseSection.AcademicTerm.Name,
                assignment.CourseSection.Schedule,
                dbContext.ApplicationChoices.Count(choice => choice.CourseSectionId == assignment.CourseSectionId)))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<FacultyApplicationListItemResponse>> GetApplicationsAsync(Guid facultyUserId, CancellationToken cancellationToken) =>
        await AuthorizedChoices(facultyUserId).AsNoTracking()
            .OrderByDescending(choice => choice.Application.SubmittedAtUtc)
            .Select(choice => new FacultyApplicationListItemResponse(
                choice.Id,
                choice.ApplicationId,
                choice.Application.ApplicantUser.DisplayName,
                choice.Application.ApplicantUser.ApplicantProfile!.Program ?? string.Empty,
                choice.CourseSection.Course.SubjectCode + " " + choice.CourseSection.Course.CatalogNumber,
                choice.CourseSection.SectionNumber,
                choice.Application.State.ToString(),
                choice.Application.SubmittedAtUtc!.Value,
                dbContext.FacultyReviewActions.Any(action => action.ApplicationChoiceId == choice.Id && action.Type == ReviewActionType.Interview && action.IsActive),
                dbContext.FacultyReviewActions.Any(action => action.ApplicationChoiceId == choice.Id && action.Type == ReviewActionType.HireRecommendation && action.IsActive)))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<FacultyInterviewQueueItemResponse>> GetInterviewQueueAsync(Guid facultyUserId, CancellationToken cancellationToken) =>
        await AuthorizedChoices(facultyUserId).AsNoTracking()
            .Where(choice => dbContext.FacultyReviewActions.Any(action => action.ApplicationChoiceId == choice.Id && action.Type == ReviewActionType.Interview && action.IsActive))
            .OrderBy(choice => choice.CourseSection.Course.SubjectCode).ThenBy(choice => choice.CourseSection.SectionNumber).ThenBy(choice => choice.Application.ApplicantUser.DisplayName)
            .Select(choice => new FacultyInterviewQueueItemResponse(choice.Id, choice.ApplicationId, choice.Application.ApplicantUser.DisplayName, choice.Application.ApplicantUser.ApplicantProfile!.Program ?? string.Empty,
                choice.CourseSection.Course.SubjectCode + " " + choice.CourseSection.Course.CatalogNumber, choice.CourseSection.SectionNumber, choice.CourseSection.AcademicTerm.Name, choice.Application.State.ToString(), choice.Application.EmploymentBasis.ToString(),
                dbContext.FacultyReviewActions.Where(action => action.ApplicationChoiceId == choice.Id && action.Type == ReviewActionType.Interview && action.IsActive).Max(action => action.CreatedAtUtc),
                dbContext.FacultyReviewActions.Any(action => action.ApplicationChoiceId == choice.Id && action.Type == ReviewActionType.HireRecommendation && action.IsActive),
                dbContext.Placements.Count(placement => placement.ApplicationChoice.ApplicationId == choice.ApplicationId && placement.IsActive),
                choice.Application.EmploymentBasis == EmploymentBasis.PartTime10Hours ? 1 : 2,
                dbContext.Placements.Count(placement => placement.ApplicationChoice.ApplicationId == choice.ApplicationId && placement.IsActive) >= (choice.Application.EmploymentBasis == EmploymentBasis.PartTime10Hours ? 1 : 2) ? "Placed" : dbContext.FacultyReviewActions.Any(action => action.ApplicationChoiceId == choice.Id && action.Type == ReviewActionType.HireRecommendation && action.IsActive) ? "HireRecommended" : "AwaitingDecision"))
            .ToListAsync(cancellationToken);

    public async Task<FacultyReviewResponse?> GetReviewAsync(Guid facultyUserId, Guid choiceId, CancellationToken cancellationToken)
    {
        var choice = await AuthorizedChoices(facultyUserId).AsNoTracking()
            .Include(item => item.Application).ThenInclude(application => application.ApplicantUser).ThenInclude(user => user.ApplicantProfile!).ThenInclude(profile => profile.EducationRecords)
            .Include(item => item.Application).ThenInclude(application => application.ApplicantUser).ThenInclude(user => user.ApplicantProfile!).ThenInclude(profile => profile.ExperienceRecords)
            .Include(item => item.CourseSection).ThenInclude(section => section.Course)
            .AsSplitQuery().SingleOrDefaultAsync(item => item.Id == choiceId, cancellationToken);
        if (choice?.Application.ApplicantUser.ApplicantProfile is not { } profile) return null;

        var actions = await dbContext.FacultyReviewActions.AsNoTracking().Where(action => action.ApplicationChoiceId == choice.Id && action.IsActive).ToListAsync(cancellationToken);
        var documents = await dbContext.Documents.AsNoTracking()
            .Where(document => document.OwnerUserId == choice.Application.ApplicantUserId && document.State == DocumentState.Active)
            .OrderBy(document => document.Type)
            .Select(document => new DocumentResponse(document.Id, document.Type.ToString(), document.OriginalFileName, document.MediaType, document.ByteLength, document.Version, document.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return new FacultyReviewResponse(
            choice.Id, choice.ApplicationId, choice.Application.Reference, choice.Application.State.ToString(), choice.Application.EmploymentBasis.ToString(),
            $"{choice.CourseSection.Course.SubjectCode} {choice.CourseSection.Course.CatalogNumber}", choice.CourseSection.SectionNumber,
            choice.Application.SubmittedAtUtc!.Value, MapProfile(choice.Application.ApplicantUser, profile), documents,
            actions.Any(action => action.Type == ReviewActionType.Interview), actions.Any(action => action.Type == ReviewActionType.HireRecommendation),
            actions.OrderByDescending(action => action.UpdatedAtUtc).Select(action => action.InternalNotes).FirstOrDefault(notes => !string.IsNullOrWhiteSpace(notes)));
    }

    public async Task<FacultyActionResponse?> RecordActionAsync(
        Guid facultyUserId,
        Guid choiceId,
        RecordFacultyActionRequest request,
        string correlationId,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ReviewActionType>(request.Action, true, out var actionType)) throw new ArgumentException("Faculty action is invalid.");
        if (request.InternalNotes?.Length > 2000) throw new ArgumentException("Internal notes cannot exceed 2000 characters.");

        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            var choice = await AuthorizedChoices(facultyUserId)
                .Include(item => item.Application).ThenInclude(application => application.Choices)
                .Include(item => item.Application).ThenInclude(application => application.ApplicantUser)
                .SingleOrDefaultAsync(item => item.Id == choiceId, cancellationToken);
            if (choice is null) return null;

            var activeAction = await dbContext.FacultyReviewActions.SingleOrDefaultAsync(action =>
                action.ApplicationChoiceId == choiceId && action.FacultyUserId == facultyUserId && action.Type == actionType && action.IsActive,
                cancellationToken);
            var now = DateTimeOffset.UtcNow;

            if (request.Active)
            {
                if (activeAction is not null) throw new FacultyActionConflictException("This action is already active.");
                if (actionType == ReviewActionType.HireRecommendation)
                {
                    var interviewed = await dbContext.FacultyReviewActions.AnyAsync(action => action.ApplicationChoiceId == choiceId && action.Type == ReviewActionType.Interview && action.IsActive, cancellationToken);
                    if (!interviewed) throw new FacultyActionConflictException("Mark the applicant for interview before recommending hire.");
                    var activeHireCount = await dbContext.FacultyReviewActions.CountAsync(action =>
                        action.Type == ReviewActionType.HireRecommendation && action.IsActive && action.ApplicationChoice.ApplicationId == choice.ApplicationId,
                        cancellationToken);
                    if (!PlacementPolicy.CanAddAssignment(choice.Application.EmploymentBasis, activeHireCount))
                        throw new FacultyActionConflictException("The applicant has reached the hiring limit for the selected employment basis.");
                }

                activeAction = new FacultyReviewAction
                {
                    ApplicationChoiceId = choiceId,
                    FacultyUserId = facultyUserId,
                    Type = actionType,
                    IsActive = true,
                    InternalNotes = Clean(request.InternalNotes),
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now,
                };
                dbContext.FacultyReviewActions.Add(activeAction);
            }
            else
            {
                if (activeAction is null) throw new FacultyActionConflictException("This action is not active.");
                if (actionType == ReviewActionType.Interview && await dbContext.FacultyReviewActions.AnyAsync(action => action.ApplicationChoiceId == choiceId && action.Type == ReviewActionType.HireRecommendation && action.IsActive, cancellationToken))
                    throw new FacultyActionConflictException("Remove the hire recommendation before removing the interview action.");
                if (actionType == ReviewActionType.HireRecommendation && await dbContext.Placements.AnyAsync(placement => placement.ApplicationChoiceId == choiceId && placement.IsActive, cancellationToken))
                    throw new FacultyActionConflictException("A placed applicant cannot have the hire recommendation removed.");
                activeAction.IsActive = false;
                activeAction.InternalNotes = Clean(request.InternalNotes) ?? activeAction.InternalNotes;
                activeAction.UpdatedAtUtc = now;
            }

            if (actionType == ReviewActionType.Interview)
            {
                var targetState = request.Active ? ApplicationState.Interview : ApplicationState.UnderReview;
                if (choice.Application.State != targetState)
                {
                    dbContext.ApplicationStatusHistory.Add(new ApplicationStatusHistory
                    {
                        Application = choice.Application,
                        FromState = choice.Application.State,
                        ToState = targetState,
                        ChangedAtUtc = now,
                        ChangedByUserId = facultyUserId,
                        Reason = request.Active ? "Faculty marked applicant for interview." : "Faculty removed interview mark.",
                    });
                    choice.Application.State = targetState;
                    choice.Application.UpdatedAtUtc = now;
                }
                if (request.Active) emailOutbox.Queue(choice.Application.ApplicantUser.Email, "GTA interview update", $"Your GTA application {choice.Application.Reference} has moved to Interview status. Sign in to review the latest status.", correlationId);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new FacultyActionResponse(choiceId, actionType.ToString(), request.Active, now);
        });
    }

    public async Task<DocumentDownload?> DownloadDocumentAsync(Guid facultyUserId, Guid documentId, CancellationToken cancellationToken)
    {
        var document = await dbContext.Documents.AsNoTracking().SingleOrDefaultAsync(candidate =>
            candidate.Id == documentId && candidate.State == DocumentState.Active &&
            dbContext.ApplicationChoices.Any(choice => choice.Application.ApplicantUserId == candidate.OwnerUserId &&
                dbContext.FacultySectionAssignments.Any(assignment => assignment.FacultyUserId == facultyUserId && assignment.CourseSectionId == choice.CourseSectionId && assignment.IsActive)),
            cancellationToken);
        return document is null ? null : new DocumentDownload(await storage.OpenReadAsync(document.StorageKey, cancellationToken), document.MediaType, document.OriginalFileName);
    }

    private IQueryable<ApplicationChoice> AuthorizedChoices(Guid facultyUserId) => dbContext.ApplicationChoices.Where(choice =>
        dbContext.FacultySectionAssignments.Any(assignment =>
            assignment.FacultyUserId == facultyUserId && assignment.CourseSectionId == choice.CourseSectionId && assignment.IsActive));

    private static ApplicantProfileResponse MapProfile(Gta.Application.Domain.Identity.User user, ApplicantProfile profile) => new(
        user.DisplayName, user.Email, user.UniversityId, profile.PreferredName, profile.PhoneNumber, profile.Program, profile.Degree, profile.Major,
        profile.Gpa, profile.ExpectedGraduationTerm, profile.ExpectedGraduationYear, profile.LinkedInUrl,
        profile.EducationRecords.Select(item => new EducationRecordResponse(item.Id, item.Institution, item.Degree, item.FieldOfStudy, item.StartDate, item.EndDate)).ToArray(),
        profile.ExperienceRecords.Select(item => new ExperienceRecordResponse(item.Id, item.Organization, item.Title, item.Description, item.StartDate, item.EndDate, item.IsGtaExperience)).ToArray(),
        profile.UpdatedAtUtc);

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
