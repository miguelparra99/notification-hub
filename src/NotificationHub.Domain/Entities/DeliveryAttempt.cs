using NotificationHub.Domain.Common;

namespace NotificationHub.Domain.Entities;

public class DeliveryAttempt : BaseEntity
{
    public Guid NotificationId { get; private set; }
    public int AttemptNumber { get; private set; }
    public bool Succeeded { get; private set; }
    public string ProviderName { get; private set; } = null!;
    public string? ProviderMessageId { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime AttemptedAt { get; private set; }

    private DeliveryAttempt() { } // EF Core

    public static DeliveryAttempt Success(Guid notificationId, int attemptNumber, string providerName, string? providerMessageId) =>
        new()
        {
            NotificationId = notificationId,
            AttemptNumber = attemptNumber,
            ProviderName = providerName,
            ProviderMessageId = providerMessageId,
            Succeeded = true,
            AttemptedAt = DateTime.UtcNow
        };

    public static DeliveryAttempt Failure(Guid notificationId, int attemptNumber, string providerName, string errorMessage) =>
        new()
        {
            NotificationId = notificationId,
            AttemptNumber = attemptNumber,
            ProviderName = providerName,
            ErrorMessage = Truncate(errorMessage, 1000),
            Succeeded = false,
            AttemptedAt = DateTime.UtcNow
        };

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..max];
}