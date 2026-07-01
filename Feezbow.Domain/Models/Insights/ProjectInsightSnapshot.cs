using Feezbow.Domain.Enums;

namespace Feezbow.Domain.Models.Insights;

/// <summary>
/// Faktografski snimak stanja jednog projekta u trenutku analize.
/// Domain servis ga sastavlja; Application handler ga serijalizuje i šalje AI-ju.
/// Sadrži samo činjenice — nikakvu interpretaciju (to je posao AI-ja).
/// </summary>
public class ProjectInsightSnapshot
{
    public long ProjectId { get; init; }
    public DateTime GeneratedAtUtc { get; init; }

    public IReadOnlyList<OverdueBillEntry> OverdueBills { get; init; } = [];
    public IReadOnlyList<ChoreBacklogEntry> ChoresBacklog { get; init; } = [];
    public IReadOnlyList<ExpiringPantryEntry> ExpiringPantryItems { get; init; } = [];
    public IReadOnlyList<DateOnly> MealPlanGapDays { get; init; } = [];

    /// <summary>True kada nema nijedne anomalije — handler tada preskače AI poziv.</summary>
    public bool IsEmpty =>
        OverdueBills.Count == 0
        && ChoresBacklog.Count == 0
        && ExpiringPantryItems.Count == 0
        && MealPlanGapDays.Count == 0;
}

public record OverdueBillEntry(
    string Title,
    decimal Amount,
    string Currency,
    DateOnly DueDate,
    int DaysOverdue);

public record ChoreBacklogEntry(
    string Title,
    DateOnly? DueDate,
    bool IsOverdue,
    string Priority);

public record ExpiringPantryEntry(
    string Name,
    decimal Quantity,
    string? Unit,
    DateOnly ExpirationDate,
    int DaysUntilExpiry);

/// <summary>
/// Uvid koji AI predloži, pripremljen za perzistenciju. Application handler mapira
/// parsirani AI odgovor u ovu strukturu i predaje je <c>IInsightService.SaveInsightsAsync</c>.
/// </summary>
public record NewInsight(
    AIInsightType Type,
    AIInsightSeverity Severity,
    string Title,
    string Description);
