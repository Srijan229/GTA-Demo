using Gta.Application.Contracts.Profiles;

namespace Gta.Application.Application.Profiles;

public interface IApplicantProfileService
{
    Task<ApplicantProfileResponse?> GetAsync(Guid userId, CancellationToken cancellationToken);
    Task<ApplicantProfileResponse> UpdateAsync(Guid userId, UpdateApplicantProfileRequest request, CancellationToken cancellationToken);
    Task<ProfileCompletionResponse?> GetCompletionAsync(Guid userId, CancellationToken cancellationToken);
}
