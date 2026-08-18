using System.Security.Claims;
using Gta.Application.Application.Authorization;
using Gta.Application.Application.Faculty;
using Gta.Application.Contracts.Faculty;

namespace Gta.Application.Api.Faculty;

public static class FacultyEndpoints
{
    public static IEndpointRouteBuilder MapFacultyEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/faculty").RequireAuthorization(AuthorizationPolicies.Faculty).WithTags("Faculty review");
        group.MapGet("/sections", async (ClaimsPrincipal principal, IFacultyReviewService service, CancellationToken token) => Results.Ok(await service.GetSectionsAsync(UserId(principal), token)));
        group.MapGet("/applications", async (ClaimsPrincipal principal, IFacultyReviewService service, CancellationToken token) => Results.Ok(await service.GetApplicationsAsync(UserId(principal), token)));
        group.MapGet("/interviews", async (ClaimsPrincipal principal, IFacultyReviewService service, CancellationToken token) => Results.Ok(await service.GetInterviewQueueAsync(UserId(principal), token)));
        group.MapGet("/applications/{choiceId:guid}", async (Guid choiceId, ClaimsPrincipal principal, IFacultyReviewService service, CancellationToken token) =>
            await service.GetReviewAsync(UserId(principal), choiceId, token) is { } review ? Results.Ok(review) : Results.NotFound());
        group.MapPost("/applications/{choiceId:guid}/actions", async (Guid choiceId, RecordFacultyActionRequest request, ClaimsPrincipal principal, HttpContext context, IFacultyReviewService service, CancellationToken token) =>
            await service.RecordActionAsync(UserId(principal), choiceId, request, context.TraceIdentifier, token) is { } action ? Results.Ok(action) : Results.NotFound());
        group.MapGet("/documents/{documentId:guid}/content", async (Guid documentId, ClaimsPrincipal principal, IFacultyReviewService service, CancellationToken token) =>
            await service.DownloadDocumentAsync(UserId(principal), documentId, token) is { } download
                ? Results.File(download.Content, download.MediaType, download.FileName, enableRangeProcessing: true)
                : Results.NotFound());
        return endpoints;
    }

    private static Guid UserId(ClaimsPrincipal principal) => Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
