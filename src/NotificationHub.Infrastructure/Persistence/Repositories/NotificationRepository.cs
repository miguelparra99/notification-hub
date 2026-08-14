using Microsoft.EntityFrameworkCore;
using NotificationHub.Application.Common.Interfaces;
using NotificationHub.Domain.Entities;
using NotificationHub.Domain.Enums;

namespace NotificationHub.Infrastructure.Persistence.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _context;

    public NotificationRepository(NotificationDbContext context) => _context = context;

    public Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _context.Notifications
            .Include(n => n.Attempts)
            .FirstOrDefaultAsync(n => n.Id == id, ct);

    public async Task<IReadOnlyList<Notification>> GetPendingRetriesAsync(DateTime now, int batchSize, CancellationToken ct = default) =>
        await _context.Notifications
            .Include(n => n.Attempts)
            .Where(n => n.Status == NotificationStatus.Failed
                     && n.NextRetryAt != null
                     && n.NextRetryAt <= now)
            .OrderBy(n => n.NextRetryAt)
            .Take(batchSize)
            .ToListAsync(ct);

    public async Task AddAsync(Notification notification, CancellationToken ct = default) =>
        await _context.Notifications.AddAsync(notification, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
}