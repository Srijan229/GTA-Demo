using System.Security.Cryptography;
using System.IO.Compression;
using Gta.Application.Application.Documents;
using Gta.Application.Contracts.Documents;
using Gta.Application.Domain.Documents;
using Gta.Application.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Gta.Application.Infrastructure.Documents;

public sealed class ApplicantDocumentService(
    GtaDbContext dbContext,
    IDocumentStorage storage,
    IConfiguration configuration) : IApplicantDocumentService
{
    public async Task<IReadOnlyCollection<DocumentResponse>> GetCurrentAsync(Guid userId, CancellationToken cancellationToken) =>
        await dbContext.Documents.AsNoTracking()
            .Where(document => document.OwnerUserId == userId && document.State == DocumentState.Active)
            .OrderBy(document => document.Type)
            .Select(document => Map(document))
            .ToListAsync(cancellationToken);

    public async Task<DocumentResponse> UploadAsync(
        Guid userId,
        DocumentType type,
        string fileName,
        string mediaType,
        long byteLength,
        Stream content,
        CancellationToken cancellationToken)
    {
        var storedMaximum = await dbContext.SystemSettings.Where(x => x.Key == "Documents.MaximumMegabytes").Select(x => x.Value).SingleOrDefaultAsync(cancellationToken);
        var maxBytes = int.TryParse(storedMaximum, out var storedMegabytes) ? storedMegabytes * 1024L * 1024L : long.TryParse(configuration["DocumentStorage:MaximumBytes"], out var configuredMaximum) ? configuredMaximum : 10 * 1024 * 1024;
        if (byteLength <= 0 || byteLength > maxBytes) throw new ArgumentException($"File size must be between 1 byte and {maxBytes / 1024 / 1024} MB.");

        var safeName = Path.GetFileName(fileName);
        if (safeName != fileName || string.IsNullOrWhiteSpace(safeName)) throw new ArgumentException("The filename is invalid.");
        await using var buffered = new MemoryStream();
        await content.CopyToAsync(buffered, cancellationToken);
        if (buffered.Length != byteLength) throw new ArgumentException("The uploaded file length is invalid.");
        ValidateSignature(type, safeName, mediaType, buffered.GetBuffer().AsSpan(0, checked((int)buffered.Length)));

        var checksum = Convert.ToHexString(SHA256.HashData(buffered.GetBuffer().AsSpan(0, checked((int)buffered.Length)))).ToLowerInvariant();
        buffered.Position = 0;
        var storageKey = await storage.SaveAsync(buffered, cancellationToken);

        try
        {
            var current = await dbContext.Documents
                .Where(document => document.OwnerUserId == userId && document.Type == type && document.State == DocumentState.Active)
                .SingleOrDefaultAsync(cancellationToken);
            var version = current?.Version + 1 ?? 1;
            if (current is not null)
            {
                current.State = DocumentState.Superseded;
                current.UpdatedAtUtc = DateTimeOffset.UtcNow;
            }

            var now = DateTimeOffset.UtcNow;
            var document = new Document
            {
                OwnerUserId = userId,
                Type = type,
                OriginalFileName = safeName,
                StorageKey = storageKey,
                MediaType = mediaType,
                ByteLength = byteLength,
                Sha256 = checksum,
                Version = version,
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
            };
            dbContext.Documents.Add(document);
            await dbContext.SaveChangesAsync(cancellationToken);
            return Map(document);
        }
        catch
        {
            await storage.DeleteAsync(storageKey, cancellationToken);
            throw;
        }
    }

    public async Task<DocumentDownload?> DownloadAsync(Guid userId, Guid documentId, CancellationToken cancellationToken)
    {
        var document = await dbContext.Documents.AsNoTracking().SingleOrDefaultAsync(
            candidate => candidate.Id == documentId && candidate.OwnerUserId == userId && candidate.State == DocumentState.Active,
            cancellationToken);
        if (document is null) return null;
        return new DocumentDownload(await storage.OpenReadAsync(document.StorageKey, cancellationToken), document.MediaType, document.OriginalFileName);
    }

    private static void ValidateSignature(DocumentType type, string fileName, string mediaType, ReadOnlySpan<byte> bytes)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var isPdf = extension == ".pdf" && mediaType == "application/pdf" && bytes.StartsWith("%PDF-"u8);
        var isDocx = type == DocumentType.Resume && extension == ".docx" &&
            mediaType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document" && IsDocx(bytes);
        if (!isPdf && !isDocx)
        {
            throw new ArgumentException(type == DocumentType.Resume ? "Resume must be a valid PDF or DOCX file." : "Unofficial transcript must be a valid PDF file.");
        }
    }

    private static bool IsDocx(ReadOnlySpan<byte> bytes)
    {
        if (!bytes.StartsWith(new byte[] { 0x50, 0x4B, 0x03, 0x04 })) return false;
        try
        {
            using var stream = new MemoryStream(bytes.ToArray(), writable: false);
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
            return archive.GetEntry("[Content_Types].xml") is not null && archive.GetEntry("word/document.xml") is not null;
        }
        catch (InvalidDataException)
        {
            return false;
        }
    }

    private static DocumentResponse Map(Document document) => new(
        document.Id, document.Type.ToString(), document.OriginalFileName, document.MediaType,
        document.ByteLength, document.Version, document.CreatedAtUtc);
}
