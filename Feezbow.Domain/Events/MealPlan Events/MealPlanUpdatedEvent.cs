namespace Feezbow.Domain.Events.MealPlan_Events;

public class MealPlanUpdatedEvent(long mealPlanId, long projectId, long modifiedBy) : DomainEvent
{
    public long MealPlanId => mealPlanId;
    public long ProjectId => projectId;
    public long ModifiedBy => modifiedBy;
}
