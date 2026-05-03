using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.MealPlans.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.MealPlans.GetRecentMealPlans;

public class GetRecentMealPlansQueryHandler(
    IMealPlanService mealPlanService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<GetRecentMealPlansQuery, IReadOnlyList<MealPlanDto>>
{
    public async Task<IReadOnlyList<MealPlanDto>> Handle(
        GetRecentMealPlansQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var cached = await cacheService.GetOrSetAsync(
            CacheKeys.ProjectMealPlansRecent(request.ProjectId, request.Count),
            async () =>
            {
                var plans = await mealPlanService.GetRecentAsync(
                    request.ProjectId, userId, request.Count, cancellationToken);
                return plans.Select(MealPlanDto.FromEntity).ToList();
            },
            CacheExpiration.Short,
            cancellationToken);

        return cached ?? new List<MealPlanDto>();
    }
}
