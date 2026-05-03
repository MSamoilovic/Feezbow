using MediatR;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Events.MealPlan_Events;

namespace Feezbow.Application.Features.Events.MealPlan;

public class MealPlanCreatedEventHandler(IMealPlanNotificationService notifications)
    : INotificationHandler<MealPlanCreatedEvent>
{
    public Task Handle(MealPlanCreatedEvent notification, CancellationToken cancellationToken)
        => notifications.NotifyCreated(
            notification.MealPlanId,
            notification.ProjectId,
            notification.WeekStart,
            notification.CreatedBy,
            cancellationToken);
}
