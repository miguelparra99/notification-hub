using NotificationHub.Domain.Entities;

namespace NotificationHub.Application.Common.Interfaces;

public interface ITemplateRepository
{
    Task<NotificationTemplate?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task AddAsync(NotificationTemplate template, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}