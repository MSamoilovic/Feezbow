using MediatR;

namespace Feezbow.Application.Features.Notifications.MarkNotificationRead;

public record MarkNotificationReadCommand(long NotificationId) : IRequest;
