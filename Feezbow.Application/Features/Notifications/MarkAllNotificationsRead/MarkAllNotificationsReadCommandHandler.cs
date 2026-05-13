using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Notifications.MarkAllNotificationsRead;

public class MarkAllNotificationsReadCommandHandler(
    INotificationDomainService notificationService,
    ICurrentUserService currentUserService,
    ICacheService cacheService) : IRequestHandler<MarkAllNotificationsReadCommand>
{
    public async Task Handle(MarkAllNotificationsReadCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId!.Value;

        await notificationService.MarkAllReadAsync(userId, cancellationToken);

        await cacheService.RemoveByPrefixAsync(CacheKeys.UserNotificationsPrefix(userId), cancellationToken);
    }
}
