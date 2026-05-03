namespace Feezbow.Domain.Events.MealPlan_Events;

public class MealPlanDeletedEvent(long mealPlanId, long projectId, long deletedBy) : DomainEvent
{
    public long MealPlanId => mealPlanId;
    public long ProjectId => projectId;
    public long DeletedBy => deletedBy;
}
