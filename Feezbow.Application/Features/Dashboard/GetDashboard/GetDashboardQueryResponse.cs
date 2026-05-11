using Feezbow.Application.Features.Dashboard.Common;

namespace Feezbow.Application.Features.Dashboard.GetDashboard;

public record GetDashboardQueryResponse(
    IReadOnlyList<DashboardBillDto> OverdueBills,
    IReadOnlyList<DashboardBillDto> UpcomingBills,
    IReadOnlyList<DashboardChoreDto> ActiveChores,
    DashboardMealSummaryDto? ThisWeekMealPlan,
    IReadOnlyList<DashboardPantryAlertDto> ExpiringPantryItems,
    DashboardBudgetSummaryDto BudgetSummary,
    IReadOnlyList<DashboardEventDto> UpcomingEvents);
