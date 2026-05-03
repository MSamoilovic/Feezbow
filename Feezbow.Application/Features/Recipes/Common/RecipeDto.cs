using Feezbow.Domain.Entities;

namespace Feezbow.Application.Features.Recipes.Common;

public record RecipeIngredientDto(
    long Id,
    string Name,
    decimal Quantity,
    string? Unit,
    string? Notes,
    int OrderIndex)
{
    public static RecipeIngredientDto FromEntity(RecipeIngredient i) => new(
        i.Id, i.Name, i.Quantity, i.Unit, i.Notes, i.OrderIndex);
}

public record RecipeDto(
    long Id,
    long ProjectId,
    string Name,
    string? Description,
    int Servings,
    int? PrepTimeMinutes,
    int? CookTimeMinutes,
    string? Instructions,
    string? SourceUrl,
    IReadOnlyList<RecipeIngredientDto> Ingredients,
    DateTime CreatedAt,
    DateTime? LastModifiedAt)
{
    public static RecipeDto FromEntity(Recipe r) => new(
        r.Id,
        r.ProjectId,
        r.Name,
        r.Description,
        r.Servings,
        r.PrepTimeMinutes,
        r.CookTimeMinutes,
        r.Instructions,
        r.SourceUrl,
        r.Ingredients
            .OrderBy(i => i.OrderIndex)
            .Select(RecipeIngredientDto.FromEntity)
            .ToList(),
        r.CreatedAt,
        r.LastModifiedAt);
}

/// <summary>Body shape for ingredient upserts — used by both create and replace endpoints.</summary>
public record RecipeIngredientInput(
    string Name,
    decimal Quantity,
    string? Unit = null,
    string? Notes = null);
