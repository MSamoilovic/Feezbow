using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Recipes.UpdateRecipe;

public class UpdateRecipeCommandHandler(
    IRecipeService recipeService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<UpdateRecipeCommand, UpdateRecipeCommandResponse>
{
    public async Task<UpdateRecipeCommandResponse> Handle(
        UpdateRecipeCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var projectId = await recipeService.UpdateAsync(
            request.RecipeId,
            userId,
            request.Name,
            request.Description,
            request.Servings,
            request.PrepTimeMinutes,
            request.CookTimeMinutes,
            request.Instructions,
            request.SourceUrl,
            cancellationToken);

        await cacheService.RemoveAsync(CacheKeys.Recipe(request.RecipeId), cancellationToken);
        await cacheService.RemoveByPrefixAsync(CacheKeys.ProjectRecipesPrefix(projectId), cancellationToken);

        return new UpdateRecipeCommandResponse(Result<bool>.Success(true, "Recipe updated."));
    }
}
