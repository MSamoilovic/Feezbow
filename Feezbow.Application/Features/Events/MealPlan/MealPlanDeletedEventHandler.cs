using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.MealPlan_Events;

namespace Feezbow.Application.Features.Events.MealPlan;

public class MealPlanDeletedEventHandler(IMealPlanNotificationService notifications)
    : INotificationHandler<MealPlanDeletedEvent>
{
    public Task Handle(MealPlanDeletedEvent notification, CancellationToken cancellationToken)
        => notifications.NotifyDeleted(
            notification.MealPlanId,
            notification.ProjectId,
            notification.DeletedBy,
            cancellationToken);
}
