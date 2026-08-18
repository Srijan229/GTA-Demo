using Gta.Application.Domain.Common;

namespace Gta.Application.Domain.Applications;

public sealed class SectionImportBatch : Entity
{
    public required string FileName { get; set; }
    public DateTimeOffset ImportedAtUtc { get; set; }
    public Guid ImportedByUserId { get; set; }
    public int TotalRows { get; set; }
    public int AcceptedRows { get; set; }
    public int RejectedRows { get; set; }
    public string? ErrorSummaryJson { get; set; }
}
