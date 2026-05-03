using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Recipes.ReplaceRecipeIngredients;

public class ReplaceRecipeIngredientsCommandHandler(
    IRecipeService recipeService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<ReplaceRecipeIngredientsCommand, ReplaceRecipeIngredientsCommandResponse>
{
    public async Task<ReplaceRecipeIngredientsCommandResponse> Handle(
        ReplaceRecipeIngredientsCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var ingredients = request.Ingredients
            .Select(i => (i.Name, i.Quantity, i.Unit, i.Notes))
            .ToList();

        var projectId = await recipeService.ReplaceIngredientsAsync(
            request.RecipeId, userId, ingredients, cancellationToken);

        await cacheService.RemoveAsync(CacheKeys.Recipe(request.RecipeId), cancellationToken);
        await cacheService.RemoveByPrefixAsync(CacheKeys.ProjectRecipesPrefix(projectId), cancellationToken);

        return new ReplaceRecipeIngredientsCommandResponse(Result<bool>.Success(true, "Ingredients replaced."));
    }
}
