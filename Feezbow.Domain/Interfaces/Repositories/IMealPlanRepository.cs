using Feezbow.Domain.Entities;

namespace Feezbow.Domain.Interfaces.Repositories;

public interface IMealPlanRepository
{
    Task<MealPlan?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a meal plan with its items, each item's recipe, and each recipe's ingredients —
    /// the full graph required to generate an aggregated shopping list. Heavier than <see cref="GetByIdAsync"/>;
    /// use only for that operation.
    /// </summary>
    Task<MealPlan?> GetByIdWithRecipeIngredientsAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<MealPlan?> GetByProjectAndWeekAsync(
        long projectId,
        DateTime weekStart,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MealPlan>> GetRecentByProjectAsync(
        long projectId,
        int count,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsForWeekAsync(
        long projectId,
        DateTime weekStart,
        CancellationToken cancellationToken = default);

    Task AddAsync(MealPlan plan, CancellationToken cancellationToken = default);
    void Remove(MealPlan plan);
}
