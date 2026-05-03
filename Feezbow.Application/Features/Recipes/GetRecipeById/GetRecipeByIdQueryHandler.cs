using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Recipes.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Recipes.GetRecipeById;

public class GetRecipeByIdQueryHandler(
    IRecipeService recipeService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<GetRecipeByIdQuery, RecipeDto>
{
    public async Task<RecipeDto> Handle(GetRecipeByIdQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var cached = await cacheService.GetOrSetAsync(
            CacheKeys.Recipe(request.RecipeId),
            async () =>
            {
                var recipe = await recipeService.GetAsync(request.RecipeId, userId, cancellationToken);
                return RecipeDto.FromEntity(recipe);
            },
            CacheExpiration.Medium,
            cancellationToken);

        return cached!;
    }
}
