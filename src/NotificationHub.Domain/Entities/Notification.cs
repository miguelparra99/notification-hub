using NotificationHub.Domain.Common;
using NotificationHub.Domain.Enums;
using NotificationHub.Domain.Exceptions;

namespace NotificationHub.Domain.Entities;

public class Notification : BaseEntity
{
    public const int MaxAttempts = 3;

    private readonly List<DeliveryAttempt> _attempts = new();

    public NotificationChannel Channel { get; private set; }
    public string Recipient { get; private set; } = null!;
    public string? Subject { get; private set; }
    public string Body { get; private set; } = null!;
    public NotificationStatus Status { get; private set; }
    public Guid? TemplateId { get; private set; }
    public DateTime? SentAt { get; private set; }
    public DateTime? NextRetryAt { get; private set; }

    public IReadOnlyCollection<DeliveryAttempt> Attempts => _attempts.AsReadOnly();
    public int AttemptCount => _attempts.Count;

    private Notification() { } // EF Core

    public static Notification Create(
        NotificationChannel channel,
        string recipient,
        string body,
        string? subject = null,
        Guid? templateId = null)
    {
        if (string.IsNullOrWhiteSpace(recipient))
            throw new DomainException("Recipient is required.");
        if (string.IsNullOrWhiteSpace(body))
            throw new DomainException("Body is required.");
        if (channel == NotificationChannel.Email && string.IsNullOrWhiteSpace(subject))
            throw new DomainException("Email notifications require a subject.");

        return new Notification
        {
            Channel = channel,
            Recipient = recipient.Trim(),
            Body = body,
            Subject = subject?.Trim(),
            TemplateId = templateId,
            Status = NotificationStatus.Pending
        };
    }

    public void MarkAsProcessing()
    {
        if (Status is not (NotificationStatus.Pending or NotificationStatus.Failed))
            throw new InvalidStateTransitionException(Status.ToString(), nameof(NotificationStatus.Processing));

        Status = NotificationStatus.Processing;
        Touch();
    }

    public void RecordSuccess(string providerName, string? providerMessageId)
    {
        _attempts.Add(DeliveryAttempt.Success(Id, AttemptCount + 1, providerName, providerMessageId));
        Status = NotificationStatus.Sent;
        SentAt = DateTime.UtcNow;
        NextRetryAt = null;
        Touch();
    }

    public void RecordFailure(string providerName, string errorMessage)
    {
        _attempts.Add(DeliveryAttempt.Failure(Id, AttemptCount + 1, providerName, errorMessage));
        Status = NotificationStatus.Failed;
        NextRetryAt = CanRetry ? DateTime.UtcNow.Add(BackoffFor(AttemptCount)) : null;
        Touch();
    }

    public bool CanRetry => Status == NotificationStatus.Failed && AttemptCount < MaxAttempts;

    public bool IsDueForRetry(DateTime now) => CanRetry && NextRetryAt.HasValue && NextRetryAt <= now;

    public void Cancel()
    {
        if (Status == NotificationStatus.Sent)
            throw new InvalidStateTransitionException(Status.ToString(), nameof(NotificationStatus.Cancelled));

        Status = NotificationStatus.Cancelled;
        NextRetryAt = null;
        Touch();
    }

    /// <summary>Exponential backoff: 1, 2, 4 minutes.</summary>
    private static TimeSpan BackoffFor(int attemptNumber) =>
        TimeSpan.FromMinutes(Math.Pow(2, attemptNumber - 1));
}