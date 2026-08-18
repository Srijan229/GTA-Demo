namespace Gta.Application.Domain.Applications;

public static class ApplicationWithdrawalPolicy
{
    public static string? BlockedReason(ApplicationState state, bool hasHiringActivity)
    {
        if (hasHiringActivity) return "Withdrawal is unavailable after interview or hiring activity has begun. Contact an administrator.";
        return state switch
        {
            ApplicationState.Submitted or ApplicationState.UnderReview => null,
            ApplicationState.Interview => "Withdrawal is unavailable after interview activity has begun. Contact an administrator.",
            ApplicationState.Selected => "A selected application cannot be withdrawn. Contact an administrator.",
            ApplicationState.NotSelected => "A completed application decision cannot be withdrawn.",
            ApplicationState.Withdrawn => "This application is already withdrawn.",
            _ => "This application cannot be withdrawn in its current state.",
        };
    }
}
