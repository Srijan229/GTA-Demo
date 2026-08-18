using System.Net.Mail;
using Gta.Application.Application.Notifications;
using Gta.Application.Domain.Notifications;
using Gta.Application.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Gta.Application.Infrastructure.Notifications;

public sealed class EmailOutbox(GtaDbContext db) : IEmailOutbox
{
    public void Queue(string recipient, string subject, string textBody, string? correlationId = null) => db.EmailOutboxMessages.Add(new EmailOutboxMessage { Recipient = recipient, Subject = subject, TextBody = textBody, CreatedAtUtc = DateTimeOffset.UtcNow, NextAttemptAtUtc = DateTimeOffset.UtcNow, CorrelationId = correlationId });
}
public sealed class SmtpEmailSender(IConfiguration configuration) : IEmailSender
{
    public async Task SendAsync(string recipient, string subject, string textBody, CancellationToken token)
    {
        var host = configuration["Email:SmtpHost"] ?? "localhost"; var port = int.TryParse(configuration["Email:SmtpPort"], out var configuredPort) ? configuredPort : 1025; var from = configuration["Email:FromAddress"] ?? "gta-application@example.test";
        using var message = new MailMessage(from, recipient, subject, textBody); using var client = new SmtpClient(host, port) { EnableSsl = false, DeliveryMethod = SmtpDeliveryMethod.Network }; await client.SendMailAsync(message, token);
    }
}
public sealed class EmailOutboxProcessor(IServiceScopeFactory scopes, ILogger<EmailOutboxProcessor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await ProcessAsync(stoppingToken); } catch (Exception ex) { logger.LogError(ex, "Email outbox cycle failed."); }
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
    private async Task ProcessAsync(CancellationToken token)
    {
        using var scope = scopes.CreateScope(); var db = scope.ServiceProvider.GetRequiredService<GtaDbContext>(); var sender = scope.ServiceProvider.GetRequiredService<IEmailSender>(); var now = DateTimeOffset.UtcNow;
        var messages = await db.EmailOutboxMessages.Where(x => x.State == EmailDeliveryState.Pending && x.NextAttemptAtUtc <= now).OrderBy(x => x.CreatedAtUtc).Take(20).ToListAsync(token);
        foreach (var message in messages) { try { await sender.SendAsync(message.Recipient, message.Subject, message.TextBody, token); message.State = EmailDeliveryState.Sent; message.SentAtUtc = DateTimeOffset.UtcNow; message.LastError = null; } catch (Exception ex) { message.AttemptCount++; message.LastError = ex.GetType().Name; message.NextAttemptAtUtc = DateTimeOffset.UtcNow.AddMinutes(Math.Min(30, message.AttemptCount * 2)); if (message.AttemptCount >= 5) message.State = EmailDeliveryState.Failed; logger.LogWarning("Email delivery failed for outbox message {MessageId}.", message.Id); } await db.SaveChangesAsync(token); }
    }
}
