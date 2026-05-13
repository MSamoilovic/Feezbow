using Feezbow.Application.Features.Notifications.Common;

namespace Feezbow.Application.Features.Notifications.GetNotifications;

public record GetNotificationsQueryResponse(
    IReadOnlyList<NotificationDto> Items,
    int TotalCount,
    int Page,
    int PageSize);
