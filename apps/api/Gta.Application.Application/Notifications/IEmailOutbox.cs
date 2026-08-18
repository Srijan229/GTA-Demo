namespace Gta.Application.Application.Notifications;

public interface IEmailOutbox { void Queue(string recipient, string subject, string textBody, string? correlationId = null); }
public interface IEmailSender { Task SendAsync(string recipient, string subject, string textBody, CancellationToken token); }
