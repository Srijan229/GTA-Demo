using Gta.Application.Contracts.Documents;
using Gta.Application.Contracts.Profiles;

namespace Gta.Application.Contracts.Faculty;

public sealed record FacultySectionResponse(
    Guid Id,
    string CourseCode,
    string SectionNumber,
    string CourseTitle,
    string Term,
    string? Schedule,
    int ApplicationCount);

public sealed record FacultyApplicationListItemResponse(
    Guid ChoiceId,
    Guid ApplicationId,
    string ApplicantName,
    string Program,
    string CourseCode,
    string SectionNumber,
    string Status,
    DateTimeOffset SubmittedAtUtc,
    bool InterviewMarked,
    bool HireRecommended);

public sealed record FacultyReviewResponse(
    Guid ChoiceId,
    Guid ApplicationId,
    string Reference,
    string Status,
    string EmploymentBasis,
    string CourseCode,
    string SectionNumber,
    DateTimeOffset SubmittedAtUtc,
    ApplicantProfileResponse Profile,
    IReadOnlyCollection<DocumentResponse> Documents,
    bool InterviewMarked,
    bool HireRecommended,
    string? InternalNotes);

public sealed record RecordFacultyActionRequest(string Action, bool Active, string? InternalNotes);

public sealed record FacultyActionResponse(Guid ChoiceId, string Action, bool Active, DateTimeOffset RecordedAtUtc);
public sealed record FacultyInterviewQueueItemResponse(Guid ChoiceId, Guid ApplicationId, string ApplicantName, string Program, string CourseCode, string SectionNumber, string Term, string ApplicationStatus, string EmploymentBasis, DateTimeOffset InterviewMarkedAtUtc, bool HireRecommended, int ActivePlacements, int MaximumPlacements, string DecisionState);
