namespace Gta.Application.Contracts.Administration;

public sealed record AdminDashboardResponse(int Applications, int Applicants, int ActiveSections, int Faculty, int AwaitingReview, int UnassignedSections, IReadOnlyCollection<string> Warnings);
public sealed record AdminApplicationResponse(Guid Id, string Reference, string ApplicantName, string Program, string State, DateTimeOffset? SubmittedAtUtc, int ChoiceCount);
public sealed record AdminApplicantResponse(Guid UserId, string DisplayName, string Email, string? UniversityId, string? Program, bool ProfileComplete, int ApplicationCount);
public sealed record AdminSectionResponse(Guid Id, string CourseCode, string Title, string SectionNumber, string Term, int? AvailablePositions, bool IsActive, Guid? FacultyUserId, string? FacultyName);
public sealed record AdminPhaseResponse(Guid Id, string Name, string Program, string Term, DateTimeOffset OpensAtUtc, DateTimeOffset ClosesAtUtc, bool IsActive);
public sealed record AdminUserResponse(Guid Id, string DisplayName, string Email, bool IsActive, IReadOnlyCollection<string> Roles);
public sealed record AdminSettingResponse(string Key, string Value, string? Description, bool IsDevelopmentOnly, DateTimeOffset UpdatedAtUtc);
public sealed record AdminAuditResponse(Guid Id, DateTimeOffset OccurredAtUtc, Guid? ActorUserId, string Action, string EntityType, string? EntityReference, string Result, string CorrelationId);
public sealed record EmailDeliveryResponse(Guid Id, string Recipient, string Subject, string State, int AttemptCount, DateTimeOffset CreatedAtUtc, DateTimeOffset? SentAtUtc, string? LastError, string? CorrelationId);
public sealed record PlacementCandidateResponse(Guid ChoiceId, Guid ApplicationId, string Reference, string ApplicantName, string EmploymentBasis, string AssignmentState, int ActivePlacements, int MaximumPlacements, Guid SectionId, string CourseCode, string SectionNumber, string Term, int? AvailablePositions, int FilledPositions, bool IsPlacedHere);
public sealed record PlacementActionResponse(Guid ChoiceId, bool Active, string AssignmentState, int ActivePlacements, int MaximumPlacements, DateTimeOffset ChangedAtUtc);
public sealed record AssignFacultyRequest(Guid? FacultyUserId);
public sealed record UpdateSectionRequest(bool IsActive, int? AvailablePositions);
public sealed record UpdatePhaseRequest(DateTimeOffset OpensAtUtc, DateTimeOffset ClosesAtUtc, bool IsActive);
public sealed record UpdateUserRequest(bool IsActive, IReadOnlyCollection<string> Roles);
public sealed record UpdateSettingRequest(string Value);
public sealed record UpdatePlacementRequest(bool Active);
