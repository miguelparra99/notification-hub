using NotificationHub.Application.Common.Interfaces;
using NotificationHub.Domain.Enums;
using NotificationHub.Domain.Exceptions;

namespace NotificationHub.Infrastructure.Senders;

public class SenderResolver : ISenderResolver
{
    private readonly IReadOnlyDictionary<NotificationChannel, INotificationSender> _senders;

    public SenderResolver(IEnumerable<INotificationSender> senders) =>
        _senders = senders.ToDictionary(s => s.Channel);

    public INotificationSender Resolve(NotificationChannel channel) =>
        _senders.TryGetValue(channel, out var sender)
            ? sender
            : throw new DomainException($"No provider configured for channel '{channel}'.");
}