using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Pantry_Events;

namespace Feezbow.Application.Features.Events.Pantry;

public class PantryItemQuantityChangedEventHandler(IPantryNotificationService notifications)
    : INotificationHandler<PantryItemQuantityChangedEvent>
{
    public Task Handle(PantryItemQuantityChangedEvent notification, CancellationToken cancellationToken)
        => notifications.NotifyQuantityChanged(
            notification.PantryItemId,
            notification.ProjectId,
            notification.PreviousQuantity,
            notification.NewQuantity,
            notification.Delta,
            notification.ModifiedBy,
            cancellationToken);
}
