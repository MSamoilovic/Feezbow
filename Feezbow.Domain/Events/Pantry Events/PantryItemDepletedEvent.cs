namespace Feezbow.Domain.Events.Pantry_Events;

public class PantryItemDepletedEvent(
    long pantryItemId,
    long projectId,
    string name,
    long modifiedBy) : DomainEvent
{
    public long PantryItemId => pantryItemId;
    public long ProjectId => projectId;
    public string Name => name;
    public long ModifiedBy => modifiedBy;
}
