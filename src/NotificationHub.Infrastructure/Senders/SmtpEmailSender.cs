using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using NotificationHub.Application.Common.Interfaces;
using NotificationHub.Application.Common.Models;
using NotificationHub.Domain.Entities;
using NotificationHub.Domain.Enums;
using System.Net.Mail;

namespace NotificationHub.Infrastructure.Senders;

public class SmtpOptions
{
    public const string SectionName = "Smtp";
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1025;
    public string FromAddress { get; set; } = "noreply@notificationhub.dev";
    public string FromName { get; set; } = "NotificationHub";
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool UseSsl { get; set; }
}

public class SmtpEmailSender : INotificationSender
{
    private readonly SmtpOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<SmtpOptions> options, ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public NotificationChannel Channel => NotificationChannel.Email;
    public string ProviderName => "SMTP";

    public async Task<SendResult> SendAsync(Notification notification, CancellationToken ct = default)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
            message.To.Add(MailboxAddress.Parse(notification.Recipient));
            message.Subject = notification.Subject ?? "(no subject)";
            message.Body = new TextPart("html") { Text = notification.Body };

            using var client = new MailKit.Net.Smtp.SmtpClient();
            await client.ConnectAsync(
                _options.Host,
                _options.Port,
                _options.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None,
                ct);

            if (!string.IsNullOrWhiteSpace(_options.Username))
                await client.AuthenticateAsync(_options.Username, _options.Password ?? string.Empty, ct);

            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);

            return SendResult.Success(message.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMTP delivery failed for {Recipient}.", notification.Recipient);
            return SendResult.Failure(ex.Message);
        }
    }
}