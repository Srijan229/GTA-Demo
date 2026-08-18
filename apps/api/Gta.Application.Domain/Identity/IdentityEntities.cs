using Gta.Application.Domain.Common;
using Gta.Application.Domain.Profiles;

namespace Gta.Application.Domain.Identity;

public sealed class User : AuditableEntity
{
    public string? UniversityId { get; set; }
    public required string Email { get; set; }
    public required string NormalizedEmail { get; set; }
    public required string DisplayName { get; set; }
    public bool IsActive { get; set; } = true;
    public ApplicantProfile? ApplicantProfile { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = [];
}

public sealed class Role : Entity
{
    public required string Name { get; set; }
    public required string NormalizedName { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = [];
}

public sealed class UserRole
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public DateTimeOffset AssignedAtUtc { get; set; }
    public Guid? AssignedByUserId { get; set; }
}
