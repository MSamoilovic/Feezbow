using Feezbow.Domain.Entities;

namespace Feezbow.Domain.Interfaces.Services;

public interface INotificationDomainService
{
    Task<Notification> CreateAsync(long userId, long projectId, string type, string title, string? body, long createdBy, CancellationToken cancellationToken = default);
    Task CreateForProjectMembersAsync(long projectId, string type, string title, string? body, long excludeUserId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Notification> Items, int TotalCount)> GetByUserAsync(long userId, bool unreadOnly, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountUnreadAsync(long userId, CancellationToken cancellationToken = default);
    Task MarkReadAsync(long notificationId, long userId, CancellationToken cancellationToken = default);
    Task MarkAllReadAsync(long userId, CancellationToken cancellationToken = default);
}
