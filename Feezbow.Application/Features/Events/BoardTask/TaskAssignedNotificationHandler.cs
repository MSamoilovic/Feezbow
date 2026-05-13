using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.TaskEvents;
using Feezbow.Domain.Interfaces.Repositories;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Events.BoardTask;

internal class TaskAssignedNotificationHandler(
    INotificationDomainService notificationService,
    INotificationPushService pushService,
    IBoardTaskRepository boardTaskRepository,
    IBoardRepository boardRepository)
    : INotificationHandler<TaskAssignedEvent>
{
    public async Task Handle(TaskAssignedEvent notification, CancellationToken cancellationToken)
    {
        if (!notification.NewAssigneeId.HasValue)
            return;

        var userId = notification.NewAssigneeId.Value;

        var boardId = await boardTaskRepository.GetBoardIdByTaskIdAsync(notification.TaskId, cancellationToken);
        var board = await boardRepository.GetByIdAsync(boardId, cancellationToken);
        if (board is null) return;

        var persisted = await notificationService.CreateAsync(
            userId,
            board.ProjectId,
            "task.assigned",
            "Task Assigned",
            "A task has been assigned to you.",
            notification.AssignedByUserId,
            cancellationToken);

        var unreadCount = await notificationService.CountUnreadAsync(userId, cancellationToken);

        await pushService.PushAsync(userId, persisted.Id, persisted.Type, persisted.Title, persisted.Body, cancellationToken);
        await pushService.PushUnreadCountAsync(userId, unreadCount, cancellationToken);
    }
}
