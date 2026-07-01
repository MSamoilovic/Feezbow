using Feezbow.Domain.Entities;
using Feezbow.Domain.Enums;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;
using Feezbow.Domain.Interfaces.Repositories;
using Feezbow.Domain.Interfaces.Services;
using Feezbow.Domain.Models.Insights;

namespace Feezbow.Domain.Services;

public class InsightService(
    IUnitOfWork unitOfWork,
    IPantryRepository pantryRepository,
    IAIInsightRepository insightRepository) : IInsightService
{
    private const int PantryExpiryWindowDays = 5;
    private const int MealPlanHorizonDays = 7;
    private static readonly TimeSpan DedupWindow = TimeSpan.FromHours(24);

    public async Task<ProjectInsightSnapshot> BuildSnapshotAsync(
        long projectId,
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        // Sve read-ove pokrećemo paralelno.
        var billsTask = unitOfWork.Bills.GetByProjectAsync(projectId, includePaid: false, cancellationToken);
        var choresTask = unitOfWork.Chores.GetByProjectAsync(projectId, includeCompleted: false, cancellationToken);
        var pantryTask = pantryRepository.GetByProjectAsync(
            projectId, null, null, PantryExpiryWindowDays, cancellationToken);
        var mealPlansTask = unitOfWork.MealPlans.GetByProjectAndDateRangeAsync(
            projectId,
            DateTime.UtcNow.Date.AddDays(-MealPlanHorizonDays),
            DateTime.UtcNow.Date.AddDays(MealPlanHorizonDays),
            cancellationToken);

        await Task.WhenAll(billsTask, choresTask, pantryTask, mealPlansTask);

        var overdueBills = (await billsTask)
            .Where(b => DateOnly.FromDateTime(b.DueDate) < today)
            .Select(b => new OverdueBillEntry(
                b.Title,
                b.Amount,
                b.Currency,
                DateOnly.FromDateTime(b.DueDate),
                today.DayNumber - DateOnly.FromDateTime(b.DueDate).DayNumber))
            .ToList();

        var choresBacklog = (await choresTask)
            .Select(c =>
            {
                DateOnly? due = c.DueDate.HasValue ? DateOnly.FromDateTime(c.DueDate.Value) : null;
                return new ChoreBacklogEntry(
                    c.Title,
                    due,
                    due.HasValue && due.Value < today,
                    c.Priority.ToString());
            })
            .ToList();

        var expiringPantry = (await pantryTask)
            .Where(p => p.ExpirationDate.HasValue)
            .Select(p =>
            {
                var exp = DateOnly.FromDateTime(p.ExpirationDate!.Value);
                return new ExpiringPantryEntry(
                    p.Name,
                    p.Quantity,
                    p.Unit,
                    exp,
                    exp.DayNumber - today.DayNumber);
            })
            .ToList();

        var mealPlanGapDays = ComputeMealPlanGaps(await mealPlansTask, today);

        return new ProjectInsightSnapshot
        {
            ProjectId = projectId,
            GeneratedAtUtc = DateTime.UtcNow,
            OverdueBills = overdueBills,
            ChoresBacklog = choresBacklog,
            ExpiringPantryItems = expiringPantry,
            MealPlanGapDays = mealPlanGapDays,
        };
    }

    public async Task<IReadOnlyList<AIInsight>> SaveInsightsAsync(
        long projectId,
        IReadOnlyList<NewInsight> insights,
        CancellationToken cancellationToken = default)
    {
        var since = DateTimeOffset.UtcNow - DedupWindow;
        var saved = new List<AIInsight>();
        var seenTypes = new HashSet<AIInsightType>();

        foreach (var candidate in insights)
        {
            // Dedup: preskoči tip koji je već u ovom batch-u ili već ima aktivan uvid iz zadnjih 24h.
            if (!seenTypes.Add(candidate.Type))
                continue;

            if (await insightRepository.ExistsActiveOfTypeSinceAsync(
                    projectId, candidate.Type, since, cancellationToken))
                continue;

            var insight = AIInsight.Create(
                projectId,
                candidate.Type,
                candidate.Severity,
                candidate.Title,
                candidate.Description);

            await insightRepository.AddAsync(insight, cancellationToken);
            saved.Add(insight);
        }

        if (saved.Count > 0)
            await unitOfWork.CompleteAsync(cancellationToken);

        return saved;
    }

    public Task<IReadOnlyList<AIInsight>> GetActiveInsightsAsync(
        long projectId,
        CancellationToken cancellationToken = default)
        => insightRepository.GetActiveByProjectAsync(projectId, cancellationToken);

    public async Task DismissInsightAsync(
        long insightId,
        long userId,
        CancellationToken cancellationToken = default)
    {
        var insight = await insightRepository.GetByIdAsync(insightId, cancellationToken)
            ?? throw new NotFoundException(nameof(AIInsight), insightId);

        insight.Dismiss(userId);
        await unitOfWork.CompleteAsync(cancellationToken);
    }

    /// <summary>
    /// Dan u narednih <see cref="MealPlanHorizonDays"/> je "gap" ako nema nijedne stavke obroka.
    /// WeekStart je uvek ponedeljak UTC, pa datum stavke računamo iz offseta u nedelji.
    /// </summary>
    private static List<DateOnly> ComputeMealPlanGaps(IReadOnlyList<MealPlan> plans, DateOnly today)
    {
        var covered = new HashSet<DateOnly>();
        foreach (var plan in plans)
        {
            var weekStart = DateOnly.FromDateTime(plan.WeekStart);
            foreach (var item in plan.Items)
            {
                var offset = ((int)item.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
                covered.Add(weekStart.AddDays(offset));
            }
        }

        var gaps = new List<DateOnly>();
        for (var i = 0; i < MealPlanHorizonDays; i++)
        {
            var day = today.AddDays(i);
            if (!covered.Contains(day))
                gaps.Add(day);
        }

        return gaps;
    }
}
