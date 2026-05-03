using MediatR;
using Feezbow.Application.Features.Recipes.Common;

namespace Feezbow.Application.Features.Recipes.ReplaceRecipeIngredients;

public record ReplaceRecipeIngredientsCommand(
    long RecipeId,
    IReadOnlyList<RecipeIngredientInput> Ingredients) : IRequest<ReplaceRecipeIngredientsCommandResponse>;
