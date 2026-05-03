using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Recipes.DeleteRecipe;

public record DeleteRecipeCommandResponse(Result<bool> Result);
