using Feezbow.Domain.Entities;
using Feezbow.Domain.Models.Insights;

namespace Feezbow.Domain.Interfaces.Services;

public interface IInsightService
{
    /// <summary>Sastavlja faktografski snimak stanja projekta (računi, poslovi, ostava, meal plan).</summary>
    Task<ProjectInsightSnapshot> BuildSnapshotAsync(long projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Perzistuje AI-predložene uvide uz dedup — preskače tip za koji već postoji aktivan
    /// uvid iz zadnjih 24h. Vraća stvarno sačuvane uvide (za SignalR push u handleru).
    /// </summary>
    Task<IReadOnlyList<AIInsight>> SaveInsightsAsync(
        long projectId,
        IReadOnlyList<NewInsight> insights,
        CancellationToken cancellationToken = default);

    /// <summary>Aktivni (nedismiss-ovani) uvidi projekta.</summary>
    Task<IReadOnlyList<AIInsight>> GetActiveInsightsAsync(long projectId, CancellationToken cancellationToken = default);

    /// <summary>Korisnik odbacuje uvid.</summary>
    Task DismissInsightAsync(long insightId, long userId, CancellationToken cancellationToken = default);
}
