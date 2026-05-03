using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Pantry_Events;

namespace Feezbow.Application.Features.Events.Pantry;

public class PantryItemRemovedEventHandler(IPantryNotificationService notifications)
    : INotificationHandler<PantryItemRemovedEvent>
{
    public Task Handle(PantryItemRemovedEvent notification, CancellationToken cancellationToken)
        => notifications.NotifyRemoved(
            notification.PantryItemId,
            notification.ProjectId,
            notification.RemovedBy,
            cancellationToken);
}
