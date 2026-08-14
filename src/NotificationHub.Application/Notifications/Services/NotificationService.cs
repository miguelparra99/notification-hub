using Microsoft.Extensions.Logging;
using NotificationHub.Application.Common.Interfaces;
using NotificationHub.Application.Notifications.Dtos;
using NotificationHub.Domain.Entities;
using NotificationHub.Domain.Exceptions;

namespace NotificationHub.Application.Notifications.Services;

public interface INotificationService
{
    Task<NotificationResponse> SendAsync(SendNotificationRequest request, CancellationToken ct = default);
    Task<NotificationResponse?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<int> ProcessRetriesAsync(int batchSize = 50, CancellationToken ct = default);
}

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;
    private readonly ITemplateRepository _templates;
    private readonly ISenderResolver _senders;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository repository,
        ITemplateRepository templates,
        ISenderResolver senders,
        ILogger<NotificationService> logger)
    {
        _repository = repository;
        _templates = templates;
        _senders = senders;
        _logger = logger;
    }

    public async Task<NotificationResponse> SendAsync(SendNotificationRequest request, CancellationToken ct = default)
    {
        var notification = await BuildAsync(request, ct);

        await _repository.AddAsync(notification, ct);
        await _repository.SaveChangesAsync(ct);

        await DispatchAsync(notification, ct);
        await _repository.SaveChangesAsync(ct);

        return NotificationResponse.From(notification);
    }

    public async Task<NotificationResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var notification = await _repository.GetByIdAsync(id, ct);
        return notification is null ? null : NotificationResponse.From(notification);
    }

    public async Task<int> ProcessRetriesAsync(int batchSize = 50, CancellationToken ct = default)
    {
        var due = await _repository.GetPendingRetriesAsync(DateTime.UtcNow, batchSize, ct);

        foreach (var notification in due)
            await DispatchAsync(notification, ct);

        await _repository.SaveChangesAsync(ct);
        return due.Count;
    }

    private async Task<Notification> BuildAsync(SendNotificationRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.TemplateCode))
        {
            if (string.IsNullOrWhiteSpace(request.Body))
                throw new DomainException("Either a body or a template code must be provided.");

            return Notification.Create(request.Channel, request.Recipient, request.Body, request.Subject);
        }

        var template = await _templates.GetByCodeAsync(request.TemplateCode, ct)
            ?? throw new DomainException($"Template '{request.TemplateCode}' was not found.");

        if (!template.IsActive)
            throw new DomainException($"Template '{request.TemplateCode}' is inactive.");

        if (template.Channel != request.Channel)
            throw new DomainException($"Template '{request.TemplateCode}' belongs to channel {template.Channel}.");

        var body = template.Render(request.TemplateValues ?? new Dictionary<string, string>());

        return Notification.Create(request.Channel, request.Recipient, body, template.Subject, template.Id);
    }

    private async Task DispatchAsync(Notification notification, CancellationToken ct)
    {
        var sender = _senders.Resolve(notification.Channel);
        notification.MarkAsProcessing();

        try
        {
            var result = await sender.SendAsync(notification, ct);

            if (result.Succeeded)
            {
                notification.RecordSuccess(sender.ProviderName, result.ProviderMessageId);
                _logger.LogInformation("Notification {Id} sent via {Provider}.", notification.Id, sender.ProviderName);
            }
            else
            {
                notification.RecordFailure(sender.ProviderName, result.ErrorMessage ?? "Unknown provider error.");
                _logger.LogWarning("Notification {Id} failed: {Error}", notification.Id, result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            notification.RecordFailure(sender.ProviderName, ex.Message);
            _logger.LogError(ex, "Unexpected error sending notification {Id}.", notification.Id);
        }
    }
}