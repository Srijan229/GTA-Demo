using Gta.Application.Contracts.Profiles;

namespace Gta.Application.Application.Profiles;

public static class ProfileCompletionCalculator
{
    public static ProfileCompletionResponse Calculate(ApplicantProfileResponse profile)
    {
        var sections = new Dictionary<string, bool>
        {
            ["Personal information"] = !string.IsNullOrWhiteSpace(profile.PreferredName) && !string.IsNullOrWhiteSpace(profile.PhoneNumber),
            ["Academic information"] = !string.IsNullOrWhiteSpace(profile.Program) && !string.IsNullOrWhiteSpace(profile.Degree) && !string.IsNullOrWhiteSpace(profile.Major),
            ["Education"] = profile.Education.Count > 0,
            ["Experience"] = profile.Experience.Count > 0,
        };

        var completed = sections.Where(section => section.Value).Select(section => section.Key).ToArray();
        var incomplete = sections.Where(section => !section.Value).Select(section => section.Key).ToArray();
        return new ProfileCompletionResponse(completed.Length * 100 / sections.Count, completed, incomplete);
    }
}
