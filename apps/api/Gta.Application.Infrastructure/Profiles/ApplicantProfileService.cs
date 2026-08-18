using Gta.Application.Application.Profiles;
using Gta.Application.Contracts.Profiles;
using Gta.Application.Domain.Profiles;
using Gta.Application.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Gta.Application.Infrastructure.Profiles;

public sealed class ApplicantProfileService(GtaDbContext dbContext) : IApplicantProfileService
{
    public async Task<ApplicantProfileResponse?> GetAsync(Guid userId, CancellationToken cancellationToken)
    {
        var profile = await Query().SingleOrDefaultAsync(candidate => candidate.UserId == userId, cancellationToken);
        return profile is null ? null : Map(profile);
    }

    public async Task<ProfileCompletionResponse?> GetCompletionAsync(Guid userId, CancellationToken cancellationToken)
    {
        var profile = await GetAsync(userId, cancellationToken);
        return profile is null ? null : ProfileCompletionCalculator.Calculate(profile);
    }

    public async Task<ApplicantProfileResponse> UpdateAsync(
        Guid userId,
        UpdateApplicantProfileRequest request,
        CancellationToken cancellationToken)
    {
        Validate(request);
        var profile = await Query().SingleOrDefaultAsync(candidate => candidate.UserId == userId, cancellationToken)
            ?? throw new KeyNotFoundException("Applicant profile was not found.");

        profile.PreferredName = Clean(request.PreferredName);
        profile.PhoneNumber = Clean(request.PhoneNumber);
        profile.Program = Clean(request.Program);
        profile.Degree = Clean(request.Degree);
        profile.Major = Clean(request.Major);
        profile.Gpa = request.Gpa;
        profile.ExpectedGraduationTerm = Clean(request.ExpectedGraduationTerm);
        profile.ExpectedGraduationYear = request.ExpectedGraduationYear;
        profile.LinkedInUrl = Clean(request.LinkedInUrl);
        profile.UpdatedAtUtc = DateTimeOffset.UtcNow;

        ReconcileEducation(profile, request.Education);
        ReconcileExperience(profile, request.Experience);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(profile);
    }

    private IQueryable<ApplicantProfile> Query() => dbContext.ApplicantProfiles
        .Include(profile => profile.User)
        .Include(profile => profile.EducationRecords)
        .Include(profile => profile.ExperienceRecords)
        .AsSplitQuery();

    private void ReconcileEducation(ApplicantProfile profile, IReadOnlyCollection<SaveEducationRecordRequest> requests)
    {
        var requestedIds = requests.Where(item => item.Id.HasValue).Select(item => item.Id!.Value).ToHashSet();
        foreach (var removed in profile.EducationRecords.Where(item => !requestedIds.Contains(item.Id)).ToList())
        {
            dbContext.EducationRecords.Remove(removed);
        }

        foreach (var request in requests)
        {
            var record = request.Id.HasValue
                ? profile.EducationRecords.SingleOrDefault(item => item.Id == request.Id.Value)
                    ?? throw new ArgumentException("An education record does not belong to this profile.")
                : new EducationRecord { Institution = request.Institution, CreatedAtUtc = DateTimeOffset.UtcNow };
            if (!request.Id.HasValue)
            {
                record.ApplicantProfile = profile;
                dbContext.EducationRecords.Add(record);
            }
            record.Institution = request.Institution.Trim();
            record.Degree = Clean(request.Degree);
            record.FieldOfStudy = Clean(request.FieldOfStudy);
            record.StartDate = request.StartDate;
            record.EndDate = request.EndDate;
            record.UpdatedAtUtc = DateTimeOffset.UtcNow;
        }
    }

    private void ReconcileExperience(ApplicantProfile profile, IReadOnlyCollection<SaveExperienceRecordRequest> requests)
    {
        var requestedIds = requests.Where(item => item.Id.HasValue).Select(item => item.Id!.Value).ToHashSet();
        foreach (var removed in profile.ExperienceRecords.Where(item => !requestedIds.Contains(item.Id)).ToList())
        {
            dbContext.ExperienceRecords.Remove(removed);
        }

        foreach (var request in requests)
        {
            var record = request.Id.HasValue
                ? profile.ExperienceRecords.SingleOrDefault(item => item.Id == request.Id.Value)
                    ?? throw new ArgumentException("An experience record does not belong to this profile.")
                : new ExperienceRecord { Organization = request.Organization, Title = request.Title, CreatedAtUtc = DateTimeOffset.UtcNow };
            if (!request.Id.HasValue)
            {
                record.ApplicantProfile = profile;
                dbContext.ExperienceRecords.Add(record);
            }
            record.Organization = request.Organization.Trim();
            record.Title = request.Title.Trim();
            record.Description = Clean(request.Description);
            record.StartDate = request.StartDate;
            record.EndDate = request.EndDate;
            record.IsGtaExperience = request.IsGtaExperience;
            record.UpdatedAtUtc = DateTimeOffset.UtcNow;
        }
    }

    private static void Validate(UpdateApplicantProfileRequest request)
    {
        if (request.Gpa is < 0 or > 4) throw new ArgumentException("GPA must be between 0 and 4.");
        if (request.ExpectedGraduationYear is < 2000 or > 2100) throw new ArgumentException("Expected graduation year is invalid.");
        if (request.Education.Any(item => string.IsNullOrWhiteSpace(item.Institution))) throw new ArgumentException("Education institution is required.");
        if (request.Experience.Any(item => string.IsNullOrWhiteSpace(item.Organization) || string.IsNullOrWhiteSpace(item.Title))) throw new ArgumentException("Experience organization and title are required.");
        if (request.Education.Any(item => item.StartDate > item.EndDate) || request.Experience.Any(item => item.StartDate > item.EndDate)) throw new ArgumentException("Start date cannot be after end date.");
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static ApplicantProfileResponse Map(ApplicantProfile profile) => new(
        profile.User.DisplayName,
        profile.User.Email,
        profile.User.UniversityId,
        profile.PreferredName,
        profile.PhoneNumber,
        profile.Program,
        profile.Degree,
        profile.Major,
        profile.Gpa,
        profile.ExpectedGraduationTerm,
        profile.ExpectedGraduationYear,
        profile.LinkedInUrl,
        profile.EducationRecords.OrderByDescending(item => item.EndDate).Select(item => new EducationRecordResponse(item.Id, item.Institution, item.Degree, item.FieldOfStudy, item.StartDate, item.EndDate)).ToArray(),
        profile.ExperienceRecords.OrderByDescending(item => item.EndDate).Select(item => new ExperienceRecordResponse(item.Id, item.Organization, item.Title, item.Description, item.StartDate, item.EndDate, item.IsGtaExperience)).ToArray(),
        profile.UpdatedAtUtc);
}
