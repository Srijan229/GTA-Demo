using Gta.Application.Infrastructure.Persistence;
using Gta.Application.Application.Profiles;
using Gta.Application.Infrastructure.Profiles;
using Gta.Application.Application.Documents;
using Gta.Application.Infrastructure.Documents;
using Gta.Application.Application.Applications;
using Gta.Application.Infrastructure.Applications;
using Gta.Application.Application.Faculty;
using Gta.Application.Infrastructure.Faculty;
using Gta.Application.Application.Administration;
using Gta.Application.Infrastructure.Administration;
using Gta.Application.Application.Notifications;
using Gta.Application.Infrastructure.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gta.Application.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("GtaDatabase")
            ?? throw new InvalidOperationException("Connection string 'GtaDatabase' is required.");

        var databaseProvider = configuration["Database:Provider"] ?? "MySql";
        services.AddDbContext<GtaDbContext>(options =>
        {
            if (databaseProvider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlite(connectionString);
                return;
            }

            if (!databaseProvider.Equals("MySql", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Unsupported database provider '{databaseProvider}'.");
            }

            options.UseMySQL(connectionString, mysql =>
                mysql.MigrationsHistoryTable("gta___EFMigrationsHistory"));
        });

        services.AddScoped<DevelopmentDataSeeder>();
        services.AddScoped<IApplicantProfileService, ApplicantProfileService>();
        services.AddSingleton<IDocumentStorage, LocalDocumentStorage>();
        services.AddScoped<IApplicantDocumentService, ApplicantDocumentService>();
        services.AddScoped<IApplicationSubmissionService, ApplicationSubmissionService>();
        services.AddScoped<IFacultyReviewService, FacultyReviewService>();
        services.AddScoped<IAdministrationService, AdministrationService>();
        services.AddScoped<IPlacementService, PlacementService>();
        services.AddScoped<ISectionImportService, SectionImportService>();
        services.AddScoped<IEmailOutbox, EmailOutbox>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddHostedService<EmailOutboxProcessor>();
        services.AddHealthChecks().AddDbContextCheck<GtaDbContext>("database");
        return services;
    }
}
