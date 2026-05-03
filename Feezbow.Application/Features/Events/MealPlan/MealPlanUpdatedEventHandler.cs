using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.MealPlan_Events;

namespace Feezbow.Application.Features.Events.MealPlan;

public class MealPlanUpdatedEventHandler(IMealPlanNotificationService notifications)
    : INotificationHandler<MealPlanUpdatedEvent>
{
    public Task Handle(MealPlanUpdatedEvent notification, CancellationToken cancellationToken)
        => notifications.NotifyUpdated(
            notification.MealPlanId,
            notification.ProjectId,
            notification.ModifiedBy,
            cancellationToken);
}
