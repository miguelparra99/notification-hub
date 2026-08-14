using Microsoft.Extensions.Logging;
using NotificationHub.Application.Common.Interfaces;
using NotificationHub.Application.Common.Models;
using NotificationHub.Domain.Entities;
using NotificationHub.Domain.Enums;

namespace NotificationHub.Infrastructure.Senders;

/// <summary>
/// Stand-in provider used until a real SMS/Push gateway is wired in.
/// Fails intermittently so retry behaviour can be exercised locally.
/// </summary>
public class SimulatedSender : INotificationSender
{
    private readonly ILogger<SimulatedSender> _logger;

    public SimulatedSender(NotificationChannel channel, string providerName, ILogger<SimulatedSender> logger)
    {
        Channel = channel;
        ProviderName = providerName;
        _logger = logger;
    }

    public NotificationChannel Channel { get; }
    public string ProviderName { get; }

    public async Task<SendResult> SendAsync(Notification notification, CancellationToken ct = default)
    {
        await Task.Delay(Random.Shared.Next(50, 200), ct);

        if (Random.Shared.NextDouble() < 0.2)
        {
            _logger.LogWarning("{Provider} simulated a transient failure.", ProviderName);
            return SendResult.Failure("Simulated provider timeout.");
        }

        _logger.LogInformation("{Provider} delivered to {Recipient}.", ProviderName, notification.Recipient);
        return SendResult.Success($"sim-{Guid.NewGuid():N}");
    }
}