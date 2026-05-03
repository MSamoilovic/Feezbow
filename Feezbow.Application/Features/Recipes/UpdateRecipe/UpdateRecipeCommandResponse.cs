using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Recipes.UpdateRecipe;

public record UpdateRecipeCommandResponse(Result<bool> Result);
