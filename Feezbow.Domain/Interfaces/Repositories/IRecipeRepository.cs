using Feezbow.Domain.Entities;

namespace Feezbow.Domain.Interfaces.Repositories;

public interface IRecipeRepository
{
    Task<Recipe?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Recipe>> GetByProjectAsync(
        long projectId,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsInProjectAsync(
        long recipeId,
        long projectId,
        CancellationToken cancellationToken = default);

    Task AddAsync(Recipe recipe, CancellationToken cancellationToken = default);
    void Remove(Recipe recipe);
}
