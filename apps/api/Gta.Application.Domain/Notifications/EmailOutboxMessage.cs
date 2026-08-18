using Gta.Application.Domain.Common;

namespace Gta.Application.Domain.Notifications;

public enum EmailDeliveryState { Pending = 1, Sent = 2, Failed = 3 }
public sealed class EmailOutboxMessage : Entity
{
    public required string Recipient { get; set; }
    public required string Subject { get; set; }
    public required string TextBody { get; set; }
    public EmailDeliveryState State { get; set; } = EmailDeliveryState.Pending;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? SentAtUtc { get; set; }
    public DateTimeOffset NextAttemptAtUtc { get; set; }
    public int AttemptCount { get; set; }
    public string? LastError { get; set; }
    public string? CorrelationId { get; set; }
}
