using Feezbow.Domain.Entities;

namespace Feezbow.Domain.Interfaces.Services;

public interface IPantryService
{
    Task<PantryItem> AddAsync(
        long projectId,
        long userId,
        string name,
        decimal quantity,
        string? unit,
        string? location,
        DateTime? expirationDate,
        string? notes,
        CancellationToken cancellationToken = default);

    Task<PantryItem> GetAsync(
        long pantryItemId,
        long userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PantryItem>> GetByProjectAsync(
        long projectId,
        long userId,
        string? search,
        string? location,
        int? expiringWithinDays,
        CancellationToken cancellationToken = default);

    Task<long> UpdateAsync(
        long pantryItemId,
        long userId,
        string? name,
        string? unit,
        string? location,
        DateTime? expirationDate,
        string? notes,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adjusts quantity by <paramref name="delta"/> (negative consumes, positive restocks).
    /// Returns the project id for cache invalidation.
    /// </summary>
    Task<long> AdjustQuantityAsync(
        long pantryItemId,
        long userId,
        decimal delta,
        CancellationToken cancellationToken = default);

    Task<long> RemoveAsync(
        long pantryItemId,
        long userId,
        CancellationToken cancellationToken = default);
}
