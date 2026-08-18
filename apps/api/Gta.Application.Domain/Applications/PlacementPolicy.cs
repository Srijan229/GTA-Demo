namespace Gta.Application.Domain.Applications;

public static class PlacementPolicy
{
    public static int MaximumAssignments(EmploymentBasis basis) => basis switch
    {
        EmploymentBasis.PartTime10Hours => 1,
        EmploymentBasis.FullTime20Hours => 2,
        _ => throw new ArgumentOutOfRangeException(nameof(basis), basis, "Unsupported employment basis."),
    };

    public static bool CanAddAssignment(EmploymentBasis basis, int activeAssignmentCount)
    {
        if (activeAssignmentCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(activeAssignmentCount));
        }

        return activeAssignmentCount < MaximumAssignments(basis);
    }
}
