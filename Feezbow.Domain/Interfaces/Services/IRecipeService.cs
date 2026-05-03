using Feezbow.Domain.Entities;

namespace Feezbow.Domain.Interfaces.Services;

public interface IRecipeService
{
    Task<Recipe> CreateAsync(
        long projectId,
        long userId,
        string name,
        string? description,
        int servings,
        int? prepTimeMinutes,
        int? cookTimeMinutes,
        string? instructions,
        string? sourceUrl,
        IReadOnlyList<(string Name, decimal Quantity, string? Unit, string? Notes)> ingredients,
        CancellationToken cancellationToken = default);

    Task<Recipe> GetAsync(
        long recipeId,
        long userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Recipe>> GetByProjectAsync(
        long projectId,
        long userId,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<long> UpdateAsync(
        long recipeId,
        long userId,
        string? name,
        string? description,
        int? servings,
        int? prepTimeMinutes,
        int? cookTimeMinutes,
        string? instructions,
        string? sourceUrl,
        CancellationToken cancellationToken = default);

    Task<long> ReplaceIngredientsAsync(
        long recipeId,
        long userId,
        IReadOnlyList<(string Name, decimal Quantity, string? Unit, string? Notes)> ingredients,
        CancellationToken cancellationToken = default);

    Task<long> DeleteAsync(
        long recipeId,
        long userId,
        CancellationToken cancellationToken = default);
}
