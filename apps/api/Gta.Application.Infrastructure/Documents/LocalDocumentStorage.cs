using Gta.Application.Application.Documents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Gta.Application.Infrastructure.Documents;

public sealed class LocalDocumentStorage : IDocumentStorage
{
    private readonly string rootPath;

    public LocalDocumentStorage(IConfiguration configuration, IHostEnvironment environment)
    {
        var configuredPath = configuration["DocumentStorage:RootPath"]
            ?? throw new InvalidOperationException("DocumentStorage:RootPath is required.");
        rootPath = Path.GetFullPath(configuredPath, environment.ContentRootPath);
        Directory.CreateDirectory(rootPath);
    }

    public async Task<string> SaveAsync(Stream content, CancellationToken cancellationToken)
    {
        var storageKey = $"{Guid.NewGuid():N}.bin";
        var path = Resolve(storageKey);
        await using var destination = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, FileOptions.Asynchronous);
        await content.CopyToAsync(destination, cancellationToken);
        return storageKey;
    }

    public Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Stream stream = new FileStream(Resolve(storageKey), FileMode.Open, FileAccess.Read, FileShare.Read, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var path = Resolve(storageKey);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    private string Resolve(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey) || Path.GetFileName(storageKey) != storageKey)
        {
            throw new InvalidOperationException("Invalid document storage key.");
        }

        var path = Path.GetFullPath(Path.Combine(rootPath, storageKey));
        if (!path.StartsWith(rootPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Document path escaped the storage root.");
        }
        return path;
    }
}
