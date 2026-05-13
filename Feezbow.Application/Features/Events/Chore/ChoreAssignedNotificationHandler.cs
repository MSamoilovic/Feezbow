using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Chore_Events;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Events.Chore;

internal class ChoreAssignedNotificationHandler(
    INotificationDomainService notificationService,
    INotificationPushService pushService)
    : INotificationHandler<ChoreAssignedEvent>
{
    public async Task Handle(ChoreAssignedEvent notification, CancellationToken cancellationToken)
    {
        if (!notification.AssignedToUserId.HasValue)
            return;

        var userId = notification.AssignedToUserId.Value;

        var persisted = await notificationService.CreateAsync(
            userId,
            notification.ProjectId,
            "chore.assigned",
            "Chore Assigned",
            "A chore has been assigned to you.",
            notification.AssignedBy,
            cancellationToken);

        var unreadCount = await notificationService.CountUnreadAsync(userId, cancellationToken);

        await pushService.PushAsync(userId, persisted.Id, persisted.Type, persisted.Title, persisted.Body, cancellationToken);
        await pushService.PushUnreadCountAsync(userId, unreadCount, cancellationToken);
    }
}
