using Gta.Application.Contracts.Administration;

namespace Gta.Application.Application.Administration;

public interface IPlacementService
{
    Task<IReadOnlyCollection<PlacementCandidateResponse>> GetCandidatesAsync(CancellationToken token);
    Task<PlacementActionResponse?> UpdateAsync(Guid choiceId, UpdatePlacementRequest request, Guid actorId, string correlationId, CancellationToken token);
}
