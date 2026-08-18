using System.Security.Claims;
using Gta.Application.Contracts.Authentication;

namespace Gta.Application.Api.Authentication;

public static class CurrentUserEndpoints
{
    public static IEndpointRouteBuilder MapCurrentUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/v1/auth/me", (ClaimsPrincipal principal) =>
        {
            var id = Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var roles = principal.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToArray();
            return Results.Ok(new CurrentUserResponse(
                id,
                principal.FindFirstValue(ClaimTypes.Name)!,
                principal.FindFirstValue(ClaimTypes.Email)!,
                roles));
        }).RequireAuthorization().WithTags("Authentication");

        return endpoints;
    }
}
