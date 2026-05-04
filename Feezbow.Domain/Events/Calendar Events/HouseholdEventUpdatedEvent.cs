namespace Feezbow.Domain.Events.Calendar_Events;

public class HouseholdEventUpdatedEvent(long eventId, long projectId, long updatedBy) : DomainEvent
{
    public long EventId => eventId;
    public long ProjectId => projectId;
    public long UpdatedBy => updatedBy;
}
