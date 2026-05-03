using Feezbow.Domain.Enums;

namespace Feezbow.Domain.Events.MealPlan_Events;

public class MealPlanItemSetEvent(
    long mealPlanId,
    long projectId,
    long itemId,
    DayOfWeek dayOfWeek,
    MealType mealType,
    string title,
    long? assignedCookId,
    bool isReplacement,
    long modifiedBy) : DomainEvent
{
    public long MealPlanId => mealPlanId;
    public long ProjectId => projectId;
    public long ItemId => itemId;
    public DayOfWeek DayOfWeek => dayOfWeek;
    public MealType MealType => mealType;
    public string Title => title;
    public long? AssignedCookId => assignedCookId;
    public bool IsReplacement => isReplacement;
    public long ModifiedBy => modifiedBy;
}
