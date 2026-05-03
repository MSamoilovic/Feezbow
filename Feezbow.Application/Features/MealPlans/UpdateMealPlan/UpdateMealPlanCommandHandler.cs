using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.MealPlans.UpdateMealPlan;

public class UpdateMealPlanCommandHandler(
    IMealPlanService mealPlanService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<UpdateMealPlanCommand, UpdateMealPlanCommandResponse>
{
    public async Task<UpdateMealPlanCommandResponse> Handle(
        UpdateMealPlanCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var projectId = await mealPlanService.UpdateAsync(
            request.MealPlanId, userId, request.Notes, cancellationToken);

        await cacheService.RemoveByPrefixAsync(
            CacheKeys.ProjectMealPlansPrefix(projectId), cancellationToken);

        return new UpdateMealPlanCommandResponse(Result<bool>.Success(true, "Meal plan updated."));
    }
}
