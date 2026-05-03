using Microsoft.EntityFrameworkCore;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Infrastructure.Persistence.Repositories;

public class MealPlanRepository(ApplicationDbContext dbContext) : IMealPlanRepository
{
    private readonly DbSet<MealPlan> _dbSet = dbContext.Set<MealPlan>();

    public async Task<MealPlan?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Project)
            .ThenInclude(p => p.ProjectMembers)
            .Include(p => p.Items).ThenInclude(i => i.AssignedCook)
            .Include(p => p.Items).ThenInclude(i => i.Recipe)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<MealPlan?> GetByIdWithRecipeIngredientsAsync(
        long id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Project)
            .ThenInclude(p => p.ProjectMembers)
            .Include(p => p.Items).ThenInclude(i => i.Recipe).ThenInclude(r => r!.Ingredients)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<MealPlan?> GetByProjectAndWeekAsync(
        long projectId,
        DateTime weekStart,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Items).ThenInclude(i => i.AssignedCook)
            .Include(p => p.Items).ThenInclude(i => i.Recipe)
            .FirstOrDefaultAsync(p => p.ProjectId == projectId && p.WeekStart == weekStart, cancellationToken);
    }

    public async Task<IReadOnlyList<MealPlan>> GetRecentByProjectAsync(
        long projectId,
        int count,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Items).ThenInclude(i => i.AssignedCook)
            .Include(p => p.Items).ThenInclude(i => i.Recipe)
            .Where(p => p.ProjectId == projectId)
            .OrderByDescending(p => p.WeekStart)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsForWeekAsync(
        long projectId,
        DateTime weekStart,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(
            p => p.ProjectId == projectId && p.WeekStart == weekStart, cancellationToken);
    }

    public async Task AddAsync(MealPlan plan, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(plan, cancellationToken);
    }

    public void Remove(MealPlan plan)
    {
        _dbSet.Remove(plan);
    }
}
