using Feezbow.Domain.Entities;

namespace Feezbow.Domain.Interfaces.Repositories;

public interface IPantryRepository
{
    Task<PantryItem?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists pantry items for a project with optional filters. Pass <paramref name="expiringWithinDays"/>
    /// to limit to items whose ExpirationDate falls within the next N days (inclusive).
    /// </summary>
    Task<IReadOnlyList<PantryItem>> GetByProjectAsync(
        long projectId,
        string? search,
        string? location,
        int? expiringWithinDays,
        CancellationToken cancellationToken = default);

    Task AddAsync(PantryItem item, CancellationToken cancellationToken = default);
    void Remove(PantryItem item);
}
