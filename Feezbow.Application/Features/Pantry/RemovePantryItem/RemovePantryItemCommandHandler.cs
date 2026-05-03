using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Pantry.RemovePantryItem;

public class RemovePantryItemCommandHandler(
    IPantryService pantryService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<RemovePantryItemCommand, RemovePantryItemCommandResponse>
{
    public async Task<RemovePantryItemCommandResponse> Handle(
        RemovePantryItemCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var projectId = await pantryService.RemoveAsync(request.PantryItemId, userId, cancellationToken);

        await cacheService.RemoveAsync(CacheKeys.PantryItem(request.PantryItemId), cancellationToken);
        await cacheService.RemoveByPrefixAsync(CacheKeys.ProjectPantryPrefix(projectId), cancellationToken);

        return new RemovePantryItemCommandResponse(Result<bool>.Success(true, "Pantry item removed."));
    }
}
