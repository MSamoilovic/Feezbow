using Feezbow.Domain.Entities;
using Feezbow.Domain.Enums;

namespace Feezbow.Domain.Interfaces.Services;

public interface IMealPlanService
{
    Task<MealPlan> CreateAsync(
        long projectId,
        long userId,
        DateTime weekStart,
        string? notes,
        CancellationToken cancellationToken = default);

    Task<MealPlan?> GetByWeekAsync(
        long projectId,
        long userId,
        DateTime weekStart,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MealPlan>> GetRecentAsync(
        long projectId,
        long userId,
        int count,
        CancellationToken cancellationToken = default);

    Task<long> UpdateAsync(
        long mealPlanId,
        long userId,
        string? notes,
        CancellationToken cancellationToken = default);

    Task<long> DeleteAsync(
        long mealPlanId,
        long userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Upserts the meal slot at (<paramref name="dayOfWeek"/>, <paramref name="mealType"/>).
    /// Returns the saved item alongside the plan's projectId so callers can scope cache invalidation.
    /// </summary>
    Task<(MealPlanItem Item, long ProjectId)> SetSlotAsync(
        long mealPlanId,
        long userId,
        DayOfWeek dayOfWeek,
        MealType mealType,
        string title,
        string? notes,
        long? assignedCookId,
        long? recipeId,
        CancellationToken cancellationToken = default);

    Task<long> RemoveSlotAsync(
        long mealPlanId,
        long userId,
        DayOfWeek dayOfWeek,
        MealType mealType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Aggregates ingredients from every recipe-linked slot in the plan and writes them into a brand-new
    /// shopping list. Free-text slots (no Recipe) are ignored. Returns the created list.
    /// </summary>
    Task<ShoppingList> GenerateShoppingListAsync(
        long mealPlanId,
        long userId,
        string? listName,
        CancellationToken cancellationToken = default);
}
