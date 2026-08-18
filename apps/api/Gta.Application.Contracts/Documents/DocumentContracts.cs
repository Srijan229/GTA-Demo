namespace Gta.Application.Contracts.Documents;

public sealed record DocumentResponse(
    Guid Id,
    string Type,
    string OriginalFileName,
    string MediaType,
    long ByteLength,
    int Version,
    DateTimeOffset UploadedAtUtc);
