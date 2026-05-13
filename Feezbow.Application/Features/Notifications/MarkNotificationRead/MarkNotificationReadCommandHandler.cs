using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Notifications.MarkNotificationRead;

public class MarkNotificationReadCommandHandler(
    INotificationDomainService notificationService,
    ICurrentUserService currentUserService,
    ICacheService cacheService) : IRequestHandler<MarkNotificationReadCommand>
{
    public async Task Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId!.Value;

        await notificationService.MarkReadAsync(request.NotificationId, userId, cancellationToken);

        await cacheService.RemoveByPrefixAsync(CacheKeys.UserNotificationsPrefix(userId), cancellationToken);
    }
}
