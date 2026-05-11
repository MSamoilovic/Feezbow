using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Dashboard.Common;
using Feezbow.Domain.Interfaces.Services;
using Feezbow.Domain.Models.Dashboard;
using MediatR;

namespace Feezbow.Application.Features.Dashboard.GetDashboard;

public class GetDashboardQueryHandler(
    IDashboardService dashboardService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<GetDashboardQuery, GetDashboardQueryResponse>
{
    public async Task<GetDashboardQueryResponse> Handle(
        GetDashboardQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        return await cacheService.GetOrSetAsync(
            CacheKeys.ProjectDashboard(request.ProjectId),
            async () => Map(await dashboardService.GetAsync(request.ProjectId, userId, cancellationToken)),
            CacheExpiration.Short,
            cancellationToken) ?? Map(await dashboardService.GetAsync(request.ProjectId, userId, cancellationToken));
    }

    private static GetDashboardQueryResponse Map(HouseholdDashboardData data) => new(
        [.. data.OverdueBills.Select(b => DashboardBillDto.FromEntity(b, data.AsOf))],
        [.. data.UpcomingBills.Select(b => DashboardBillDto.FromEntity(b, data.AsOf))],
        [.. data.ActiveChores.Select(c => DashboardChoreDto.FromEntity(c, data.AsOf))],
        data.ThisWeekMealPlan is null ? null : DashboardMealSummaryDto.FromEntity(data.ThisWeekMealPlan),
        [.. data.ExpiringPantryItems.Select(p => DashboardPantryAlertDto.FromEntity(p, data.AsOf))],
        DashboardBudgetSummaryDto.FromBills(data.MonthBills, data.AsOf),
        [.. data.UpcomingEvents.Select(DashboardEventDto.FromEntity)]);
}
