using NotificationHub.Domain.Entities;

namespace NotificationHub.Application.Notifications.Dtos;

public record NotificationResponse(
    Guid Id,
    string Channel,
    string Recipient,
    string? Subject,
    string Body,
    string Status,
    int AttemptCount,
    DateTime? SentAt,
    DateTime? NextRetryAt,
    DateTime CreatedAt,
    IEnumerable<AttemptResponse> Attempts)
{
    public static NotificationResponse From(Notification n) => new(
        n.Id,
        n.Channel.ToString(),
        n.Recipient,
        n.Subject,
        n.Body,
        n.Status.ToString(),
        n.AttemptCount,
        n.SentAt,
        n.NextRetryAt,
        n.CreatedAt,
        n.Attempts.Select(AttemptResponse.From));
}

public record AttemptResponse(
    int AttemptNumber, bool Succeeded, string ProviderName, string? ErrorMessage, DateTime AttemptedAt)
{
    public static AttemptResponse From(DeliveryAttempt a) =>
        new(a.AttemptNumber, a.Succeeded, a.ProviderName, a.ErrorMessage, a.AttemptedAt);
}