using Feezbow.Domain.Enums;

namespace Feezbow.Domain.Events.MealPlan_Events;

public class MealPlanItemRemovedEvent(
    long mealPlanId,
    long projectId,
    DayOfWeek dayOfWeek,
    MealType mealType,
    long removedBy) : DomainEvent
{
    public long MealPlanId => mealPlanId;
    public long ProjectId => projectId;
    public DayOfWeek DayOfWeek => dayOfWeek;
    public MealType MealType => mealType;
    public long RemovedBy => removedBy;
}
