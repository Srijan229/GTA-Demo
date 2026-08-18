using Gta.Application.Application.Administration;
using Gta.Application.Contracts.Administration;
using Gta.Application.Domain.Applications;
using Gta.Application.Domain.Auditing;
using Gta.Application.Domain.Identity;
using Gta.Application.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Gta.Application.Infrastructure.Administration;

public sealed class AdministrationService(GtaDbContext db) : IAdministrationService
{
    public async Task<AdminDashboardResponse> GetDashboardAsync(CancellationToken token)
    {
        var applications = await db.Applications.CountAsync(token);
        var applicants = await db.UserRoles.CountAsync(x => x.Role.NormalizedName == "APPLICANT" && x.User.IsActive, token);
        var sections = await db.CourseSections.CountAsync(x => x.IsActive, token);
        var faculty = await db.UserRoles.CountAsync(x => x.Role.NormalizedName == "FACULTY" && x.User.IsActive, token);
        var awaiting = await db.Applications.CountAsync(x => x.State == ApplicationState.Submitted || x.State == ApplicationState.UnderReview, token);
        var unassigned = await db.CourseSections.CountAsync(x => x.IsActive && !db.FacultySectionAssignments.Any(a => a.CourseSectionId == x.Id && a.IsActive), token);
        var warnings = new List<string>();
        if (unassigned > 0) warnings.Add($"{unassigned} active section(s) have no faculty assignment.");
        if (!await db.ApplicationPhases.AnyAsync(x => x.IsActive, token)) warnings.Add("No application phase is active.");
        if (awaiting > 0) warnings.Add($"{awaiting} application(s) are awaiting a decision.");
        return new(applications, applicants, sections, faculty, awaiting, unassigned, warnings);
    }

    public async Task<IReadOnlyCollection<AdminApplicationResponse>> GetApplicationsAsync(CancellationToken token) =>
        await db.Applications.AsNoTracking().OrderByDescending(x => x.SubmittedAtUtc).Select(x => new AdminApplicationResponse(x.Id, x.Reference, x.ApplicantUser.DisplayName, x.ApplicantUser.ApplicantProfile!.Program ?? "", x.State.ToString(), x.SubmittedAtUtc, x.Choices.Count)).ToListAsync(token);

    public async Task<IReadOnlyCollection<AdminApplicantResponse>> GetApplicantsAsync(CancellationToken token) =>
        await db.Users.AsNoTracking().Where(x => x.UserRoles.Any(r => r.Role.NormalizedName == "APPLICANT")).OrderBy(x => x.DisplayName).Select(x => new AdminApplicantResponse(x.Id, x.DisplayName, x.Email, x.UniversityId, x.ApplicantProfile != null ? x.ApplicantProfile.Program : null, x.ApplicantProfile != null && x.ApplicantProfile.Program != null && x.ApplicantProfile.Degree != null && x.ApplicantProfile.Major != null && x.ApplicantProfile.ExpectedGraduationYear != null, db.Applications.Count(a => a.ApplicantUserId == x.Id))).ToListAsync(token);

    public async Task<IReadOnlyCollection<AdminSectionResponse>> GetSectionsAsync(CancellationToken token) =>
        await db.CourseSections.AsNoTracking().OrderBy(x => x.Course.SubjectCode).ThenBy(x => x.Course.CatalogNumber).ThenBy(x => x.SectionNumber).Select(x => new AdminSectionResponse(x.Id, x.Course.SubjectCode + " " + x.Course.CatalogNumber, x.Course.Title, x.SectionNumber, x.AcademicTerm.Name, x.AvailablePositions, x.IsActive, db.FacultySectionAssignments.Where(a => a.CourseSectionId == x.Id && a.IsActive).Select(a => (Guid?)a.FacultyUserId).FirstOrDefault(), db.FacultySectionAssignments.Where(a => a.CourseSectionId == x.Id && a.IsActive).Select(a => a.FacultyUser.DisplayName).FirstOrDefault())).ToListAsync(token);

    public async Task<IReadOnlyCollection<AdminPhaseResponse>> GetPhasesAsync(CancellationToken token) =>
        await db.ApplicationPhases.AsNoTracking().OrderByDescending(x => x.OpensAtUtc).Select(x => new AdminPhaseResponse(x.Id, x.Name, x.Program, x.AcademicTerm.Name, x.OpensAtUtc, x.ClosesAtUtc, x.IsActive)).ToListAsync(token);

