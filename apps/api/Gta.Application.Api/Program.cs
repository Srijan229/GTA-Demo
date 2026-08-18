using Gta.Application.Infrastructure;
using Gta.Application.Api.Authentication;
using Gta.Application.Infrastructure.Persistence;
using Gta.Application.Application.Authorization;
using Gta.Application.Api.Profiles;
using Gta.Application.Api.Documents;
using Gta.Application.Api.Applications;
using Gta.Application.Api.Faculty;
using Gta.Application.Api.Administration;
using Gta.Application.Application.Administration;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Gta.Application.Application.Applications;
using Gta.Application.Application.Faculty;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.FileProviders;
using System.Net;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseWebRoot("wwwroot");

builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options => options.IncludeScopes = true);

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["correlationId"] = context.HttpContext.TraceIdentifier;
    };
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplicationAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddDemoAccessGate(builder.Configuration);
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownProxies.Add(IPAddress.Loopback);
    options.KnownProxies.Add(IPAddress.IPv6Loopback);
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseForwardedHeaders();
app.UseMiddleware<DemoAccessGateMiddleware>();
app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.ContentRootPath, "wwwroot")),
});
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var databaseProvider = builder.Configuration["Database:Provider"] ?? "MySql";
if (databaseProvider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
{
    await using var databaseScope = app.Services.CreateAsyncScope();
    await databaseScope.ServiceProvider.GetRequiredService<GtaDbContext>().Database.EnsureCreatedAsync();
}
else if (builder.Configuration.GetValue<bool>("Database:ApplyMigrations"))
{
    await using var migrationScope = app.Services.CreateAsyncScope();
    await migrationScope.ServiceProvider.GetRequiredService<GtaDbContext>().Database.MigrateAsync();
}

app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready");
if (app.Environment.IsDevelopment())
{
    app.MapDevelopmentAuthentication();
}
app.MapCurrentUserEndpoints();
app.MapProfileEndpoints();
app.MapDocumentEndpoints();
app.MapApplicationEndpoints();
app.MapFacultyEndpoints();
app.MapAdministrationEndpoints();
app.MapGet("/api/v1/applicant/access", () => Results.NoContent())
    .RequireAuthorization(AuthorizationPolicies.Applicant)
    .WithTags("Authorization");
app.MapGet("/api/v1/faculty/access", () => Results.NoContent())
    .RequireAuthorization(AuthorizationPolicies.Faculty)
    .WithTags("Authorization");
app.MapGet("/api/v1/admin/access", () => Results.NoContent())
    .RequireAuthorization(AuthorizationPolicies.Administrator)
    .WithTags("Authorization");
app.MapGet("/api/v1/system/info", (IHostEnvironment environment) => Results.Ok(new
{
    service = "GTA Application API",
    environment = environment.EnvironmentName,
    utcNow = DateTimeOffset.UtcNow,
})).WithName("GetSystemInfo");

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<DevelopmentDataSeeder>().SeedAsync();
}

app.Map("/api/{**path}", () => Results.NotFound());
app.MapFallback(() => Results.File(
    Path.Combine(app.Environment.ContentRootPath, "wwwroot", "index.html"),
    "text/html"));

app.Run();

public partial class Program;

internal sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var isValidationError = exception is ArgumentException;
        var isConcurrencyConflict = exception is DbUpdateConcurrencyException or ApplicationConflictException or FacultyActionConflictException or AdministrationConflictException;
        var isReadinessError = exception is ApplicationReadinessException;
        var status = isValidationError
            ? StatusCodes.Status400BadRequest
            : isReadinessError
                ? StatusCodes.Status422UnprocessableEntity
            : isConcurrencyConflict
                ? StatusCodes.Status409Conflict
                : StatusCodes.Status500InternalServerError;
        if (isValidationError || isConcurrencyConflict || isReadinessError)
        {
            logger.LogInformation("Request validation failed. CorrelationId: {CorrelationId}", httpContext.TraceIdentifier);
        }
        else
        {
            logger.LogError(exception, "Unhandled request failure. CorrelationId: {CorrelationId}", httpContext.TraceIdentifier);
        }
        httpContext.Response.StatusCode = status;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new()
            {
                Status = status,
                Title = isValidationError ? "The request is invalid." : isReadinessError ? "The application is not ready to submit." : isConcurrencyConflict ? "The request conflicts with existing data." : "An unexpected error occurred.",
                Detail = isValidationError || isReadinessError || exception is ApplicationConflictException or FacultyActionConflictException or AdministrationConflictException ? exception.Message : isConcurrencyConflict ? "Reload the current record and try again." : "The request could not be completed. Use the correlation identifier when requesting support.",
            },
            Exception = exception,
        });
    }
}
