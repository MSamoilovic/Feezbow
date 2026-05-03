namespace Feezbow.Domain.Events.Pantry_Events;

public class PantryItemUpdatedEvent(
    long pantryItemId,
    long projectId,
    long modifiedBy) : DomainEvent
{
    public long PantryItemId => pantryItemId;
    public long ProjectId => projectId;
    public long ModifiedBy => modifiedBy;
}
