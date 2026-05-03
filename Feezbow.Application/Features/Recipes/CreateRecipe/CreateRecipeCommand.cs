using MediatR;
using Feezbow.Application.Features.Recipes.Common;

namespace Feezbow.Application.Features.Recipes.CreateRecipe;

public record CreateRecipeCommand(
    long ProjectId,
    string Name,
    string? Description = null,
    int Servings = 4,
    int? PrepTimeMinutes = null,
    int? CookTimeMinutes = null,
    string? Instructions = null,
    string? SourceUrl = null,
    IReadOnlyList<RecipeIngredientInput>? Ingredients = null) : IRequest<CreateRecipeCommandResponse>;
