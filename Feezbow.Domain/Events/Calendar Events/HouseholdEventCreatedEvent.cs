namespace Feezbow.Domain.Events.Calendar_Events;

public class HouseholdEventCreatedEvent(long eventId, long projectId, string title, DateTime startDate, long createdBy) : DomainEvent
{
    public long EventId => eventId;
    public long ProjectId => projectId;
    public string Title => title;
    public DateTime StartDate => startDate;
    public long CreatedBy => createdBy;
}
