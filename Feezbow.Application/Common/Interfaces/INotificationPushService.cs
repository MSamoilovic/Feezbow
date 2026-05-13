namespace Feezbow.Application.Common.Interfaces;

public interface INotificationPushService
{
    Task PushAsync(long userId, long notificationId, string type, string title, string? body, CancellationToken cancellationToken = default);
    Task PushUnreadCountAsync(long userId, int unreadCount, CancellationToken cancellationToken = default);
}
