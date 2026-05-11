using Feezbow.Domain.Entities;

namespace Feezbow.Application.Features.Dashboard.Common;

public record DashboardMealSummaryDto(
    DateTime WeekStart,
    string? Notes,
    IReadOnlyList<DashboardMealItemDto> Items)
{
    public static DashboardMealSummaryDto FromEntity(MealPlan plan) => new(
        plan.WeekStart,
        plan.Notes,
        plan.Items
            .OrderBy(i => ((int)i.DayOfWeek + 6) % 7)
            .ThenBy(i => i.MealType)
            .Select(DashboardMealItemDto.FromEntity)
            .ToList());
}

public record DashboardMealItemDto(
    long Id,
    DayOfWeek DayOfWeek,
    string MealType,
    string Title,
    long? RecipeId,
    long? AssignedCookId)
{
    public static DashboardMealItemDto FromEntity(MealPlanItem item) => new(
        item.Id,
        item.DayOfWeek,
        item.MealType.ToString(),
        item.Title,
        item.RecipeId,
        item.AssignedCookId);
}
