using Feezbow.Domain.Entities;
using Feezbow.Domain.Enums;

namespace Feezbow.Application.Features.MealPlans.Common;

public record MealPlanItemDto(
    long Id,
    DayOfWeek DayOfWeek,
    MealType MealType,
    string Title,
    string? Notes,
    long? AssignedCookId,
    string? AssignedCookName,
    long? RecipeId,
    string? RecipeName,
    DateTime CreatedAt)
{
    public static MealPlanItemDto FromEntity(MealPlanItem i) => new(
        i.Id,
        i.DayOfWeek,
        i.MealType,
        i.Title,
        i.Notes,
        i.AssignedCookId,
        i.AssignedCook is null ? null : $"{i.AssignedCook.FirstName} {i.AssignedCook.LastName}".Trim(),
        i.RecipeId,
        i.Recipe?.Name,
        i.CreatedAt);
}

public record MealPlanDto(
    long Id,
    long ProjectId,
    DateTime WeekStart,
    string? Notes,
    IReadOnlyList<MealPlanItemDto> Items,
    DateTime CreatedAt)
{
    public static MealPlanDto FromEntity(MealPlan p) => new(
        p.Id,
        p.ProjectId,
        p.WeekStart,
        p.Notes,
        p.Items
            .OrderBy(i => i.DayOfWeek)
            .ThenBy(i => i.MealType)
            .Select(MealPlanItemDto.FromEntity)
            .ToList(),
        p.CreatedAt);
}
