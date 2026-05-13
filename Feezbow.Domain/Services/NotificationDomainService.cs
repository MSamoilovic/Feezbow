using Feezbow.Domain.Entities;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Domain.Services;

public class NotificationDomainService(IUnitOfWork unitOfWork) : INotificationDomainService
{
    public async Task<Notification> CreateAsync(
        long userId, long projectId, string type, string title, string? body, long createdBy,
        CancellationToken cancellationToken = default)
    {
        var notification = Notification.Create(userId, projectId, type, title, body, createdBy);
        await unitOfWork.Notifications.AddAsync(notification, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return notification;
    }

    public async Task CreateForProjectMembersAsync(
        long projectId, string type, string title, string? body, long excludeUserId,
        CancellationToken cancellationToken = default)
    {
        var project = await unitOfWork.Projects.GetProjectWithMembersAsync(projectId, cancellationToken);
        if (project is null) return;

        var notifications = project.ProjectMembers
            .Select(m => m.UserId)
            .Where(id => id.HasValue && id.Value != excludeUserId)
            .Select(uid => Notification.Create(uid!.Value, projectId, type, title, body, excludeUserId))
            .ToList();

        if (notifications.Count == 0) return;

        await unitOfWork.Notifications.AddRangeAsync(notifications, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Notification> Items, int TotalCount)> GetByUserAsync(
        long userId, bool unreadOnly, int page, int pageSize,
        CancellationToken cancellationToken = default)
    {
        var items = await unitOfWork.Notifications.GetByUserAsync(userId, unreadOnly, page, pageSize, cancellationToken);
        var total = await unitOfWork.Notifications.CountAsync(userId, unreadOnly, cancellationToken);
        return (items, total);
    }

    public Task<int> CountUnreadAsync(long userId, CancellationToken cancellationToken = default)
        => unitOfWork.Notifications.CountAsync(userId, unreadOnly: true, cancellationToken);

    public async Task MarkReadAsync(long notificationId, long userId, CancellationToken cancellationToken = default)
    {
        var notification = await unitOfWork.Notifications.GetByIdAsync(notificationId, cancellationToken)
            ?? throw new NotFoundException("Notification", notificationId);

        if (notification.UserId != userId)
            throw new AccessDeniedException("You can only mark your own notifications as read.");

        notification.MarkAsRead();
        await unitOfWork.CompleteAsync(cancellationToken);
    }

    public Task MarkAllReadAsync(long userId, CancellationToken cancellationToken = default)
        => unitOfWork.Notifications.MarkAllReadAsync(userId, cancellationToken);
}
