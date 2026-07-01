using Microsoft.EntityFrameworkCore;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Enums;
using Feezbow.Domain.Interfaces.Repositories;

namespace Feezbow.Infrastructure.Persistence.Repositories;

public class AIInsightRepository(ApplicationDbContext dbContext) : IAIInsightRepository
{
    private readonly DbSet<AIInsight> _dbSet = dbContext.Set<AIInsight>();

    public async Task<IReadOnlyList<AIInsight>> GetActiveByProjectAsync(
        long projectId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(i => i.ProjectId == projectId && i.DismissedAt == null)
            .OrderByDescending(i => i.GeneratedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<AIInsight?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsActiveOfTypeSinceAsync(
        long projectId,
        AIInsightType type,
        DateTimeOffset since,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(
            i => i.ProjectId == projectId
                 && i.Type == type
                 && i.DismissedAt == null
                 && i.GeneratedAt >= since,
            cancellationToken);
    }

    public async Task AddAsync(AIInsight insight, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(insight, cancellationToken);
    }
}
