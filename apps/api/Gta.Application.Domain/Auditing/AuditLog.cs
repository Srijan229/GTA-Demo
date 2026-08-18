using Gta.Application.Domain.Common;

namespace Gta.Application.Domain.Auditing;

public sealed class AuditLog : Entity
{
    public DateTimeOffset OccurredAtUtc { get; set; }
    public Guid? ActorUserId { get; set; }
    public required string Action { get; set; }
    public required string EntityType { get; set; }
    public string? EntityReference { get; set; }
    public required string Result { get; set; }
    public required string CorrelationId { get; set; }
    public string? RedactedDetailsJson { get; set; }
}
