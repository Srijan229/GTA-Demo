using Gta.Application.Application.Documents;
using Gta.Application.Contracts.Faculty;

namespace Gta.Application.Application.Faculty;

public interface IFacultyReviewService
{
    Task<IReadOnlyCollection<FacultySectionResponse>> GetSectionsAsync(Guid facultyUserId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<FacultyApplicationListItemResponse>> GetApplicationsAsync(Guid facultyUserId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<FacultyInterviewQueueItemResponse>> GetInterviewQueueAsync(Guid facultyUserId, CancellationToken cancellationToken);
    Task<FacultyReviewResponse?> GetReviewAsync(Guid facultyUserId, Guid choiceId, CancellationToken cancellationToken);
    Task<FacultyActionResponse?> RecordActionAsync(Guid facultyUserId, Guid choiceId, RecordFacultyActionRequest request, string correlationId, CancellationToken cancellationToken);
    Task<DocumentDownload?> DownloadDocumentAsync(Guid facultyUserId, Guid documentId, CancellationToken cancellationToken);
}

public sealed class FacultyActionConflictException(string message) : Exception(message);
