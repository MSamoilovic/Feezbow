using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Recipes.ReplaceRecipeIngredients;

public record ReplaceRecipeIngredientsCommandResponse(Result<bool> Result);
