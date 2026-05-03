using Feezbow.Domain.Enums;
using Feezbow.Domain.Events.MealPlan_Events;
using Feezbow.Domain.Exceptions;

namespace Feezbow.Domain.Entities;

/// <summary>
/// A weekly meal plan tied to a project. WeekStart is always the Monday (UTC) of the target week.
/// Slots are uniquely identified by (DayOfWeek, MealType).
/// </summary>
public class MealPlan : AuditableEntity
{
    public long ProjectId { get; private set; }
    public Project Project { get; private set; } = null!;

    public DateTime WeekStart { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<MealPlanItem> _items = [];
    public IReadOnlyCollection<MealPlanItem> Items => _items.AsReadOnly();

    private MealPlan() { }

    public static MealPlan Create(long projectId, DateTime weekStart, string? notes, long createdBy)
    {
        if (projectId <= 0)
            throw new BusinessRuleValidationException("ProjectId must be a positive number.");

        if (createdBy <= 0)
            throw new BusinessRuleValidationException("CreatedBy must be a positive number.");

        var normalizedWeekStart = NormalizeWeekStart(weekStart);

        var plan = new MealPlan
        {
            ProjectId = projectId,
            WeekStart = normalizedWeekStart,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        plan.AddDomainEvent(new MealPlanCreatedEvent(plan.Id, projectId, normalizedWeekStart, createdBy));
        return plan;
    }

    public void Update(string? notes, long modifiedBy)
    {
        if (modifiedBy <= 0)
            throw new BusinessRuleValidationException("ModifiedBy must be a positive number.");

        var normalized = string.IsNullOrWhiteSpace(notes) ? null : notes!.Trim();
        if (string.Equals(Notes, normalized, StringComparison.Ordinal)) return;

        Notes = normalized;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;

        AddDomainEvent(new MealPlanUpdatedEvent(Id, ProjectId, modifiedBy));
    }

    /// <summary>
    /// Sets (creates or replaces) the slot for (<paramref name="dayOfWeek"/>, <paramref name="mealType"/>).
    /// Returns the affected item.
    /// </summary>
    public MealPlanItem SetSlot(
        DayOfWeek dayOfWeek,
        MealType mealType,
        string title,
        string? notes,
        long? assignedCookId,
        long? recipeId,
        long modifiedBy)
    {
        if (modifiedBy <= 0)
            throw new BusinessRuleValidationException("ModifiedBy must be a positive number.");

        var existing = _items.FirstOrDefault(i => i.DayOfWeek == dayOfWeek && i.MealType == mealType);
        bool isReplacement;

        if (existing is null)
        {
            existing = MealPlanItem.Create(Id, dayOfWeek, mealType, title, notes, assignedCookId, recipeId, modifiedBy);
            _items.Add(existing);
            isReplacement = false;
        }
        else
        {
            existing.Update(title, notes, assignedCookId, recipeId, modifiedBy);
            isReplacement = true;
        }

        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;

        AddDomainEvent(new MealPlanItemSetEvent(
            Id, ProjectId, existing.Id, dayOfWeek, mealType, existing.Title, existing.AssignedCookId, isReplacement, modifiedBy));

        return existing;
    }

    public void RemoveSlot(DayOfWeek dayOfWeek, MealType mealType, long modifiedBy)
    {
        if (modifiedBy <= 0)
            throw new BusinessRuleValidationException("ModifiedBy must be a positive number.");

        var existing = _items.FirstOrDefault(i => i.DayOfWeek == dayOfWeek && i.MealType == mealType);
        if (existing is null) return;

        _items.Remove(existing);
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;

        AddDomainEvent(new MealPlanItemRemovedEvent(Id, ProjectId, dayOfWeek, mealType, modifiedBy));
    }

    public void MarkForDeletion(long deletedBy)
    {
        if (deletedBy <= 0)
            throw new BusinessRuleValidationException("DeletedBy must be a positive number.");

        AddDomainEvent(new MealPlanDeletedEvent(Id, ProjectId, deletedBy));
    }

    /// <summary>
    /// Snaps a date to the Monday of its ISO week, at 00:00 UTC. Throws if the input is in an unsupported kind.
    /// </summary>
    private static DateTime NormalizeWeekStart(DateTime input)
    {
        var date = input.Date;
        var diff = ((int)date.DayOfWeek - (int)System.DayOfWeek.Monday + 7) % 7;
        var monday = date.AddDays(-diff);
        return DateTime.SpecifyKind(monday, DateTimeKind.Utc);
    }
}
