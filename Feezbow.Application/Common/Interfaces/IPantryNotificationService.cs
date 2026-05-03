namespace Feezbow.Application.Common.Interfaces;

public interface IPantryNotificationService
{
    Task NotifyAdded(long pantryItemId, long projectId, string name, decimal quantity, string? unit, long addedBy, CancellationToken cancellationToken = default);
    Task NotifyUpdated(long pantryItemId, long projectId, long modifiedBy, CancellationToken cancellationToken = default);
    Task NotifyQuantityChanged(long pantryItemId, long projectId, decimal previousQuantity, decimal newQuantity, decimal delta, long modifiedBy, CancellationToken cancellationToken = default);
    Task NotifyDepleted(long pantryItemId, long projectId, string name, long modifiedBy, CancellationToken cancellationToken = default);
    Task NotifyRemoved(long pantryItemId, long projectId, long removedBy, CancellationToken cancellationToken = default);
}
