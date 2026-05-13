using Microsoft.EntityFrameworkCore;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Infrastructure.Persistence.Repositories;

public class NotificationRepository(ApplicationDbContext dbContext) : INotificationRepository
{
    private readonly DbSet<Notification> _dbSet = dbContext.Set<Notification>();

    public async Task<Notification?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => await _dbSet.FirstOrDefaultAsync(n => n.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Notification>> GetByUserAsync(
        long userId, bool unreadOnly, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(n => n.UserId == userId);
        if (unreadOnly)
            query = query.Where(n => !n.IsRead);

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(long userId, bool unreadOnly, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(n => n.UserId == userId);
        if (unreadOnly)
            query = query.Where(n => !n.IsRead);

        return await query.CountAsync(cancellationToken);
    }

    public async Task AddAsync(Notification notification, CancellationToken cancellationToken = default)
        => await _dbSet.AddAsync(notification, cancellationToken);

    public async Task AddRangeAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken = default)
        => await _dbSet.AddRangeAsync(notifications, cancellationToken);

    public async Task MarkAllReadAsync(long userId, CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.ReadAt, DateTime.UtcNow)
                .SetProperty(n => n.LastModifiedAt, DateTime.UtcNow),
            cancellationToken);
}
