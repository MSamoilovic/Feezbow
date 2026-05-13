using MediatR;
using Microsoft.EntityFrameworkCore;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Comment_Events;
using Feezbow.Domain.Interfaces.Repositories;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Events.Comment;

internal class UserMentionedNotificationHandler(
    INotificationDomainService notificationService,
    INotificationPushService pushService,
    IUserRepository userRepository,
    IBoardTaskRepository boardTaskRepository,
    IBoardRepository boardRepository)
    : INotificationHandler<UserMentionedInCommentEvent>
{
    public async Task Handle(UserMentionedInCommentEvent notification, CancellationToken cancellationToken)
    {
        var usernames = notification.MentionedUsernames.ToList();

        var mentionedUserIds = await userRepository.SearchUsers()
            .Where(u => u.IsActive && usernames.Contains(u.UserName!))
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        if (mentionedUserIds.Count == 0)
            return;

        var boardId = await boardTaskRepository.GetBoardIdByTaskIdAsync(notification.TaskId, cancellationToken);
        var board = await boardRepository.GetByIdAsync(boardId, cancellationToken);
        if (board is null) return;

        foreach (var userId in mentionedUserIds)
        {
            if (userId == notification.MentionedByUserId)
                continue;

            var persisted = await notificationService.CreateAsync(
                userId,
                board.ProjectId,
                "user.mentioned",
                "You Were Mentioned",
                "Someone mentioned you in a comment.",
                notification.MentionedByUserId,
                cancellationToken);

            var unreadCount = await notificationService.CountUnreadAsync(userId, cancellationToken);

            await pushService.PushAsync(userId, persisted.Id, persisted.Type, persisted.Title, persisted.Body, cancellationToken);
            await pushService.PushUnreadCountAsync(userId, unreadCount, cancellationToken);
        }
    }
}
