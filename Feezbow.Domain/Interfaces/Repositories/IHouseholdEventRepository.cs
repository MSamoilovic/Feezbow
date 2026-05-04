using Feezbow.Domain.Entities;

namespace Feezbow.Domain.Interfaces.Repositories;

public interface IHouseholdEventRepository
{
    Task<HouseholdEvent?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<HouseholdEvent>> GetByProjectAndDateRangeAsync(long projectId, DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task AddAsync(HouseholdEvent householdEvent, CancellationToken cancellationToken = default);
    void Remove(HouseholdEvent householdEvent);
}
