using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.MealPlans.Common;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.MealPlans.CreateMealPlan;

public class CreateMealPlanCommandHandler(
    IMealPlanService mealPlanService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<CreateMealPlanCommand, CreateMealPlanCommandResponse>
{
    public async Task<CreateMealPlanCommandResponse> Handle(
        CreateMealPlanCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var plan = await mealPlanService.CreateAsync(
            request.ProjectId, userId, request.WeekStart, request.Notes, cancellationToken);

        await cacheService.RemoveByPrefixAsync(
            CacheKeys.ProjectMealPlansPrefix(request.ProjectId), cancellationToken);

        return new CreateMealPlanCommandResponse(
            Result<MealPlanDto>.Success(MealPlanDto.FromEntity(plan), "Meal plan created."));
    }
}
