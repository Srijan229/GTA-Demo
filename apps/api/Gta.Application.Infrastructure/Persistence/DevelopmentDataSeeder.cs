using Gta.Application.Domain.Applications;
using Gta.Application.Domain.Identity;
using Gta.Application.Domain.Profiles;
using Gta.Application.Domain.Configuration;
using Gta.Application.Domain.Auditing;
using Gta.Application.Domain.Notifications;
using Gta.Application.Application.Documents;
using Gta.Application.Domain.Documents;
using Microsoft.EntityFrameworkCore;

namespace Gta.Application.Infrastructure.Persistence;

public static class DevelopmentData
{
    public static readonly Guid ApplicantUserId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    public static readonly Guid FacultyUserId = Guid.Parse("10000000-0000-0000-0000-000000000002");
    public static readonly Guid AdministratorUserId = Guid.Parse("10000000-0000-0000-0000-000000000003");
}

public sealed class DevelopmentDataSeeder(GtaDbContext dbContext, IDocumentStorage documentStorage)
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
            PhoneNumber = "555-0101",
            Program = "Master's",
            Degree = "MS",
            Major = "Information Systems",
            Gpa = 3.82m,
            ExpectedGraduationTerm = "Spring",
            ExpectedGraduationYear = 2027,
            LinkedInUrl = "https://www.linkedin.com/in/alex-applicant-demo",
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
        await AddShowcaseDataAsync(applicantRole, facultyRole, applicant, faculty, administrator, profile, term, course, section, phase, now, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task AddShowcaseDataAsync(
        Role applicantRole,
        Role facultyRole,
        User alex,
        User morgan,
        User administrator,
        ApplicantProfile alexProfile,
        AcademicTerm fallTerm,
        Course analytics,
        CourseSection analyticsSection,
        ApplicationPhase fallPhase,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var priya = User(Id("10000000-0000-0000-0000-000000000004"), "Priya Shah", "priya.shah@example.test", "G00000004", now);
        var mateo = User(Id("10000000-0000-0000-0000-000000000005"), "Mateo Rivera", "mateo.rivera@example.test", "G00000005", now);
        var riley = User(Id("10000000-0000-0000-0000-000000000006"), "Riley Chen", "riley.chen@example.test", "G00000006", now);
        var casey = User(Id("10000000-0000-0000-0000-000000000007"), "Dr. Casey Nguyen", "casey.nguyen@example.test", "G00000007", now);
        var inactive = User(Id("10000000-0000-0000-0000-000000000008"), "Taylor Inactive", "taylor.inactive@example.test", "G00000008", now);
        inactive.IsActive = false;

        dbContext.AddRange(priya, mateo, riley, casey, inactive);
        dbContext.UserRoles.AddRange(
            UserRole(priya, applicantRole, now),
            UserRole(mateo, applicantRole, now),
            UserRole(riley, applicantRole, now),
            UserRole(casey, facultyRole, now),
            UserRole(inactive, applicantRole, now));

        var priyaProfile = Profile(Id("30000000-0000-0000-0000-000000000002"), priya, "Priya", "Master's", "MS", "Software Engineering", 3.94m, "Fall", 2027, now);
        var mateoProfile = Profile(Id("30000000-0000-0000-0000-000000000003"), mateo, "Mateo", "Master's", "MS", "Information Systems", 3.68m, "Spring", 2027, now);
        var rileyProfile = Profile(Id("30000000-0000-0000-0000-000000000004"), riley, "Riley", "Doctoral", "PhD", "Information Technology", 3.89m, "Spring", 2028, now);
        dbContext.AddRange(priyaProfile, mateoProfile, rileyProfile);
        dbContext.EducationRecords.AddRange(
            Education(Id("31000000-0000-0000-0000-000000000001"), alexProfile, "George Mason University", "BS", "Information Technology", new(2021, 8, 23), new(2025, 5, 15), now),
            Education(Id("31000000-0000-0000-0000-000000000002"), priyaProfile, "Virginia Tech", "BS", "Computer Science", new(2020, 8, 24), new(2024, 5, 10), now),
            Education(Id("31000000-0000-0000-0000-000000000003"), mateoProfile, "James Madison University", "BBA", "Computer Information Systems", new(2019, 8, 26), new(2023, 5, 12), now),
            Education(Id("31000000-0000-0000-0000-000000000004"), rileyProfile, "George Mason University", "MS", "Information Systems", new(2023, 8, 21), new(2025, 5, 15), now));
        dbContext.ExperienceRecords.AddRange(
            Experience(Id("32000000-0000-0000-0000-000000000001"), alexProfile, "Mason ITS", "Student Analyst", "Supported analytics dashboards and data-quality reviews.", new(2024, 1, 15), null, false, now),
            Experience(Id("32000000-0000-0000-0000-000000000002"), priyaProfile, "Demo Learning Lab", "Peer Tutor", "Tutored introductory programming and software design.", new(2023, 9, 1), new(2024, 5, 1), true, now),
            Experience(Id("32000000-0000-0000-0000-000000000003"), mateoProfile, "Demo Consulting Group", "Business Systems Intern", "Documented workflows and tested internal applications.", new(2023, 6, 1), new(2024, 8, 1), false, now),
            Experience(Id("32000000-0000-0000-0000-000000000004"), rileyProfile, "Mason Research Lab", "Graduate Research Assistant", "Conducted applied cybersecurity research and led lab sessions.", new(2025, 8, 25), null, true, now));

        var springTerm = new AcademicTerm { Id = Id("40000000-0000-0000-0000-000000000002"), Code = "2026SP", Name = "Spring 2026", StartsOn = new(2026, 1, 20), EndsOn = new(2026, 5, 13) };
        var programming = Course(Id("50000000-0000-0000-0000-000000000002"), "AIT", "512", "Programming Fundamentals");
        var security = Course(Id("50000000-0000-0000-0000-000000000003"), "ISA", "650", "Security Policy");
        var database = Course(Id("50000000-0000-0000-0000-000000000004"), "AIT", "614", "Database Management");
        var capstone = Course(Id("50000000-0000-0000-0000-000000000005"), "AIT", "690", "IT Capstone");
        var sections = new[]
        {
            Section(Id("60000000-0000-0000-0000-000000000002"), analytics, fallTerm, "002", "Wednesday 4:30 PM-7:10 PM", "Online synchronous", 2, now),
            Section(Id("60000000-0000-0000-0000-000000000003"), programming, fallTerm, "001", "Tuesday 7:20 PM-10:00 PM", "In person", 2, now),
            Section(Id("60000000-0000-0000-0000-000000000004"), security, fallTerm, "DL1", "Thursday 4:30 PM-7:10 PM", "Online synchronous", 1, now),
            Section(Id("60000000-0000-0000-0000-000000000005"), database, fallTerm, "001", "Monday 7:20 PM-10:00 PM", "In person", 2, now),
            Section(Id("60000000-0000-0000-0000-000000000006"), capstone, springTerm, "001", "Friday 10:30 AM-1:10 PM", "Hybrid", 1, now),
        };
        sections[4].IsActive = false;
        var springPhase = new ApplicationPhase { Id = Id("70000000-0000-0000-0000-000000000002"), AcademicTerm = springTerm, Name = "Spring 2026 GTA Applications", Program = "Master's", OpensAtUtc = new(2025, 11, 1, 0, 0, 0, TimeSpan.Zero), ClosesAtUtc = new(2025, 12, 15, 23, 59, 59, TimeSpan.Zero), IsActive = false, CreatedAtUtc = now.AddMonths(-9), UpdatedAtUtc = now.AddMonths(-8) };
        var doctoralPhase = new ApplicationPhase { Id = Id("70000000-0000-0000-0000-000000000003"), AcademicTerm = fallTerm, Name = "Fall 2026 Doctoral GTA Applications", Program = "Doctoral", OpensAtUtc = fallPhase.OpensAtUtc, ClosesAtUtc = fallPhase.ClosesAtUtc, IsActive = true, CreatedAtUtc = now, UpdatedAtUtc = now };
        dbContext.AddRange(springTerm, programming, security, database, capstone);
        dbContext.AddRange(sections);
        dbContext.AddRange(springPhase, doctoralPhase);

        dbContext.FacultySectionAssignments.AddRange(
            Assignment(Id("80000000-0000-0000-0000-000000000002"), morgan, sections[0], now),
            Assignment(Id("80000000-0000-0000-0000-000000000003"), morgan, sections[1], now),
            Assignment(Id("80000000-0000-0000-0000-000000000004"), casey, sections[2], now),
            Assignment(Id("80000000-0000-0000-0000-000000000005"), casey, sections[3], now));

        var alexApplication = Application(Id("90000000-0000-0000-0000-000000000001"), alex, fallPhase, "GTA-2026-1001", ApplicationState.Submitted, EmploymentBasis.FullTime20Hours, now.AddDays(-10), now);
        var priyaApplication = Application(Id("90000000-0000-0000-0000-000000000002"), priya, fallPhase, "GTA-2026-1002", ApplicationState.Interview, EmploymentBasis.PartTime10Hours, now.AddDays(-9), now);
        var mateoApplication = Application(Id("90000000-0000-0000-0000-000000000003"), mateo, fallPhase, "GTA-2026-1003", ApplicationState.Selected, EmploymentBasis.PartTime10Hours, now.AddDays(-8), now);
        var rileyApplication = Application(Id("90000000-0000-0000-0000-000000000004"), riley, doctoralPhase, "GTA-2026-1004", ApplicationState.UnderReview, EmploymentBasis.FullTime20Hours, now.AddDays(-7), now);
        var alexHistorical = Application(Id("90000000-0000-0000-0000-000000000005"), alex, springPhase, "GTA-2026-0501", ApplicationState.NotSelected, EmploymentBasis.PartTime10Hours, now.AddMonths(-8), now);
        var priyaHistorical = Application(Id("90000000-0000-0000-0000-000000000006"), priya, springPhase, "GTA-2026-0502", ApplicationState.Withdrawn, EmploymentBasis.PartTime10Hours, now.AddMonths(-8), now);
        dbContext.AddRange(alexApplication, priyaApplication, mateoApplication, rileyApplication, alexHistorical, priyaHistorical);

        var choices = new[]
        {
            Choice(Id("91000000-0000-0000-0000-000000000001"), alexApplication, analyticsSection, 1, now),
            Choice(Id("91000000-0000-0000-0000-000000000002"), alexApplication, sections[1], 2, now),
            Choice(Id("91000000-0000-0000-0000-000000000003"), priyaApplication, sections[1], 1, now),
            Choice(Id("91000000-0000-0000-0000-000000000004"), mateoApplication, sections[0], 1, now),
            Choice(Id("91000000-0000-0000-0000-000000000005"), rileyApplication, sections[2], 1, now),
            Choice(Id("91000000-0000-0000-0000-000000000006"), alexHistorical, sections[4], 1, now.AddMonths(-8)),
            Choice(Id("91000000-0000-0000-0000-000000000007"), priyaHistorical, sections[4], 1, now.AddMonths(-8)),
        };
        dbContext.AddRange(choices);
        dbContext.FacultyReviewActions.AddRange(
            Review(Id("92000000-0000-0000-0000-000000000001"), choices[2], morgan, ReviewActionType.Interview, "Strong programming background; schedule a technical discussion.", now.AddDays(-4)),
            Review(Id("92000000-0000-0000-0000-000000000002"), choices[3], morgan, ReviewActionType.Interview, "Prior tutoring experience aligns with course needs.", now.AddDays(-5)),
            Review(Id("92000000-0000-0000-0000-000000000003"), choices[3], morgan, ReviewActionType.HireRecommendation, "Recommended after interview.", now.AddDays(-3)));
        dbContext.Placements.Add(new Placement { Id = Id("93000000-0000-0000-0000-000000000001"), ApplicationChoice = choices[3], CourseSection = sections[0], IsActive = true, CreatedAtUtc = now.AddDays(-2), UpdatedAtUtc = now.AddDays(-2) });

        AddHistory(alexApplication, ApplicationState.Draft, ApplicationState.Submitted, alex.Id, now.AddDays(-10), "Applicant submitted the application.");
        AddHistory(priyaApplication, ApplicationState.Submitted, ApplicationState.Interview, morgan.Id, now.AddDays(-4), "Faculty marked applicant for interview.");
        AddHistory(mateoApplication, ApplicationState.Interview, ApplicationState.Selected, administrator.Id, now.AddDays(-2), "Administrator assigned applicant to a section.");
        AddHistory(rileyApplication, ApplicationState.Submitted, ApplicationState.UnderReview, casey.Id, now.AddDays(-5), "Faculty review started.");
        AddHistory(alexHistorical, ApplicationState.UnderReview, ApplicationState.NotSelected, administrator.Id, now.AddMonths(-7), "Selection cycle completed.");
        AddHistory(priyaHistorical, ApplicationState.Submitted, ApplicationState.Withdrawn, priya.Id, now.AddMonths(-8).AddDays(2), "Applicant withdrew from consideration.");

        dbContext.SectionImportBatches.AddRange(
            new SectionImportBatch { Id = Id("94000000-0000-0000-0000-000000000001"), FileName = "fall-2026-sections.csv", ImportedAtUtc = now.AddDays(-30), ImportedByUserId = administrator.Id, TotalRows = 6, AcceptedRows = 6, RejectedRows = 0 },
            new SectionImportBatch { Id = Id("94000000-0000-0000-0000-000000000002"), FileName = "fall-2026-section-updates.csv", ImportedAtUtc = now.AddDays(-14), ImportedByUserId = administrator.Id, TotalRows = 5, AcceptedRows = 4, RejectedRows = 1, ErrorSummaryJson = "[{\"row\":5,\"message\":\"Available positions must be a non-negative number.\"}]" });
        dbContext.EmailOutboxMessages.AddRange(
            Email(Id("95000000-0000-0000-0000-000000000001"), priya.Email, "GTA interview update", EmailDeliveryState.Sent, now.AddDays(-4), null),
            Email(Id("95000000-0000-0000-0000-000000000002"), mateo.Email, "GTA placement update", EmailDeliveryState.Sent, now.AddDays(-2), null),
            Email(Id("95000000-0000-0000-0000-000000000003"), "delivery.failure@example.test", "GTA notification delivery test", EmailDeliveryState.Failed, now.AddDays(-1), "Demo SMTP recipient rejected."));
        dbContext.AuditLogs.AddRange(
            Audit(Id("96000000-0000-0000-0000-000000000001"), administrator.Id, "SectionImportCompleted", "SectionImportBatch", "fall-2026-sections.csv", "Succeeded", now.AddDays(-30)),
            Audit(Id("96000000-0000-0000-0000-000000000002"), morgan.Id, "InterviewMarked", "ApplicationChoice", choices[2].Id.ToString(), "Succeeded", now.AddDays(-4)),
            Audit(Id("96000000-0000-0000-0000-000000000003"), morgan.Id, "HireRecommended", "ApplicationChoice", choices[3].Id.ToString(), "Succeeded", now.AddDays(-3)),
            Audit(Id("96000000-0000-0000-0000-000000000004"), administrator.Id, "PlacementCreated", "ApplicationChoice", choices[3].Id.ToString(), "Succeeded", now.AddDays(-2)),
            Audit(Id("96000000-0000-0000-0000-000000000005"), administrator.Id, "SectionImportValidated", "SectionImportBatch", "fall-2026-section-updates.csv", "CompletedWithErrors", now.AddDays(-14)));

        await AddDemoDocumentAsync(Id("97000000-0000-0000-0000-000000000001"), alex, DocumentType.Resume, "alex-demo-resume.pdf", "Synthetic resume for the GTA demonstration.", now.AddDays(-12), cancellationToken);
        await AddDemoDocumentAsync(Id("97000000-0000-0000-0000-000000000002"), alex, DocumentType.UnofficialTranscript, "alex-demo-transcript.pdf", "Synthetic transcript for the GTA demonstration.", now.AddDays(-12), cancellationToken);
        await AddDemoDocumentAsync(Id("97000000-0000-0000-0000-000000000003"), priya, DocumentType.Resume, "priya-demo-resume.pdf", "Synthetic resume for the GTA demonstration.", now.AddDays(-11), cancellationToken);
        await AddDemoDocumentAsync(Id("97000000-0000-0000-0000-000000000004"), priya, DocumentType.UnofficialTranscript, "priya-demo-transcript.pdf", "Synthetic transcript for the GTA demonstration.", now.AddDays(-11), cancellationToken);
        await AddDemoDocumentAsync(Id("97000000-0000-0000-0000-000000000005"), mateo, DocumentType.Resume, "mateo-demo-resume.pdf", "Synthetic resume for the GTA demonstration.", now.AddDays(-10), cancellationToken);
        await AddDemoDocumentAsync(Id("97000000-0000-0000-0000-000000000006"), mateo, DocumentType.UnofficialTranscript, "mateo-demo-transcript.pdf", "Synthetic transcript for the GTA demonstration.", now.AddDays(-10), cancellationToken);
    }

    private async Task AddDemoDocumentAsync(Guid id, User owner, DocumentType type, string fileName, string label, DateTimeOffset uploadedAt, CancellationToken cancellationToken)
    {
        var bytes = System.Text.Encoding.ASCII.GetBytes($"%PDF-1.4\n% GTA DEMO ONLY\n1 0 obj<</Type/Catalog>>endobj\n% {label}\n%%EOF");
        await using var content = new MemoryStream(bytes, writable: false);
        var storageKey = await documentStorage.SaveAsync(content, cancellationToken);
        dbContext.Documents.Add(new Gta.Application.Domain.Documents.Document
        {
            Id = id,
            OwnerUser = owner,
            Type = type,
            OriginalFileName = fileName,
            StorageKey = storageKey,
            MediaType = "application/pdf",
            ByteLength = bytes.Length,
            Sha256 = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes)),
            Version = 1,
            State = DocumentState.Active,
            CreatedAtUtc = uploadedAt,
            UpdatedAtUtc = uploadedAt,
        });
    }

    private void AddHistory(ApplicantApplication application, ApplicationState from, ApplicationState to, Guid actorId, DateTimeOffset changedAt, string reason) =>
        dbContext.ApplicationStatusHistory.Add(new ApplicationStatusHistory { Application = application, FromState = from, ToState = to, ChangedByUserId = actorId, ChangedAtUtc = changedAt, Reason = reason });

    private static Guid Id(string value) => Guid.Parse(value);

    private static ApplicantProfile Profile(Guid id, User user, string preferredName, string program, string degree, string major, decimal gpa, string graduationTerm, int graduationYear, DateTimeOffset now) => new()
    {
        Id = id,
        User = user,
        PreferredName = preferredName,
        PhoneNumber = "555-01" + graduationYear.ToString()[^2..],
        Program = program,
        Degree = degree,
        Major = major,
        Gpa = gpa,
        ExpectedGraduationTerm = graduationTerm,
        ExpectedGraduationYear = graduationYear,
        CreatedAtUtc = now,
        UpdatedAtUtc = now,
    };

    private static EducationRecord Education(Guid id, ApplicantProfile profile, string institution, string degree, string field, DateOnly start, DateOnly end, DateTimeOffset now) => new()
    {
        Id = id,
        ApplicantProfile = profile,
        Institution = institution,
        Degree = degree,
        FieldOfStudy = field,
        StartDate = start,
        EndDate = end,
        CreatedAtUtc = now,
        UpdatedAtUtc = now,
    };

    private static ExperienceRecord Experience(Guid id, ApplicantProfile profile, string organization, string title, string description, DateOnly start, DateOnly? end, bool isGta, DateTimeOffset now) => new()
    {
        Id = id,
        ApplicantProfile = profile,
        Organization = organization,
        Title = title,
        Description = description,
        StartDate = start,
        EndDate = end,
        IsGtaExperience = isGta,
        CreatedAtUtc = now,
        UpdatedAtUtc = now,
    };

    private static Course Course(Guid id, string subject, string number, string title) => new() { Id = id, SubjectCode = subject, CatalogNumber = number, Title = title };

    private static CourseSection Section(Guid id, Course course, AcademicTerm term, string number, string schedule, string delivery, int positions, DateTimeOffset now) => new()
    {
        Id = id,
        Course = course,
        AcademicTerm = term,
        SectionNumber = number,
        Schedule = schedule,
        DeliveryMethod = delivery,
        AvailablePositions = positions,
        CreatedAtUtc = now,
        UpdatedAtUtc = now,
    };

    private static FacultySectionAssignment Assignment(Guid id, User faculty, CourseSection section, DateTimeOffset now) => new()
    {
        Id = id,
        FacultyUser = faculty,
        CourseSection = section,
        IsActive = true,
        CreatedAtUtc = now,
        UpdatedAtUtc = now,
    };

    private static ApplicantApplication Application(Guid id, User applicant, ApplicationPhase phase, string reference, ApplicationState state, EmploymentBasis basis, DateTimeOffset submittedAt, DateTimeOffset now) => new()
    {
        Id = id,
        ApplicantUser = applicant,
        ApplicationPhase = phase,
        Reference = reference,
        State = state,
        EmploymentBasis = basis,
        SubmittedAtUtc = submittedAt,
        CreatedAtUtc = submittedAt,
        UpdatedAtUtc = now,
    };

    private static ApplicationChoice Choice(Guid id, ApplicantApplication application, CourseSection section, int preference, DateTimeOffset now) => new()
    {
        Id = id,
        Application = application,
        CourseSection = section,
        PreferenceOrder = preference,
        CreatedAtUtc = now,
        UpdatedAtUtc = now,
    };

    private static FacultyReviewAction Review(Guid id, ApplicationChoice choice, User faculty, ReviewActionType type, string notes, DateTimeOffset now) => new()
    {
        Id = id,
        ApplicationChoice = choice,
        FacultyUser = faculty,
        Type = type,
        InternalNotes = notes,
        IsActive = true,
        CreatedAtUtc = now,
        UpdatedAtUtc = now,
    };

    private static EmailOutboxMessage Email(Guid id, string recipient, string subject, EmailDeliveryState state, DateTimeOffset createdAt, string? error) => new()
    {
        Id = id,
        Recipient = recipient,
        Subject = subject,
        TextBody = "This is fictional demonstration notification content.",
        State = state,
        CreatedAtUtc = createdAt,
        SentAtUtc = state == EmailDeliveryState.Sent ? createdAt.AddMinutes(1) : null,
        NextAttemptAtUtc = createdAt.AddMinutes(5),
        AttemptCount = state == EmailDeliveryState.Failed ? 3 : 1,
        LastError = error,
        CorrelationId = "demo-seed-" + id.ToString("N")[..8],
    };

    private static AuditLog Audit(Guid id, Guid actorId, string action, string entityType, string reference, string result, DateTimeOffset occurredAt) => new()
    {
        Id = id,
        ActorUserId = actorId,
        Action = action,
        EntityType = entityType,
        EntityReference = reference,
        Result = result,
        OccurredAtUtc = occurredAt,
        CorrelationId = "demo-audit-" + id.ToString("N")[..8],
    };

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
