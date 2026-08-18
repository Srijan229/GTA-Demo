using Gta.Application.Contracts.Documents;
using Gta.Application.Domain.Documents;

namespace Gta.Application.Application.Documents;

public sealed record DocumentDownload(Stream Content, string MediaType, string FileName);

public interface IApplicantDocumentService
{
    Task<IReadOnlyCollection<DocumentResponse>> GetCurrentAsync(Guid userId, CancellationToken cancellationToken);
    Task<DocumentResponse> UploadAsync(Guid userId, DocumentType type, string fileName, string mediaType, long byteLength, Stream content, CancellationToken cancellationToken);
    Task<DocumentDownload?> DownloadAsync(Guid userId, Guid documentId, CancellationToken cancellationToken);
}
