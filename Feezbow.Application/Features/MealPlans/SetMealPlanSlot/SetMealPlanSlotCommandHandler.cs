using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.MealPlans.Common;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.MealPlans.SetMealPlanSlot;

public class SetMealPlanSlotCommandHandler(
    IMealPlanService mealPlanService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<SetMealPlanSlotCommand, SetMealPlanSlotCommandResponse>
{
    public async Task<SetMealPlanSlotCommandResponse> Handle(
        SetMealPlanSlotCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var (item, projectId) = await mealPlanService.SetSlotAsync(
            request.MealPlanId,
            userId,
            request.DayOfWeek,
            request.MealType,
            request.Title,
            request.Notes,
            request.AssignedCookId,
            request.RecipeId,
            cancellationToken);

        await cacheService.RemoveByPrefixAsync(
            CacheKeys.ProjectMealPlansPrefix(projectId), cancellationToken);

        return new SetMealPlanSlotCommandResponse(
            Result<MealPlanItemDto>.Success(MealPlanItemDto.FromEntity(item), "Meal slot saved."));
    }
}
