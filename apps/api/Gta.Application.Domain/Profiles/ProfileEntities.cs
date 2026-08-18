using Gta.Application.Domain.Common;
using Gta.Application.Domain.Identity;

namespace Gta.Application.Domain.Profiles;

public sealed class ApplicantProfile : AuditableEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string? PreferredName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Program { get; set; }
    public string? Degree { get; set; }
    public string? Major { get; set; }
    public decimal? Gpa { get; set; }
    public string? ExpectedGraduationTerm { get; set; }
    public int? ExpectedGraduationYear { get; set; }
    public string? LinkedInUrl { get; set; }
    public ICollection<EducationRecord> EducationRecords { get; set; } = [];
    public ICollection<ExperienceRecord> ExperienceRecords { get; set; } = [];
}

public sealed class EducationRecord : AuditableEntity
{
    public Guid ApplicantProfileId { get; set; }
    public ApplicantProfile ApplicantProfile { get; set; } = null!;
    public required string Institution { get; set; }
    public string? Degree { get; set; }
    public string? FieldOfStudy { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}

public sealed class ExperienceRecord : AuditableEntity
{
    public Guid ApplicantProfileId { get; set; }
    public ApplicantProfile ApplicantProfile { get; set; } = null!;
    public required string Organization { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsGtaExperience { get; set; }
}
