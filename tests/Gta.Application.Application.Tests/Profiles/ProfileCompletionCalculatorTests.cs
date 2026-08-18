using Gta.Application.Application.Profiles;
using Gta.Application.Contracts.Profiles;

namespace Gta.Application.Application.Tests.Profiles;

public sealed class ProfileCompletionCalculatorTests
{
    [Fact]
    public void Calculate_returns_full_completion_when_every_section_is_present()
    {
        var profile = Profile(
            preferredName: "Alex",
            phone: "555-0100",
            program: "Master's",
            degree: "MS",
            major: "Information Systems",
            education: [new(Guid.NewGuid(), "Example University", "BS", "IT", null, null)],
            experience: [new(Guid.NewGuid(), "Example Org", "Analyst", null, null, null, false)]);

        var result = ProfileCompletionCalculator.Calculate(profile);

        Assert.Equal(100, result.Percentage);
        Assert.Empty(result.IncompleteSections);
    }

    [Fact]
    public void Calculate_reports_incomplete_sections_without_storing_a_percentage()
    {
        var result = ProfileCompletionCalculator.Calculate(Profile());

        Assert.Equal(0, result.Percentage);
        Assert.Equal(4, result.IncompleteSections.Count);
    }

    private static ApplicantProfileResponse Profile(
        string? preferredName = null,
        string? phone = null,
        string? program = null,
        string? degree = null,
        string? major = null,
        IReadOnlyCollection<EducationRecordResponse>? education = null,
        IReadOnlyCollection<ExperienceRecordResponse>? experience = null) =>
        new("Alex Applicant", "alex@example.test", "G00000001", preferredName, phone, program, degree, major, null, null, null, null,
            education ?? [], experience ?? [], DateTimeOffset.UtcNow);
}
