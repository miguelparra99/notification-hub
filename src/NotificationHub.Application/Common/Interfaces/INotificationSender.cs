using NotificationHub.Application.Common.Models;
using NotificationHub.Domain.Entities;
using NotificationHub.Domain.Enums;

namespace NotificationHub.Application.Common.Interfaces;

public interface INotificationSender
{
    NotificationChannel Channel { get; }
    string ProviderName { get; }
    Task<SendResult> SendAsync(Notification notification, CancellationToken ct = default);
}

public interface ISenderResolver
{
    INotificationSender Resolve(NotificationChannel channel);
}