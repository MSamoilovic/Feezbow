using Microsoft.EntityFrameworkCore;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Infrastructure.Persistence.Repositories;

public class RecipeRepository(ApplicationDbContext dbContext) : IRecipeRepository
{
    private readonly DbSet<Recipe> _dbSet = dbContext.Set<Recipe>();

    public async Task<Recipe?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.Project)
            .ThenInclude(p => p.ProjectMembers)
            .Include(r => r.Ingredients)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Recipe>> GetByProjectAsync(
        long projectId, int skip, int take, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.Ingredients)
            .Where(r => r.ProjectId == projectId)
            .OrderBy(r => r.Name)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsInProjectAsync(
        long recipeId, long projectId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(
            r => r.Id == recipeId && r.ProjectId == projectId, cancellationToken);
    }

    public async Task AddAsync(Recipe recipe, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(recipe, cancellationToken);
    }

    public void Remove(Recipe recipe)
    {
        _dbSet.Remove(recipe);
    }
}
