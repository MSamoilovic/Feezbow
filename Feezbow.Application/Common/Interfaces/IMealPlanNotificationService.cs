using Feezbow.Domain.Enums;

namespace Feezbow.Application.Common.Interfaces;

public interface IMealPlanNotificationService
{
    Task NotifyCreated(long mealPlanId, long projectId, DateTime weekStart, long createdBy, CancellationToken cancellationToken = default);
    Task NotifyUpdated(long mealPlanId, long projectId, long modifiedBy, CancellationToken cancellationToken = default);
    Task NotifyDeleted(long mealPlanId, long projectId, long deletedBy, CancellationToken cancellationToken = default);

    Task NotifyItemSet(
        long mealPlanId,
        long projectId,
        long itemId,
        DayOfWeek dayOfWeek,
        MealType mealType,
        string title,
        long? assignedCookId,
        bool isReplacement,
        long modifiedBy,
        CancellationToken cancellationToken = default);

    Task NotifyItemRemoved(
        long mealPlanId,
        long projectId,
        DayOfWeek dayOfWeek,
        MealType mealType,
        long removedBy,
        CancellationToken cancellationToken = default);
}
