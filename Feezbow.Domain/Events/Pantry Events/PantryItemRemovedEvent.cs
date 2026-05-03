namespace Feezbow.Domain.Events.Pantry_Events;

public class PantryItemRemovedEvent(
    long pantryItemId,
    long projectId,
    long removedBy) : DomainEvent
{
    public long PantryItemId => pantryItemId;
    public long ProjectId => projectId;
    public long RemovedBy => removedBy;
}
