namespace Gta.Application.Application.Documents;

public interface IDocumentStorage
{
    Task<string> SaveAsync(Stream content, CancellationToken cancellationToken);
    Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken);
    Task DeleteAsync(string storageKey, CancellationToken cancellationToken);
}
