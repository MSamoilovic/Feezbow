using MediatR;

namespace Feezbow.Application.Features.Notifications.GetNotifications;

public record GetNotificationsQuery(
    bool UnreadOnly = false,
    int Page = 1,
    int PageSize = 20) : IRequest<GetNotificationsQueryResponse>;
