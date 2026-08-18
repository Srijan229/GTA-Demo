using System.Security.Claims;
using Gta.Application.Application.Authorization;
using Gta.Application.Application.Documents;
using Gta.Application.Domain.Documents;

namespace Gta.Application.Api.Documents;

public static class DocumentEndpoints
{
    public static IEndpointRouteBuilder MapDocumentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/documents")
            .RequireAuthorization(AuthorizationPolicies.Applicant)
            .WithTags("Applicant documents");

        group.MapGet("/", async (ClaimsPrincipal principal, IApplicantDocumentService service, CancellationToken token) =>
            Results.Ok(await service.GetCurrentAsync(UserId(principal), token)));

        group.MapPost("/{type}", async (string type, IFormFile file, ClaimsPrincipal principal, IApplicantDocumentService service, CancellationToken token) =>
        {
            if (!Enum.TryParse<DocumentType>(type, true, out var documentType)) return Results.BadRequest();
            await using var stream = file.OpenReadStream();
            return Results.Ok(await service.UploadAsync(UserId(principal), documentType, file.FileName, file.ContentType, file.Length, stream, token));
        }).DisableAntiforgery();

        group.MapGet("/{documentId:guid}/content", async (Guid documentId, ClaimsPrincipal principal, IApplicantDocumentService service, CancellationToken token) =>
            await service.DownloadAsync(UserId(principal), documentId, token) is { } download
                ? Results.File(download.Content, download.MediaType, download.FileName, enableRangeProcessing: true)
                : Results.NotFound());

        return endpoints;
    }

    private static Guid UserId(ClaimsPrincipal principal) => Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
