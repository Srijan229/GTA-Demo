using Gta.Application.Contracts.Administration;

namespace Gta.Application.Application.Administration;

public interface IAdministrationService
{
    Task<AdminDashboardResponse> GetDashboardAsync(CancellationToken token);
    Task<IReadOnlyCollection<AdminApplicationResponse>> GetApplicationsAsync(CancellationToken token);
    Task<IReadOnlyCollection<AdminApplicantResponse>> GetApplicantsAsync(CancellationToken token);
    Task<IReadOnlyCollection<AdminSectionResponse>> GetSectionsAsync(CancellationToken token);
    Task<IReadOnlyCollection<AdminPhaseResponse>> GetPhasesAsync(CancellationToken token);
    Task<IReadOnlyCollection<AdminUserResponse>> GetUsersAsync(CancellationToken token);
    Task<IReadOnlyCollection<AdminSettingResponse>> GetSettingsAsync(CancellationToken token);
    Task<IReadOnlyCollection<AdminAuditResponse>> GetAuditAsync(CancellationToken token);
    Task<IReadOnlyCollection<EmailDeliveryResponse>> GetEmailDeliveriesAsync(CancellationToken token);
    Task<bool> AssignFacultyAsync(Guid sectionId, AssignFacultyRequest request, Guid actorId, string correlationId, CancellationToken token);
    Task<bool> UpdateSectionAsync(Guid sectionId, UpdateSectionRequest request, Guid actorId, string correlationId, CancellationToken token);
    Task<bool> UpdatePhaseAsync(Guid phaseId, UpdatePhaseRequest request, Guid actorId, string correlationId, CancellationToken token);
    Task<bool> UpdateUserAsync(Guid userId, UpdateUserRequest request, Guid actorId, string correlationId, CancellationToken token);
    Task<bool> UpdateSettingAsync(string key, UpdateSettingRequest request, Guid actorId, string correlationId, CancellationToken token);
}

public sealed class AdministrationConflictException(string message) : Exception(message);
