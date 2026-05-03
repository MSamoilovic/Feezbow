using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.Pantry_Events;

namespace Feezbow.Application.Features.Events.Pantry;

public class PantryItemDepletedEventHandler(IPantryNotificationService notifications)
    : INotificationHandler<PantryItemDepletedEvent>
{
    public Task Handle(PantryItemDepletedEvent notification, CancellationToken cancellationToken)
        => notifications.NotifyDepleted(
            notification.PantryItemId,
            notification.ProjectId,
            notification.Name,
            notification.ModifiedBy,
            cancellationToken);
}
