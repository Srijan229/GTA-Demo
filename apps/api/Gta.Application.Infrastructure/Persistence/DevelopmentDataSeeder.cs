using Gta.Application.Domain.Applications;
using Gta.Application.Domain.Identity;
using Gta.Application.Domain.Profiles;
using Gta.Application.Domain.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Gta.Application.Infrastructure.Persistence;

public static class DevelopmentData
{
    public static readonly Guid ApplicantUserId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    public static readonly Guid FacultyUserId = Guid.Parse("10000000-0000-0000-0000-000000000002");
    public static readonly Guid AdministratorUserId = Guid.Parse("10000000-0000-0000-0000-000000000003");
}

public sealed class DevelopmentDataSeeder(GtaDbContext dbContext)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await dbContext.Users.AnyAsync(cancellationToken))
        {
            await EnsureSettingsAsync(cancellationToken);
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var applicantRole = Role("20000000-0000-0000-0000-000000000001", "Applicant");
        var facultyRole = Role("20000000-0000-0000-0000-000000000002", "Faculty");
        var administratorRole = Role("20000000-0000-0000-0000-000000000003", "Administrator");

        var applicant = User(DevelopmentData.ApplicantUserId, "Alex Applicant", "alex.applicant@example.test", "G00000001", now);
        var faculty = User(DevelopmentData.FacultyUserId, "Dr. Morgan Lee", "morgan.lee@example.test", "G00000002", now);
        var administrator = User(DevelopmentData.AdministratorUserId, "Jordan Administrator", "jordan.admin@example.test", "G00000003", now);

        dbContext.AddRange(applicantRole, facultyRole, administratorRole, applicant, faculty, administrator);
        dbContext.UserRoles.AddRange(
            UserRole(applicant, applicantRole, now),
            UserRole(faculty, facultyRole, now),
            UserRole(administrator, administratorRole, now));

        var profile = new ApplicantProfile
        {
            Id = Guid.Parse("30000000-0000-0000-0000-000000000001"),
            User = applicant,
            PreferredName = "Alex",
            Program = "Master's",
            Degree = "MS",
            Major = "Information Systems",
            ExpectedGraduationTerm = "Spring",
            ExpectedGraduationYear = 2027,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };

        var term = new AcademicTerm
        {
            Id = Guid.Parse("40000000-0000-0000-0000-000000000001"),
            Code = "2026FA",
            Name = "Fall 2026",
            StartsOn = new DateOnly(2026, 8, 24),
            EndsOn = new DateOnly(2026, 12, 16),
        };
        var course = new Course
        {
            Id = Guid.Parse("50000000-0000-0000-0000-000000000001"),
            SubjectCode = "AIT",
            CatalogNumber = "580",
            Title = "Analytics: Big Data to Information",
        };
        var section = new CourseSection
        {
            Id = Guid.Parse("60000000-0000-0000-0000-000000000001"),
            Course = course,
            AcademicTerm = term,
            SectionNumber = "001",
            Schedule = "Monday 4:30 PM–7:10 PM",
            DeliveryMethod = "In person",
            AvailablePositions = 1,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };
        var phase = new ApplicationPhase
        {
            Id = Guid.Parse("70000000-0000-0000-0000-000000000001"),
            AcademicTerm = term,
            Name = "Fall 2026 Master's GTA Applications",
            Program = "Master's",
            OpensAtUtc = new DateTimeOffset(2026, 8, 1, 0, 0, 0, TimeSpan.Zero),
            ClosesAtUtc = new DateTimeOffset(2026, 9, 15, 23, 59, 59, TimeSpan.Zero),
            IsActive = true,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };
        var facultyAssignment = new FacultySectionAssignment
        {
            Id = Guid.Parse("80000000-0000-0000-0000-000000000001"),
            FacultyUser = faculty,
            CourseSection = section,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };

        dbContext.AddRange(profile, term, course, section, phase, facultyAssignment);
        dbContext.SystemSettings.AddRange(
            new SystemSetting { Key = "Applications.MaximumSectionChoices", Value = "5", Description = "Maximum number of sections an applicant may select.", CreatedAtUtc = now, UpdatedAtUtc = now },
            new SystemSetting { Key = "Documents.MaximumMegabytes", Value = "10", Description = "Maximum local document upload size in megabytes.", CreatedAtUtc = now, UpdatedAtUtc = now },
            new SystemSetting { Key = "Development.ShowUserSwitcher", Value = "true", Description = "Shows the local development identity selector.", IsDevelopmentOnly = true, CreatedAtUtc = now, UpdatedAtUtc = now });
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureSettingsAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var settings = new[]
        {
            new SystemSetting { Key = "Applications.MaximumSectionChoices", Value = "5", Description = "Maximum number of sections an applicant may select.", CreatedAtUtc = now, UpdatedAtUtc = now },
            new SystemSetting { Key = "Documents.MaximumMegabytes", Value = "10", Description = "Maximum local document upload size in megabytes.", CreatedAtUtc = now, UpdatedAtUtc = now },
            new SystemSetting { Key = "Development.ShowUserSwitcher", Value = "true", Description = "Shows the local development identity selector.", IsDevelopmentOnly = true, CreatedAtUtc = now, UpdatedAtUtc = now },
        };
        var keys = await dbContext.SystemSettings.Select(x => x.Key).ToListAsync(cancellationToken);
        dbContext.SystemSettings.AddRange(settings.Where(x => !keys.Contains(x.Key)));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static Role Role(string id, string name) => new()
    {
        Id = Guid.Parse(id),
        Name = name,
        NormalizedName = name.ToUpperInvariant(),
    };

    private static User User(Guid id, string name, string email, string universityId, DateTimeOffset now) => new()
    {
        Id = id,
        DisplayName = name,
        Email = email,
        NormalizedEmail = email.ToUpperInvariant(),
        UniversityId = universityId,
        CreatedAtUtc = now,
        UpdatedAtUtc = now,
    };

    private static UserRole UserRole(User user, Role role, DateTimeOffset now) => new()
    {
        User = user,
        Role = role,
        AssignedAtUtc = now,
    };
}
