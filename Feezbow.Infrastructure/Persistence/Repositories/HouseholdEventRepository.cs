using Microsoft.EntityFrameworkCore;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Infrastructure.Persistence.Repositories;

public class HouseholdEventRepository(ApplicationDbContext dbContext) : IHouseholdEventRepository
{
    private readonly DbSet<HouseholdEvent> _dbSet = dbContext.Set<HouseholdEvent>();

    public async Task<HouseholdEvent?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(e => e.Project)
            .ThenInclude(p => p.ProjectMembers)
            .Include(e => e.AssignedTo)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<HouseholdEvent>> GetByProjectAndDateRangeAsync(
        long projectId, DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(e => e.AssignedTo)
            .Where(e => e.ProjectId == projectId
                && e.StartDate < to
                && (e.EndDate == null ? e.StartDate >= from : e.EndDate >= from))
            .OrderBy(e => e.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(HouseholdEvent householdEvent, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(householdEvent, cancellationToken);
    }

    public void Remove(HouseholdEvent householdEvent)
    {
        _dbSet.Remove(householdEvent);
    }
}
