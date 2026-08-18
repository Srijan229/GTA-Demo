using Gta.Application.Contracts.Applications;

namespace Gta.Application.Application.Applications;

public interface IApplicationSubmissionService
{
    Task<IReadOnlyCollection<AvailableSectionResponse>> GetAvailableSectionsAsync(Guid userId, CancellationToken cancellationToken);
    Task<ApplicationConfigurationResponse> GetConfigurationAsync(CancellationToken cancellationToken);
    Task<ApplicationResponse> SubmitAsync(Guid userId, SubmitApplicationRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ApplicationResponse>> GetMineAsync(Guid userId, CancellationToken cancellationToken);
    Task<ApplicationDetailResponse?> GetMineAsync(Guid userId, Guid applicationId, CancellationToken cancellationToken);
    Task<ApplicationDetailResponse?> WithdrawAsync(Guid userId, Guid applicationId, WithdrawApplicationRequest request, string correlationId, CancellationToken cancellationToken);
}

public sealed class ApplicationConflictException(string message) : Exception(message);
public sealed class ApplicationReadinessException(string message) : Exception(message);
