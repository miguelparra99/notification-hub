using NotificationHub.Domain.Enums;

namespace NotificationHub.Application.Notifications.Dtos;

public record SendNotificationRequest(
    NotificationChannel Channel,
    string Recipient,
    string? Subject,
    string? Body,
    string? TemplateCode,
    Dictionary<string, string>? TemplateValues);