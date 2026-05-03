using MediatR;

namespace Feezbow.Application.Features.Recipes.UpdateRecipe;

public record UpdateRecipeCommand(
    long RecipeId,
    string? Name = null,
    string? Description = null,
    int? Servings = null,
    int? PrepTimeMinutes = null,
    int? CookTimeMinutes = null,
    string? Instructions = null,
    string? SourceUrl = null) : IRequest<UpdateRecipeCommandResponse>;
