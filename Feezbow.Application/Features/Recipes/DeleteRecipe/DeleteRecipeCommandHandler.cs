using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Recipes.DeleteRecipe;

public class DeleteRecipeCommandHandler(
    IRecipeService recipeService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<DeleteRecipeCommand, DeleteRecipeCommandResponse>
{
    public async Task<DeleteRecipeCommandResponse> Handle(
        DeleteRecipeCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var projectId = await recipeService.DeleteAsync(request.RecipeId, userId, cancellationToken);

        await cacheService.RemoveAsync(CacheKeys.Recipe(request.RecipeId), cancellationToken);
        await cacheService.RemoveByPrefixAsync(CacheKeys.ProjectRecipesPrefix(projectId), cancellationToken);

        return new DeleteRecipeCommandResponse(Result<bool>.Success(true, "Recipe deleted."));
    }
}
