using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.MealPlans.DeleteMealPlan;

public class DeleteMealPlanCommandHandler(
    IMealPlanService mealPlanService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<DeleteMealPlanCommand, DeleteMealPlanCommandResponse>
{
    public async Task<DeleteMealPlanCommandResponse> Handle(
        DeleteMealPlanCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var projectId = await mealPlanService.DeleteAsync(request.MealPlanId, userId, cancellationToken);

        await cacheService.RemoveByPrefixAsync(
            CacheKeys.ProjectMealPlansPrefix(projectId), cancellationToken);

        return new DeleteMealPlanCommandResponse(Result<bool>.Success(true, "Meal plan deleted."));
    }
}
