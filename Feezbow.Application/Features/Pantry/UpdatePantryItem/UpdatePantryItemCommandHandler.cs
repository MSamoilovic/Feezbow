using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Pantry.UpdatePantryItem;

public class UpdatePantryItemCommandHandler(
    IPantryService pantryService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<UpdatePantryItemCommand, UpdatePantryItemCommandResponse>
{
    public async Task<UpdatePantryItemCommandResponse> Handle(
        UpdatePantryItemCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var projectId = await pantryService.UpdateAsync(
            request.PantryItemId, userId,
            request.Name, request.Unit, request.Location, request.ExpirationDate, request.Notes,
            cancellationToken);

        await cacheService.RemoveAsync(CacheKeys.PantryItem(request.PantryItemId), cancellationToken);
        await cacheService.RemoveByPrefixAsync(CacheKeys.ProjectPantryPrefix(projectId), cancellationToken);

        return new UpdatePantryItemCommandResponse(Result<bool>.Success(true, "Pantry item updated."));
    }
}
