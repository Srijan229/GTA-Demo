namespace Gta.Application.Contracts.Applications;

public sealed record AvailableSectionResponse(
    Guid Id,
    Guid PhaseId,
    string PhaseName,
    string Term,
    string CourseCode,
    string CourseTitle,
    string SectionNumber,
    string? Schedule,
    string? DeliveryMethod,
    int? AvailablePositions,
    bool AlreadyApplied);

public sealed record SubmitApplicationRequest(
    Guid PhaseId,
    string EmploymentBasis,
    IReadOnlyCollection<Guid> SectionIds);

public sealed record ApplicationChoiceResponse(Guid SectionId, string CourseCode, string SectionNumber, string CourseTitle);

public sealed record ApplicationResponse(
    Guid Id,
    string Reference,
    string PhaseName,
    string Term,
    string EmploymentBasis,
    string Status,
    DateTimeOffset SubmittedAtUtc,
    IReadOnlyCollection<ApplicationChoiceResponse> Choices);

public sealed record ApplicationStatusHistoryResponse(string FromStatus, string ToStatus, DateTimeOffset ChangedAtUtc, string? Reason);
public sealed record ApplicationDetailResponse(
    Guid Id,
    string Reference,
    string PhaseName,
    string Term,
    string EmploymentBasis,
    string Status,
    DateTimeOffset SubmittedAtUtc,
    IReadOnlyCollection<ApplicationChoiceResponse> Choices,
    IReadOnlyCollection<ApplicationStatusHistoryResponse> StatusHistory,
    bool CanWithdraw,
    string? WithdrawalBlockedReason);
public sealed record WithdrawApplicationRequest(string? Reason);
public sealed record ApplicationConfigurationResponse(int MaximumSectionChoices);
