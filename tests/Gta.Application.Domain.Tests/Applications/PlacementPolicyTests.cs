using Gta.Application.Domain.Applications;

namespace Gta.Application.Domain.Tests.Applications;

public sealed class PlacementPolicyTests
{
    [Theory]
    [InlineData(EmploymentBasis.PartTime10Hours, 0, true)]
    [InlineData(EmploymentBasis.PartTime10Hours, 1, false)]
    [InlineData(EmploymentBasis.FullTime20Hours, 0, true)]
    [InlineData(EmploymentBasis.FullTime20Hours, 1, true)]
    [InlineData(EmploymentBasis.FullTime20Hours, 2, false)]
    public void CanAddAssignment_enforces_workload_limit(
        EmploymentBasis basis,
        int existingAssignments,
        bool expected) =>
        Assert.Equal(expected, PlacementPolicy.CanAddAssignment(basis, existingAssignments));

    [Fact]
    public void CanAddAssignment_rejects_negative_counts() =>
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PlacementPolicy.CanAddAssignment(EmploymentBasis.PartTime10Hours, -1));
}
