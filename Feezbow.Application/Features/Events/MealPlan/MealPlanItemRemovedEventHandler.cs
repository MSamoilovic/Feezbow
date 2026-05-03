using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.MealPlan_Events;

namespace Feezbow.Application.Features.Events.MealPlan;

public class MealPlanItemRemovedEventHandler(IMealPlanNotificationService notifications)
    : INotificationHandler<MealPlanItemRemovedEvent>
{
    public Task Handle(MealPlanItemRemovedEvent notification, CancellationToken cancellationToken)
        => notifications.NotifyItemRemoved(
            notification.MealPlanId,
            notification.ProjectId,
            notification.DayOfWeek,
            notification.MealType,
            notification.RemovedBy,
            cancellationToken);
}
