using Feezbow.Application.Features.Recipes.Common;
using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Recipes.CreateRecipe;

public record CreateRecipeCommandResponse(Result<RecipeDto> Result);
