using Feezbow.Domain.Entities;

namespace Feezbow.Domain.Models.Dashboard;

public record HouseholdDashboardData(
    IReadOnlyList<Bill> OverdueBills,
    IReadOnlyList<Bill> UpcomingBills,
    IReadOnlyList<HouseholdChore> ActiveChores,
    MealPlan? ThisWeekMealPlan,
    IReadOnlyList<PantryItem> ExpiringPantryItems,
    IReadOnlyList<Bill> MonthBills,
    IReadOnlyList<HouseholdEvent> UpcomingEvents,
    DateTime AsOf);
