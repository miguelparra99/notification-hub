namespace NotificationHub.Application.Common.Models;

public record SendResult(bool Succeeded, string? ProviderMessageId, string? ErrorMessage)
{
    public static SendResult Success(string? messageId = null) => new(true, messageId, null);
    public static SendResult Failure(string error) => new(false, null, error);
}