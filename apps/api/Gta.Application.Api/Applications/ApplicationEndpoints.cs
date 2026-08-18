using System.Security.Claims;
using Gta.Application.Application.Applications;
using Gta.Application.Application.Authorization;
using Gta.Application.Contracts.Applications;

namespace Gta.Application.Api.Applications;

public static class ApplicationEndpoints
{
    public static IEndpointRouteBuilder MapApplicationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/applications").RequireAuthorization(AuthorizationPolicies.Applicant).WithTags("Applicant applications");
        group.MapGet("/available-sections", async (ClaimsPrincipal principal, IApplicationSubmissionService service, CancellationToken token) =>
            Results.Ok(await service.GetAvailableSectionsAsync(UserId(principal), token)));
        group.MapGet("/configuration", async (IApplicationSubmissionService service, CancellationToken token) => Results.Ok(await service.GetConfigurationAsync(token)));
        group.MapGet("/mine", async (ClaimsPrincipal principal, IApplicationSubmissionService service, CancellationToken token) =>
            Results.Ok(await service.GetMineAsync(UserId(principal), token)));
        group.MapGet("/mine/{applicationId:guid}", async (Guid applicationId, ClaimsPrincipal principal, IApplicationSubmissionService service, CancellationToken token) =>
            await service.GetMineAsync(UserId(principal), applicationId, token) is { } application ? Results.Ok(application) : Results.NotFound());
        group.MapPost("/mine/{applicationId:guid}/withdraw", async (Guid applicationId, WithdrawApplicationRequest request, ClaimsPrincipal principal, HttpContext context, IApplicationSubmissionService service, CancellationToken token) =>
            await service.WithdrawAsync(UserId(principal), applicationId, request, context.TraceIdentifier, token) is { } application ? Results.Ok(application) : Results.NotFound());
        group.MapPost("/", async (ClaimsPrincipal principal, SubmitApplicationRequest request, IApplicationSubmissionService service, CancellationToken token) =>
            Results.Created("/api/v1/applications/mine", await service.SubmitAsync(UserId(principal), request, token)));
        return endpoints;
    }

    private static Guid UserId(ClaimsPrincipal principal) => Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
