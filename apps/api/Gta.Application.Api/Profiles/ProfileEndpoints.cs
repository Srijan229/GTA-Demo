using System.Security.Claims;
using Gta.Application.Application.Authorization;
using Gta.Application.Application.Profiles;
using Gta.Application.Contracts.Profiles;

namespace Gta.Application.Api.Profiles;

public static class ProfileEndpoints
{
    public static IEndpointRouteBuilder MapProfileEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/profile/me")
            .RequireAuthorization(AuthorizationPolicies.Applicant)
            .WithTags("Applicant profile");

        group.MapGet("/", async (ClaimsPrincipal principal, IApplicantProfileService service, CancellationToken token) =>
            await service.GetAsync(UserId(principal), token) is { } profile ? Results.Ok(profile) : Results.NotFound());

        group.MapGet("/completion", async (ClaimsPrincipal principal, IApplicantProfileService service, CancellationToken token) =>
            await service.GetCompletionAsync(UserId(principal), token) is { } completion ? Results.Ok(completion) : Results.NotFound());

        group.MapPut("/", async (ClaimsPrincipal principal, UpdateApplicantProfileRequest request, IApplicantProfileService service, CancellationToken token) =>
            Results.Ok(await service.UpdateAsync(UserId(principal), request, token)));

        return endpoints;
    }

    private static Guid UserId(ClaimsPrincipal principal) => Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
