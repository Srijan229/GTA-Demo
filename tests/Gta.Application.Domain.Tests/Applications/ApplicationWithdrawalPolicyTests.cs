using Gta.Application.Domain.Applications;

namespace Gta.Application.Domain.Tests.Applications;

public sealed class ApplicationWithdrawalPolicyTests
{
    [Theory]
    [InlineData(ApplicationState.Submitted)]
    [InlineData(ApplicationState.UnderReview)]
    public void Allows_pre_hiring_states(ApplicationState state) => Assert.Null(ApplicationWithdrawalPolicy.BlockedReason(state, false));

    [Theory]
    [InlineData(ApplicationState.Interview)]
    [InlineData(ApplicationState.Selected)]
    [InlineData(ApplicationState.NotSelected)]
    [InlineData(ApplicationState.Withdrawn)]
    public void Blocks_terminal_or_hiring_states(ApplicationState state) => Assert.NotNull(ApplicationWithdrawalPolicy.BlockedReason(state, false));

    [Fact]
    public void Hiring_activity_blocks_an_otherwise_eligible_application() => Assert.NotNull(ApplicationWithdrawalPolicy.BlockedReason(ApplicationState.Submitted, true));
}
