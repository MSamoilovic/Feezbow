namespace Feezbow.Domain.Events.Calendar_Events;

public class HouseholdEventDeletedEvent(long eventId, long projectId, long deletedBy) : DomainEvent
{
    public long EventId => eventId;
    public long ProjectId => projectId;
    public long DeletedBy => deletedBy;
}
