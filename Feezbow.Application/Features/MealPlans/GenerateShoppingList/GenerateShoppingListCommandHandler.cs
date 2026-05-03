using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.MealPlans.GenerateShoppingList;

public class GenerateShoppingListCommandHandler(
    IMealPlanService mealPlanService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<GenerateShoppingListCommand, GenerateShoppingListCommandResponse>
{
    public async Task<GenerateShoppingListCommandResponse> Handle(
        GenerateShoppingListCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var list = await mealPlanService.GenerateShoppingListAsync(
            request.MealPlanId, userId, request.Name, cancellationToken);

        await cacheService.RemoveAsync(CacheKeys.ProjectShoppingLists(list.ProjectId), cancellationToken);

        var summary = new GeneratedShoppingListSummary(
            list.Id, list.Name, list.ProjectId, list.Items.Count);

        return new GenerateShoppingListCommandResponse(
            Result<GeneratedShoppingListSummary>.Success(summary, "Shopping list generated."));
    }
}
