using Microsoft.EntityFrameworkCore;
using NotificationHub.Application.Common.Interfaces;
using NotificationHub.Domain.Entities;

namespace NotificationHub.Infrastructure.Persistence.Repositories;

public class TemplateRepository : ITemplateRepository
{
    private readonly NotificationDbContext _context;

    public TemplateRepository(NotificationDbContext context) => _context = context;

    public Task<NotificationTemplate?> GetByCodeAsync(string code, CancellationToken ct = default) =>
        _context.Templates.FirstOrDefaultAsync(t => t.Code == code.ToUpperInvariant(), ct);

    public async Task AddAsync(NotificationTemplate template, CancellationToken ct = default) =>
        await _context.Templates.AddAsync(template, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
}