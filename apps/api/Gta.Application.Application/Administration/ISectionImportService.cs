using Gta.Application.Contracts.Administration;
namespace Gta.Application.Application.Administration;

public interface ISectionImportService
{
    Task<SectionImportPreviewResponse> PreviewAsync(Stream content, CancellationToken token);
    Task<SectionImportHistoryResponse> ImportAsync(Stream content, string fileName, Guid actorId, string correlationId, CancellationToken token);
    Task<IReadOnlyCollection<SectionImportHistoryResponse>> GetHistoryAsync(CancellationToken token);
}
