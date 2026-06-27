using Feezbow.Domain.Models.Dashboard;

namespace Feezbow.Domain.Interfaces.Services;

public interface IDashboardService
{
    Task<HouseholdDashboardData> GetAsync(
        long projectId,
        long userId,
        CancellationToken cancellationToken = default);

    Task<HouseholdDashboardData> GetSystemAsync(
        long projectId,
        CancellationToken cancellationToken = default);
}
