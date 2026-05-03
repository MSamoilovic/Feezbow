using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.MealPlans.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.MealPlans.GetMealPlanByWeek;

public class GetMealPlanByWeekQueryHandler(
    IMealPlanService mealPlanService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<GetMealPlanByWeekQuery, MealPlanDto?>
{
    public async Task<MealPlanDto?> Handle(
        GetMealPlanByWeekQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;
        var weekStart = SnapToMonday(request.WeekStart ?? DateTime.UtcNow);

        return await cacheService.GetOrSetAsync(
            CacheKeys.ProjectMealPlanByWeek(request.ProjectId, weekStart),
            async () =>
            {
                var plan = await mealPlanService.GetByWeekAsync(
                    request.ProjectId, userId, weekStart, cancellationToken);
                return plan is null ? null : MealPlanDto.FromEntity(plan);
            },
            CacheExpiration.Short,
            cancellationToken);
    }

    private static DateTime SnapToMonday(DateTime input)
    {
        var date = input.Date;
        var diff = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        var monday = date.AddDays(-diff);
        return DateTime.SpecifyKind(monday, DateTimeKind.Utc);
    }
}
