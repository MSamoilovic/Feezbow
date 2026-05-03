using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Pantry_Events;

namespace Feezbow.Application.Features.Events.Pantry;

public class PantryItemAddedEventHandler(IPantryNotificationService notifications)
    : INotificationHandler<PantryItemAddedEvent>
{
    public Task Handle(PantryItemAddedEvent notification, CancellationToken cancellationToken)
        => notifications.NotifyAdded(
            notification.PantryItemId,
            notification.ProjectId,
            notification.Name,
            notification.Quantity,
            notification.Unit,
            notification.AddedBy,
            cancellationToken);
}
