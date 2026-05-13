using MediatR;
using Feezbow.Domain.Events.Pantry_Events;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Events.Pantry;

internal class PantryItemDepletedNotificationHandler(INotificationDomainService notificationService)
    : INotificationHandler<PantryItemDepletedEvent>
{
    public Task Handle(PantryItemDepletedEvent notification, CancellationToken cancellationToken)
        => notificationService.CreateForProjectMembersAsync(
            notification.ProjectId,
            "pantry.depleted",
            "Pantry Item Depleted",
            $"{notification.Name} has been depleted.",
            notification.ModifiedBy,
            cancellationToken);
}