    public async Task<IReadOnlyCollection<AdminUserResponse>> GetUsersAsync(CancellationToken token) =>
        await db.Users.AsNoTracking().OrderBy(x => x.DisplayName).Select(x => new AdminUserResponse(x.Id, x.DisplayName, x.Email, x.IsActive, x.UserRoles.Select(r => r.Role.Name).OrderBy(n => n).ToArray())).ToListAsync(token);

    public async Task<IReadOnlyCollection<AdminSettingResponse>> GetSettingsAsync(CancellationToken token) =>
        await db.SystemSettings.AsNoTracking().OrderBy(x => x.Key).Select(x => new AdminSettingResponse(x.Key, x.Value, x.Description, x.IsDevelopmentOnly, x.UpdatedAtUtc)).ToListAsync(token);

    public async Task<IReadOnlyCollection<AdminAuditResponse>> GetAuditAsync(CancellationToken token) =>
        await db.AuditLogs.AsNoTracking().OrderByDescending(x => x.OccurredAtUtc).Take(200).Select(x => new AdminAuditResponse(x.Id, x.OccurredAtUtc, x.ActorUserId, x.Action, x.EntityType, x.EntityReference, x.Result, x.CorrelationId)).ToListAsync(token);
    public async Task<IReadOnlyCollection<EmailDeliveryResponse>> GetEmailDeliveriesAsync(CancellationToken token) => await db.EmailOutboxMessages.AsNoTracking().OrderByDescending(x => x.CreatedAtUtc).Take(200).Select(x => new EmailDeliveryResponse(x.Id, x.Recipient, x.Subject, x.State.ToString(), x.AttemptCount, x.CreatedAtUtc, x.SentAtUtc, x.LastError, x.CorrelationId)).ToListAsync(token);

    public async Task<bool> AssignFacultyAsync(Guid sectionId, AssignFacultyRequest request, Guid actorId, string correlationId, CancellationToken token)
    {
        if (!await db.CourseSections.AnyAsync(x => x.Id == sectionId, token)) return false;
        if (request.FacultyUserId.HasValue && !await db.UserRoles.AnyAsync(x => x.UserId == request.FacultyUserId && x.User.IsActive && x.Role.NormalizedName == "FACULTY", token)) throw new ArgumentException("The selected user is not active faculty.");
        var now = DateTimeOffset.UtcNow;
        var current = await db.FacultySectionAssignments.Where(x => x.CourseSectionId == sectionId && x.IsActive).ToListAsync(token);
        foreach (var assignment in current) { assignment.IsActive = false; assignment.UpdatedAtUtc = now; }
        if (request.FacultyUserId.HasValue) db.FacultySectionAssignments.Add(new FacultySectionAssignment { FacultyUserId = request.FacultyUserId.Value, CourseSectionId = sectionId, CreatedAtUtc = now, UpdatedAtUtc = now });
        Audit(actorId, "FacultyAssignmentUpdated", "CourseSection", sectionId.ToString(), correlationId);
        await db.SaveChangesAsync(token); return true;
    }

    public async Task<bool> UpdateSectionAsync(Guid sectionId, UpdateSectionRequest request, Guid actorId, string correlationId, CancellationToken token)
    {
        if (request.AvailablePositions is < 0) throw new ArgumentException("Available positions cannot be negative.");
        var section = await db.CourseSections.FindAsync([sectionId], token); if (section is null) return false;
        section.IsActive = request.IsActive; section.AvailablePositions = request.AvailablePositions; section.UpdatedAtUtc = DateTimeOffset.UtcNow;
        Audit(actorId, "SectionUpdated", "CourseSection", sectionId.ToString(), correlationId); await db.SaveChangesAsync(token); return true;
    }

