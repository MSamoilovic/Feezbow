using MediatR;

namespace Feezbow.Application.Features.Recipes.DeleteRecipe;

public record DeleteRecipeCommand(long RecipeId) : IRequest<DeleteRecipeCommandResponse>;
