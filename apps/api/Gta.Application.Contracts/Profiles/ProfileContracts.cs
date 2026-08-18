namespace Gta.Application.Contracts.Profiles;

public sealed record EducationRecordResponse(
    Guid Id,
    string Institution,
    string? Degree,
    string? FieldOfStudy,
    DateOnly? StartDate,
    DateOnly? EndDate);

public sealed record ExperienceRecordResponse(
    Guid Id,
    string Organization,
    string Title,
    string? Description,
    DateOnly? StartDate,
    DateOnly? EndDate,
    bool IsGtaExperience);

public sealed record ApplicantProfileResponse(
    string DisplayName,
    string Email,
    string? UniversityId,
    string? PreferredName,
    string? PhoneNumber,
    string? Program,
    string? Degree,
    string? Major,
    decimal? Gpa,
    string? ExpectedGraduationTerm,
    int? ExpectedGraduationYear,
    string? LinkedInUrl,
    IReadOnlyCollection<EducationRecordResponse> Education,
    IReadOnlyCollection<ExperienceRecordResponse> Experience,
    DateTimeOffset UpdatedAtUtc);

public sealed record SaveEducationRecordRequest(
    Guid? Id,
    string Institution,
    string? Degree,
    string? FieldOfStudy,
    DateOnly? StartDate,
    DateOnly? EndDate);

public sealed record SaveExperienceRecordRequest(
    Guid? Id,
    string Organization,
    string Title,
    string? Description,
    DateOnly? StartDate,
    DateOnly? EndDate,
    bool IsGtaExperience);

public sealed record UpdateApplicantProfileRequest(
    string? PreferredName,
    string? PhoneNumber,
    string? Program,
    string? Degree,
    string? Major,
    decimal? Gpa,
    string? ExpectedGraduationTerm,
    int? ExpectedGraduationYear,
    string? LinkedInUrl,
    IReadOnlyCollection<SaveEducationRecordRequest> Education,
    IReadOnlyCollection<SaveExperienceRecordRequest> Experience);

public sealed record ProfileCompletionResponse(
    int Percentage,
    IReadOnlyCollection<string> CompletedSections,
    IReadOnlyCollection<string> IncompleteSections);