    public async Task<bool> UpdatePhaseAsync(Guid phaseId, UpdatePhaseRequest request, Guid actorId, string correlationId, CancellationToken token)
    {
        if (request.OpensAtUtc >= request.ClosesAtUtc) throw new ArgumentException("The phase opening time must precede its closing time.");
        var phase = await db.ApplicationPhases.FindAsync([phaseId], token); if (phase is null) return false;
        var overlaps = await db.ApplicationPhases.AnyAsync(x => x.Id != phaseId && x.IsActive && request.IsActive && x.AcademicTermId == phase.AcademicTermId && x.Program == phase.Program && request.OpensAtUtc < x.ClosesAtUtc && request.ClosesAtUtc > x.OpensAtUtc, token);
        if (overlaps) throw new AdministrationConflictException("An active phase already overlaps these dates for the same term and program.");
        phase.OpensAtUtc = request.OpensAtUtc; phase.ClosesAtUtc = request.ClosesAtUtc; phase.IsActive = request.IsActive; phase.UpdatedAtUtc = DateTimeOffset.UtcNow;
        Audit(actorId, "ApplicationPhaseUpdated", "ApplicationPhase", phaseId.ToString(), correlationId); await db.SaveChangesAsync(token); return true;
    }

    public async Task<bool> UpdateUserAsync(Guid userId, UpdateUserRequest request, Guid actorId, string correlationId, CancellationToken token)
    {
        var allowed = new HashSet<string>(["Applicant", "Faculty", "Administrator"], StringComparer.OrdinalIgnoreCase);
        if (request.Roles.Count == 0 || request.Roles.Any(x => !allowed.Contains(x))) throw new ArgumentException("At least one valid role is required.");
        var user = await db.Users.Include(x => x.UserRoles).ThenInclude(x => x.Role).SingleOrDefaultAsync(x => x.Id == userId, token); if (user is null) return false;
        var removesAdmin = user.UserRoles.Any(x => x.Role.NormalizedName == "ADMINISTRATOR") && (!request.IsActive || !request.Roles.Contains("Administrator", StringComparer.OrdinalIgnoreCase));
        if (removesAdmin && await db.UserRoles.CountAsync(x => x.Role.NormalizedName == "ADMINISTRATOR" && x.User.IsActive, token) <= 1) throw new AdministrationConflictException("The final active administrator cannot be removed or disabled.");
        var roles = await db.Roles.ToListAsync(token); var requested = request.Roles.Select(x => x.ToUpperInvariant()).ToHashSet();
        db.UserRoles.RemoveRange(user.UserRoles.Where(x => !requested.Contains(x.Role.NormalizedName)));
        foreach (var role in roles.Where(x => requested.Contains(x.NormalizedName) && user.UserRoles.All(ur => ur.RoleId != x.Id))) db.UserRoles.Add(new UserRole { UserId = userId, RoleId = role.Id, AssignedAtUtc = DateTimeOffset.UtcNow, AssignedByUserId = actorId });
        user.IsActive = request.IsActive; user.UpdatedAtUtc = DateTimeOffset.UtcNow;
        Audit(actorId, "UserAccessUpdated", "User", userId.ToString(), correlationId); await db.SaveChangesAsync(token); return true;
    }

    public async Task<bool> UpdateSettingAsync(string key, UpdateSettingRequest request, Guid actorId, string correlationId, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(request.Value) || request.Value.Length > 2000) throw new ArgumentException("A setting value of at most 2,000 characters is required.");
        var setting = await db.SystemSettings.SingleOrDefaultAsync(x => x.Key == key, token); if (setting is null) return false;
        var value = request.Value.Trim();
        if (key == "Applications.MaximumSectionChoices" && (!int.TryParse(value, out var choices) || choices is < 1 or > 10)) throw new ArgumentException("Maximum course selections must be between 1 and 10.");
        if (key == "Documents.MaximumMegabytes" && (!int.TryParse(value, out var megabytes) || megabytes is < 1 or > 50)) throw new ArgumentException("Maximum upload size must be between 1 and 50 MB.");
        if (key == "Development.ShowUserSwitcher" && !bool.TryParse(value, out _)) throw new ArgumentException("The development user switcher must be on or off.");
        setting.Value = value; setting.UpdatedAtUtc = DateTimeOffset.UtcNow;
        Audit(actorId, "SystemSettingUpdated", "SystemSetting", key, correlationId); await db.SaveChangesAsync(token); return true;
    }

    private void Audit(Guid actorId, string action, string entityType, string reference, string correlationId) => db.AuditLogs.Add(new AuditLog { ActorUserId = actorId, OccurredAtUtc = DateTimeOffset.UtcNow, Action = action, EntityType = entityType, EntityReference = reference, Result = "Succeeded", CorrelationId = correlationId });
}
