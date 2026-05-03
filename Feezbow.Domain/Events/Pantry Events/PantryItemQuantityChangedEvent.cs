namespace Feezbow.Domain.Events.Pantry_Events;

public class PantryItemQuantityChangedEvent(
    long pantryItemId,
    long projectId,
    decimal previousQuantity,
    decimal newQuantity,
    decimal delta,
    long modifiedBy) : DomainEvent
{
    public long PantryItemId => pantryItemId;
    public long ProjectId => projectId;
    public decimal PreviousQuantity => previousQuantity;
    public decimal NewQuantity => newQuantity;
    public decimal Delta => delta;
    public long ModifiedBy => modifiedBy;
}
