using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.MealPlans.RemoveMealPlanSlot;

public class RemoveMealPlanSlotCommandHandler(
    IMealPlanService mealPlanService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<RemoveMealPlanSlotCommand, RemoveMealPlanSlotCommandResponse>
{
    public async Task<RemoveMealPlanSlotCommandResponse> Handle(
        RemoveMealPlanSlotCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var projectId = await mealPlanService.RemoveSlotAsync(
            request.MealPlanId,
            userId,
            request.DayOfWeek,
            request.MealType,
            cancellationToken);

        await cacheService.RemoveByPrefixAsync(
            CacheKeys.ProjectMealPlansPrefix(projectId), cancellationToken);

        return new RemoveMealPlanSlotCommandResponse(Result<bool>.Success(true, "Meal slot removed."));
    }
}
