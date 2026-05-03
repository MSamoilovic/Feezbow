using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Recipes.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Recipes.GetRecipesByProject;

public class GetRecipesByProjectQueryHandler(
    IRecipeService recipeService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<GetRecipesByProjectQuery, IReadOnlyList<RecipeDto>>
{
    public async Task<IReadOnlyList<RecipeDto>> Handle(
        GetRecipesByProjectQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var cached = await cacheService.GetOrSetAsync(
            CacheKeys.ProjectRecipesList(request.ProjectId, request.Skip, request.Take),
            async () =>
            {
                var recipes = await recipeService.GetByProjectAsync(
                    request.ProjectId, userId, request.Skip, request.Take, cancellationToken);
                return recipes.Select(RecipeDto.FromEntity).ToList();
            },
            CacheExpiration.Medium,
            cancellationToken);

        return cached ?? new List<RecipeDto>();
    }
}
