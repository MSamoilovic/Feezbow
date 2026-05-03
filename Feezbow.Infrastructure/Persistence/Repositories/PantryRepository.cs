using Microsoft.EntityFrameworkCore;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Infrastructure.Persistence.Repositories;

public class PantryRepository(ApplicationDbContext dbContext) : IPantryRepository
{
    private readonly DbSet<PantryItem> _dbSet = dbContext.Set<PantryItem>();

    public async Task<PantryItem?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Project)
            .ThenInclude(p => p.ProjectMembers)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<PantryItem>> GetByProjectAsync(
        long projectId,
        string? search,
        string? location,
        int? expiringWithinDays,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(p => p.ProjectId == projectId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(p => EF.Functions.ILike(p.Name, pattern));
        }

        if (!string.IsNullOrWhiteSpace(location))
        {
            var normalized = location.Trim();
            query = query.Where(p => p.Location != null && p.Location == normalized);
        }

        if (expiringWithinDays is > 0)
        {
            var cutoff = DateTime.UtcNow.Date.AddDays(expiringWithinDays.Value);
            query = query.Where(p => p.ExpirationDate.HasValue && p.ExpirationDate.Value.Date <= cutoff);
        }

        return await query
            .OrderBy(p => p.ExpirationDate ?? DateTime.MaxValue)
            .ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(PantryItem item, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(item, cancellationToken);
    }

    public void Remove(PantryItem item)
    {
        _dbSet.Remove(item);
    }
}
