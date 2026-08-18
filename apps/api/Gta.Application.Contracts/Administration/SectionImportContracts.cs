namespace Gta.Application.Contracts.Administration;

public sealed record SectionImportError(int RowNumber, string Message);
public sealed record SectionImportPreviewResponse(int TotalRows, int AcceptedRows, int RejectedRows, IReadOnlyCollection<SectionImportError> Errors);
public sealed record SectionImportHistoryResponse(Guid Id, string FileName, DateTimeOffset ImportedAtUtc, int TotalRows, int AcceptedRows, int RejectedRows, IReadOnlyCollection<SectionImportError> Errors);
