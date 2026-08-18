using Gta.Application.Domain.Common;
using Gta.Application.Domain.Identity;

namespace Gta.Application.Domain.Documents;

public enum DocumentType { Resume = 1, UnofficialTranscript = 2 }
public enum DocumentState { Active = 1, Superseded = 2, Removed = 3 }

public sealed class Document : AuditableEntity
{
    public Guid OwnerUserId { get; set; }
    public User OwnerUser { get; set; } = null!;
    public DocumentType Type { get; set; }
    public required string OriginalFileName { get; set; }
    public required string StorageKey { get; set; }
    public required string MediaType { get; set; }
    public long ByteLength { get; set; }
    public required string Sha256 { get; set; }
    public int Version { get; set; }
    public DocumentState State { get; set; } = DocumentState.Active;
}
