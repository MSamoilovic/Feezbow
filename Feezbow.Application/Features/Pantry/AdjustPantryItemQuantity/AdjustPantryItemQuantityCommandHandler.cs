using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Pantry.AdjustPantryItemQuantity;

public class AdjustPantryItemQuantityCommandHandler(
    IPantryService pantryService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<AdjustPantryItemQuantityCommand, AdjustPantryItemQuantityCommandResponse>
{
    public async Task<AdjustPantryItemQuantityCommandResponse> Handle(
        AdjustPantryItemQuantityCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var projectId = await pantryService.AdjustQuantityAsync(
            request.PantryItemId, userId, request.Delta, cancellationToken);

        await cacheService.RemoveAsync(CacheKeys.PantryItem(request.PantryItemId), cancellationToken);
        await cacheService.RemoveByPrefixAsync(CacheKeys.ProjectPantryPrefix(projectId), cancellationToken);

        return new AdjustPantryItemQuantityCommandResponse(
            Result<bool>.Success(true, "Pantry item quantity adjusted."));
    }
}
