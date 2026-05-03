using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.MealPlan_Events;

namespace Feezbow.Application.Features.Events.MealPlan;

public class MealPlanItemSetEventHandler(IMealPlanNotificationService notifications)
    : INotificationHandler<MealPlanItemSetEvent>
{
    public Task Handle(MealPlanItemSetEvent notification, CancellationToken cancellationToken)
        => notifications.NotifyItemSet(
            notification.MealPlanId,
            notification.ProjectId,
            notification.ItemId,
            notification.DayOfWeek,
            notification.MealType,
            notification.Title,
            notification.AssignedCookId,
            notification.IsReplacement,
            notification.ModifiedBy,
            cancellationToken);
}
