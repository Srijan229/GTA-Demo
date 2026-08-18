using System.Security.Claims;
using Gta.Application.Application.Administration;
using Gta.Application.Application.Authorization;
using Gta.Application.Contracts.Administration;

namespace Gta.Application.Api.Administration;

public static class AdministrationEndpoints
{
    public static IEndpointRouteBuilder MapAdministrationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/admin").RequireAuthorization(AuthorizationPolicies.Administrator).WithTags("Administration");
        group.MapGet("/dashboard", async (IAdministrationService service, CancellationToken token) => Results.Ok(await service.GetDashboardAsync(token)));
        group.MapGet("/applications", async (IAdministrationService service, CancellationToken token) => Results.Ok(await service.GetApplicationsAsync(token)));
        group.MapGet("/applicants", async (IAdministrationService service, CancellationToken token) => Results.Ok(await service.GetApplicantsAsync(token)));
        group.MapGet("/sections", async (IAdministrationService service, CancellationToken token) => Results.Ok(await service.GetSectionsAsync(token)));
        group.MapGet("/phases", async (IAdministrationService service, CancellationToken token) => Results.Ok(await service.GetPhasesAsync(token)));
        group.MapGet("/users", async (IAdministrationService service, CancellationToken token) => Results.Ok(await service.GetUsersAsync(token)));
        group.MapGet("/settings", async (IAdministrationService service, CancellationToken token) => Results.Ok(await service.GetSettingsAsync(token)));
        group.MapGet("/audit", async (IAdministrationService service, CancellationToken token) => Results.Ok(await service.GetAuditAsync(token)));
        group.MapGet("/email-deliveries", async (IAdministrationService service, CancellationToken token) => Results.Ok(await service.GetEmailDeliveriesAsync(token)));
        group.MapGet("/placements", async (IPlacementService service, CancellationToken token) => Results.Ok(await service.GetCandidatesAsync(token)));
        group.MapGet("/section-imports", async (ISectionImportService service, CancellationToken token) => Results.Ok(await service.GetHistoryAsync(token)));
        group.MapGet("/section-imports/template", () => Results.Text("TermCode,TermName,TermStart,TermEnd,SubjectCode,CatalogNumber,CourseTitle,SectionNumber,Schedule,DeliveryMethod,AvailablePositions,IsActive\n2027SP,Spring 2027,2027-01-19,2027-05-12,AIT,580,Analytics Big Data to Information,001,Monday 4:30 PM-7:10 PM,In person,1,true\n", "text/csv", System.Text.Encoding.UTF8)).WithName("DownloadSectionImportTemplate");
        group.MapPost("/section-imports/preview", async (IFormFile file, ISectionImportService service, CancellationToken token) => { await using var stream = file.OpenReadStream(); return Results.Ok(await service.PreviewAsync(stream, token)); }).DisableAntiforgery();
        group.MapPost("/section-imports", async (IFormFile file, ClaimsPrincipal principal, HttpContext context, ISectionImportService service, CancellationToken token) => { await using var stream = file.OpenReadStream(); return Results.Ok(await service.ImportAsync(stream, file.FileName, UserId(principal), context.TraceIdentifier, token)); }).DisableAntiforgery();
        group.MapPut("/placements/{choiceId:guid}", async (Guid choiceId, UpdatePlacementRequest request, ClaimsPrincipal principal, HttpContext context, IPlacementService service, CancellationToken token) =>
            await service.UpdateAsync(choiceId, request, UserId(principal), context.TraceIdentifier, token) is { } result ? Results.Ok(result) : Results.NotFound());
        group.MapPut("/sections/{id:guid}/faculty", async (Guid id, AssignFacultyRequest request, ClaimsPrincipal principal, HttpContext context, IAdministrationService service, CancellationToken token) => Result(await service.AssignFacultyAsync(id, request, UserId(principal), context.TraceIdentifier, token)));
        group.MapPut("/sections/{id:guid}", async (Guid id, UpdateSectionRequest request, ClaimsPrincipal principal, HttpContext context, IAdministrationService service, CancellationToken token) => Result(await service.UpdateSectionAsync(id, request, UserId(principal), context.TraceIdentifier, token)));
        group.MapPut("/phases/{id:guid}", async (Guid id, UpdatePhaseRequest request, ClaimsPrincipal principal, HttpContext context, IAdministrationService service, CancellationToken token) => Result(await service.UpdatePhaseAsync(id, request, UserId(principal), context.TraceIdentifier, token)));
        group.MapPut("/users/{id:guid}", async (Guid id, UpdateUserRequest request, ClaimsPrincipal principal, HttpContext context, IAdministrationService service, CancellationToken token) => Result(await service.UpdateUserAsync(id, request, UserId(principal), context.TraceIdentifier, token)));
        group.MapPut("/settings/{key}", async (string key, UpdateSettingRequest request, ClaimsPrincipal principal, HttpContext context, IAdministrationService service, CancellationToken token) => Result(await service.UpdateSettingAsync(key, request, UserId(principal), context.TraceIdentifier, token)));
        return endpoints;
    }

    private static IResult Result(bool found) => found ? Results.NoContent() : Results.NotFound();
    private static Guid UserId(ClaimsPrincipal principal) => Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
