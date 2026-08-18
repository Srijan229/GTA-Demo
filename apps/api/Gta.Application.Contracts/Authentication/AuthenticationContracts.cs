namespace Gta.Application.Contracts.Authentication;

public sealed record DevelopmentUserResponse(Guid Id, string DisplayName, string Email, IReadOnlyCollection<string> Roles, string Description);

public sealed record CurrentUserResponse(Guid Id, string DisplayName, string Email, IReadOnlyCollection<string> Roles);
