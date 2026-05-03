using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Pantry_Events;

namespace Feezbow.Application.Features.Events.Pantry;

public class PantryItemUpdatedEventHandler(IPantryNotificationService notifications)
    : INotificationHandler<PantryItemUpdatedEvent>
{
    public Task Handle(PantryItemUpdatedEvent notification, CancellationToken cancellationToken)
        => notifications.NotifyUpdated(
            notification.PantryItemId,
            notification.ProjectId,
            notification.ModifiedBy,
            cancellationToken);
}
