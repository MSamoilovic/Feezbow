namespace Feezbow.Domain.Events.Pantry_Events;

public class PantryItemAddedEvent(
    long pantryItemId,
    long projectId,
    string name,
    decimal quantity,
    string? unit,
    long addedBy) : DomainEvent
{
    public long PantryItemId => pantryItemId;
    public long ProjectId => projectId;
    public string Name => name;
    public decimal Quantity => quantity;
    public string? Unit => unit;
    public long AddedBy => addedBy;
}
