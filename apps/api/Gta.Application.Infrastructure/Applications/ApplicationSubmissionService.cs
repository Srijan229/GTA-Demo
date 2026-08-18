using System.Data;
using Gta.Application.Application.Applications;
using Gta.Application.Application.Profiles;
using Gta.Application.Contracts.Applications;
using Gta.Application.Domain.Applications;
using Gta.Application.Domain.Documents;
using Gta.Application.Domain.Auditing;
using Gta.Application.Application.Notifications;
using Gta.Application.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Gta.Application.Infrastructure.Applications;

public sealed class ApplicationSubmissionService(
    GtaDbContext dbContext,
    IApplicantProfileService profileService, IEmailOutbox emailOutbox) : IApplicationSubmissionService
{
    public async Task<ApplicationConfigurationResponse> GetConfigurationAsync(CancellationToken cancellationToken) { var value = await dbContext.SystemSettings.Where(x => x.Key == "Applications.MaximumSectionChoices").Select(x => x.Value).SingleOrDefaultAsync(cancellationToken); return new(int.TryParse(value, out var maximum) ? maximum : 5); }
    public async Task<IReadOnlyCollection<AvailableSectionResponse>> GetAvailableSectionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var program = await dbContext.ApplicantProfiles.Where(profile => profile.UserId == userId).Select(profile => profile.Program).SingleAsync(cancellationToken);
        var phases = await dbContext.ApplicationPhases.AsNoTracking()
            .Where(phase => phase.Program == program && phase.IsActive && phase.OpensAtUtc <= now && phase.ClosesAtUtc >= now)
            .ToListAsync(cancellationToken);
        var phaseByTerm = phases.GroupBy(phase => phase.AcademicTermId).ToDictionary(group => group.Key, group => group.OrderBy(phase => phase.ClosesAtUtc).First());
        var termIds = phaseByTerm.Keys.ToArray();
        var appliedSectionIds = await dbContext.ApplicationChoices.Where(choice => choice.Application.ApplicantUserId == userId).Select(choice => choice.CourseSectionId).ToListAsync(cancellationToken);
        var sections = await dbContext.CourseSections.AsNoTracking().Include(section => section.Course).Include(section => section.AcademicTerm)
            .Where(section => section.IsActive && termIds.Contains(section.AcademicTermId))
            .OrderBy(section => section.Course.SubjectCode).ThenBy(section => section.Course.CatalogNumber).ThenBy(section => section.SectionNumber)
            .ToListAsync(cancellationToken);
        return sections.Select(section =>
        {
            var phase = phaseByTerm[section.AcademicTermId];
            return new AvailableSectionResponse(section.Id, phase.Id, phase.Name, section.AcademicTerm.Name,
                $"{section.Course.SubjectCode} {section.Course.CatalogNumber}", section.Course.Title, section.SectionNumber,
                section.Schedule, section.DeliveryMethod, section.AvailablePositions, appliedSectionIds.Contains(section.Id));
        }).ToArray();
    }

    public async Task<ApplicationResponse> SubmitAsync(Guid userId, SubmitApplicationRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<EmploymentBasis>(request.EmploymentBasis, true, out var basis)) throw new ArgumentException("Employment basis is invalid.");
        var sectionIds = request.SectionIds.Distinct().ToArray();
        if (sectionIds.Length == 0) throw new ArgumentException("Select at least one section.");
        var choiceLimit = (await GetConfigurationAsync(cancellationToken)).MaximumSectionChoices;
        if (sectionIds.Length > choiceLimit) throw new ArgumentException($"Select no more than {choiceLimit} sections.");

        var completion = await profileService.GetCompletionAsync(userId, cancellationToken);
        if (completion?.Percentage != 100) throw new ApplicationReadinessException("Complete all profile sections before submitting an application.");
        var documentTypes = await dbContext.Documents.Where(document => document.OwnerUserId == userId && document.State == DocumentState.Active).Select(document => document.Type).Distinct().ToListAsync(cancellationToken);
        if (!documentTypes.Contains(DocumentType.Resume) || !documentTypes.Contains(DocumentType.UnofficialTranscript))
            throw new ApplicationReadinessException("Upload a resume and unofficial transcript before submitting an application.");

        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            var now = DateTimeOffset.UtcNow;
            var phase = await dbContext.ApplicationPhases.Include(item => item.AcademicTerm).SingleOrDefaultAsync(
                item => item.Id == request.PhaseId && item.IsActive && item.OpensAtUtc <= now && item.ClosesAtUtc >= now,
                cancellationToken) ?? throw new ArgumentException("The selected application phase is not open.");

            var program = await dbContext.ApplicantProfiles.Where(profile => profile.UserId == userId).Select(profile => profile.Program).SingleAsync(cancellationToken);
            if (!string.Equals(program, phase.Program, StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("The selected phase is not available for this program.");
            if (await dbContext.Applications.AnyAsync(item => item.ApplicantUserId == userId && item.ApplicationPhaseId == phase.Id, cancellationToken))
                throw new ApplicationConflictException("An application has already been submitted for this phase.");

            var sections = await dbContext.CourseSections.Include(section => section.Course)
                .Where(section => sectionIds.Contains(section.Id) && section.AcademicTermId == phase.AcademicTermId && section.IsActive)
                .ToListAsync(cancellationToken);
            if (sections.Count != sectionIds.Length) throw new ArgumentException("One or more selected sections are unavailable.");

            var application = new ApplicantApplication
            {
                ApplicantUserId = userId,
                ApplicationPhase = phase,
                Reference = $"GTA-{phase.AcademicTerm.Code}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
                EmploymentBasis = basis,
                State = ApplicationState.Submitted,
                SubmittedAtUtc = now,
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
            };
            var preference = 1;
            foreach (var section in sections.OrderBy(section => Array.IndexOf(sectionIds, section.Id)))
            {
                application.Choices.Add(new ApplicationChoice
                {
                    CourseSection = section,
                    PreferenceOrder = preference++,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now,
                });
            }
            dbContext.Applications.Add(application);
            dbContext.ApplicationStatusHistory.Add(new ApplicationStatusHistory
            {
                Application = application,
                FromState = ApplicationState.Draft,
                ToState = ApplicationState.Submitted,
                ChangedAtUtc = now,
                ChangedByUserId = userId,
                Reason = "Applicant submitted application.",
            });
            var applicantEmail = await dbContext.Users.Where(x => x.Id == userId).Select(x => x.Email).SingleAsync(cancellationToken);
            emailOutbox.Queue(applicantEmail, "GTA application submitted", $"Your GTA application {application.Reference} was submitted successfully for {phase.Name}. Current status: Submitted.");
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Map(application, phase);
        });
    }

    public async Task<IReadOnlyCollection<ApplicationResponse>> GetMineAsync(Guid userId, CancellationToken cancellationToken)
    {
        var applications = await dbContext.Applications.AsNoTracking()
            .Include(application => application.ApplicationPhase).ThenInclude(phase => phase.AcademicTerm)
            .Include(application => application.Choices).ThenInclude(choice => choice.CourseSection).ThenInclude(section => section.Course)
            .Where(application => application.ApplicantUserId == userId)
            .OrderByDescending(application => application.SubmittedAtUtc)
            .AsSplitQuery().ToListAsync(cancellationToken);
        return applications.Select(application => Map(application, application.ApplicationPhase)).ToArray();
    }

    public async Task<ApplicationDetailResponse?> GetMineAsync(Guid userId, Guid applicationId, CancellationToken cancellationToken)
    {
        var application = await DetailQuery(userId).SingleOrDefaultAsync(x => x.Id == applicationId, cancellationToken);
        if (application is null) return null;
        var hasHiringActivity = await HasHiringActivityAsync(applicationId, cancellationToken);
        return MapDetail(application, hasHiringActivity);
    }

    public async Task<ApplicationDetailResponse?> WithdrawAsync(Guid userId, Guid applicationId, WithdrawApplicationRequest request, string correlationId, CancellationToken cancellationToken)
    {
        if (request.Reason?.Length > 500) throw new ArgumentException("Withdrawal reason cannot exceed 500 characters.");
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            var application = await DetailQuery(userId).SingleOrDefaultAsync(x => x.Id == applicationId, cancellationToken);
            if (application is null) return null;
            if (application.State == ApplicationState.Withdrawn) throw new ApplicationConflictException("This application is already withdrawn.");
            var hasHiringActivity = await HasHiringActivityAsync(applicationId, cancellationToken);
            var blockedReason = ApplicationWithdrawalPolicy.BlockedReason(application.State, hasHiringActivity);
            if (blockedReason is not null) throw new ApplicationConflictException(blockedReason);

            var now = DateTimeOffset.UtcNow;
            var priorState = application.State;
            application.State = ApplicationState.Withdrawn;
            application.UpdatedAtUtc = now;
            dbContext.ApplicationStatusHistory.Add(new ApplicationStatusHistory
            {
                Application = application,
                FromState = priorState,
                ToState = ApplicationState.Withdrawn,
                ChangedAtUtc = now,
                ChangedByUserId = userId,
                Reason = string.IsNullOrWhiteSpace(request.Reason) ? "Applicant withdrew application." : $"Applicant withdrew application: {request.Reason.Trim()}",
            });
            dbContext.AuditLogs.Add(new AuditLog { ActorUserId = userId, OccurredAtUtc = now, Action = "ApplicationWithdrawn", EntityType = "ApplicantApplication", EntityReference = application.Id.ToString(), Result = "Succeeded", CorrelationId = correlationId });
            var applicantEmail = await dbContext.Users.Where(x => x.Id == userId).Select(x => x.Email).SingleAsync(cancellationToken);
            emailOutbox.Queue(applicantEmail, "GTA application withdrawn", $"Your GTA application {application.Reference} has been withdrawn.", correlationId);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return MapDetail(application, false);
        });
    }

    private IQueryable<ApplicantApplication> DetailQuery(Guid userId) => dbContext.Applications
        .Include(application => application.ApplicationPhase).ThenInclude(phase => phase.AcademicTerm)
        .Include(application => application.Choices).ThenInclude(choice => choice.CourseSection).ThenInclude(section => section.Course)
        .Include(application => application.StatusHistory)
        .Where(application => application.ApplicantUserId == userId)
        .AsSplitQuery();

    private async Task<bool> HasHiringActivityAsync(Guid applicationId, CancellationToken cancellationToken) =>
        await dbContext.FacultyReviewActions.AnyAsync(x => x.ApplicationChoice.ApplicationId == applicationId && x.IsActive, cancellationToken)
        || await dbContext.Placements.AnyAsync(x => x.ApplicationChoice.ApplicationId == applicationId && x.IsActive, cancellationToken);

    private static ApplicationDetailResponse MapDetail(ApplicantApplication application, bool hasHiringActivity)
    {
        var blocked = ApplicationWithdrawalPolicy.BlockedReason(application.State, hasHiringActivity);
        return new ApplicationDetailResponse(application.Id, application.Reference, application.ApplicationPhase.Name, application.ApplicationPhase.AcademicTerm.Name,
            application.EmploymentBasis.ToString(), application.State.ToString(), application.SubmittedAtUtc!.Value,
            application.Choices.OrderBy(x => x.PreferenceOrder).Select(x => new ApplicationChoiceResponse(x.CourseSectionId, $"{x.CourseSection.Course.SubjectCode} {x.CourseSection.Course.CatalogNumber}", x.CourseSection.SectionNumber, x.CourseSection.Course.Title)).ToArray(),
            application.StatusHistory.OrderBy(x => x.ChangedAtUtc).Select(x => new ApplicationStatusHistoryResponse(x.FromState.ToString(), x.ToState.ToString(), x.ChangedAtUtc, x.Reason)).ToArray(),
            blocked is null, blocked);
    }

    private static ApplicationResponse Map(ApplicantApplication application, ApplicationPhase phase) => new(
        application.Id, application.Reference, phase.Name, phase.AcademicTerm.Name, application.EmploymentBasis.ToString(), application.State.ToString(),
        application.SubmittedAtUtc!.Value,
        application.Choices.OrderBy(choice => choice.PreferenceOrder).Select(choice => new ApplicationChoiceResponse(
            choice.CourseSectionId,
            $"{choice.CourseSection.Course.SubjectCode} {choice.CourseSection.Course.CatalogNumber}",
            choice.CourseSection.SectionNumber,
            choice.CourseSection.Course.Title)).ToArray());
}
