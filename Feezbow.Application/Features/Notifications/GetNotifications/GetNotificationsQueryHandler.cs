using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Notifications.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Notifications.GetNotifications;

public class GetNotificationsQueryHandler(
    INotificationDomainService notificationService,
    ICurrentUserService currentUserService,
    ICacheService cacheService) : IRequestHandler<GetNotificationsQuery, GetNotificationsQueryResponse>
{
    public async Task<GetNotificationsQueryResponse> Handle(
        GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId!.Value;

        var result = await cacheService.GetOrSetAsync(
            CacheKeys.UserNotifications(userId, request.UnreadOnly, request.Page, request.PageSize),
            async () =>
            {
                var (items, total) = await notificationService.GetByUserAsync(
                    userId, request.UnreadOnly, request.Page, request.PageSize, cancellationToken);
                return new GetNotificationsQueryResponse(
                    [.. items.Select(NotificationDto.FromEntity)],
                    total, request.Page, request.PageSize);
            },
            CacheExpiration.Short, cancellationToken);

        if (result is not null) return result;

        var (items2, total2) = await notificationService.GetByUserAsync(
            userId, request.UnreadOnly, request.Page, request.PageSize, cancellationToken);
        return new GetNotificationsQueryResponse(
            [.. items2.Select(NotificationDto.FromEntity)],
            total2, request.Page, request.PageSize);
    }
}
