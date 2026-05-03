namespace Feezbow.Domain.Events.MealPlan_Events;

public class MealPlanCreatedEvent(long mealPlanId, long projectId, DateTime weekStart, long createdBy) : DomainEvent
{
    public long MealPlanId => mealPlanId;
    public long ProjectId => projectId;
    public DateTime WeekStart => weekStart;
    public long CreatedBy => createdBy;
}
