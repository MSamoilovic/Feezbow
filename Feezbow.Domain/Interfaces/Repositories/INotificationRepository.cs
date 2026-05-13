using Feezbow.Domain.Entities;

namespace Feezbow.Domain.Interfaces.Repositories;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Notification>> GetByUserAsync(long userId, bool unreadOnly, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(long userId, bool unreadOnly, CancellationToken cancellationToken = default);
    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken = default);
    Task MarkAllReadAsync(long userId, CancellationToken cancellationToken = default);
}
