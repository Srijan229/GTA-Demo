using System.Security.Claims;
using Gta.Application.Application.Authorization;
using Gta.Application.Contracts.Authentication;
using Gta.Application.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace Gta.Application.Api.Authentication;

public static class DevelopmentAuthentication
{
    public const string Scheme = "DevelopmentCookie";

    public static IServiceCollection AddApplicationAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var enabled = configuration.GetValue<bool>("DevelopmentAuthentication:Enabled");
        if (enabled && !environment.IsDevelopment())
        {
            throw new InvalidOperationException("Development authentication cannot be enabled outside Development.");
        }

        if (enabled)
        {
            services.AddAuthentication(Scheme).AddCookie(Scheme, options => ConfigureCookie(options));
        }
        else
        {
            services.AddAuthentication(FailClosedAuthenticationHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, FailClosedAuthenticationHandler>(FailClosedAuthenticationHandler.SchemeName, _ => { });
        }

        services.AddAuthorizationBuilder()
            .AddPolicy(AuthorizationPolicies.Applicant, policy => policy.RequireRole(ApplicationRoles.Applicant))
            .AddPolicy(AuthorizationPolicies.Faculty, policy => policy.RequireRole(ApplicationRoles.Faculty))
            .AddPolicy(AuthorizationPolicies.Administrator, policy => policy.RequireRole(ApplicationRoles.Administrator));
        return services;
    }

    private static void ConfigureCookie(CookieAuthenticationOptions options)
    {
        options.Cookie.Name = "gta.development.session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    }

    public static RouteGroupBuilder MapDevelopmentAuthentication(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/development").WithTags("Development authentication");

        group.MapGet("/users", async (GtaDbContext db, CancellationToken cancellationToken) =>
        {
            var users = await db.Users
                .AsNoTracking()
                .OrderBy(user => user.DisplayName)
                .Select(user => new DevelopmentUserResponse(
                    user.Id,
                    user.DisplayName,
                    user.Email,
                    user.UserRoles.Select(userRole => userRole.Role.Name).ToArray(),
                    user.UserRoles.Any(userRole => userRole.Role.Name == ApplicationRoles.Applicant)
                        ? "Complete a profile and submit applications."
                        : user.UserRoles.Any(userRole => userRole.Role.Name == ApplicationRoles.Faculty)
                            ? "Review applicants for assigned sections."
                            : "Manage application operations and configuration."))
                .ToListAsync(cancellationToken);
            return Results.Ok(users);
        }).AllowAnonymous();

        group.MapPost("/session/{userId:guid}", async (
            Guid userId,
            HttpContext httpContext,
            GtaDbContext db,
            CancellationToken cancellationToken) =>
        {
            var user = await db.Users
                .AsNoTracking()
                .Include(candidate => candidate.UserRoles)
                .ThenInclude(userRole => userRole.Role)
                .SingleOrDefaultAsync(candidate => candidate.Id == userId && candidate.IsActive, cancellationToken);

            if (user is null)
            {
                return Results.NotFound();
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.DisplayName),
                new(ClaimTypes.Email, user.Email),
            };
            claims.AddRange(user.UserRoles.Select(userRole => new Claim(ClaimTypes.Role, userRole.Role.Name)));
            await httpContext.SignInAsync(Scheme, new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme)));
            return Results.NoContent();
        }).AllowAnonymous();

        group.MapDelete("/session", async (HttpContext context) =>
        {
            await context.SignOutAsync(Scheme);
            return Results.NoContent();
        }).RequireAuthorization();

        return group;
    }
}
