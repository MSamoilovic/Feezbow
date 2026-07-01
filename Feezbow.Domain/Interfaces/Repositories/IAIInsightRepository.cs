using Feezbow.Domain.Entities;
using Feezbow.Domain.Enums;

namespace Feezbow.Domain.Interfaces.Repositories;

public interface IAIInsightRepository
{
    /// <summary>Nedismiss-ovani uvidi projekta, najnoviji prvo.</summary>
    Task<IReadOnlyList<AIInsight>> GetActiveByProjectAsync(long projectId, CancellationToken cancellationToken = default);

    Task<AIInsight?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>Dedup guard — postoji li aktivan (nedismiss-ovan) uvid istog tipa generisan posle <paramref name="since"/>.</summary>
    Task<bool> ExistsActiveOfTypeSinceAsync(
        long projectId,
        AIInsightType type,
        DateTimeOffset since,
        CancellationToken cancellationToken = default);

    Task AddAsync(AIInsight insight, CancellationToken cancellationToken = default);
}
