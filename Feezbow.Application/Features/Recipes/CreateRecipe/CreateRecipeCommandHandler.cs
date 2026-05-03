using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Recipes.Common;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Recipes.CreateRecipe;

public class CreateRecipeCommandHandler(
    IRecipeService recipeService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<CreateRecipeCommand, CreateRecipeCommandResponse>
{
    public async Task<CreateRecipeCommandResponse> Handle(
        CreateRecipeCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var ingredients = (request.Ingredients ?? [])
            .Select(i => (i.Name, i.Quantity, i.Unit, i.Notes))
            .ToList();

        var recipe = await recipeService.CreateAsync(
            request.ProjectId,
            userId,
            request.Name,
            request.Description,
            request.Servings,
            request.PrepTimeMinutes,
            request.CookTimeMinutes,
            request.Instructions,
            request.SourceUrl,
            ingredients,
            cancellationToken);

        await cacheService.RemoveByPrefixAsync(
            CacheKeys.ProjectRecipesPrefix(request.ProjectId), cancellationToken);

        return new CreateRecipeCommandResponse(
            Result<RecipeDto>.Success(RecipeDto.FromEntity(recipe), "Recipe created."));
    }
}
