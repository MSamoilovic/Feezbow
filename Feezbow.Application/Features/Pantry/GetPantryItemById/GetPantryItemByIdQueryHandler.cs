using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Pantry.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Pantry.GetPantryItemById;

public class GetPantryItemByIdQueryHandler(
    IPantryService pantryService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<GetPantryItemByIdQuery, PantryItemDto>
{
    public async Task<PantryItemDto> Handle(GetPantryItemByIdQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var cached = await cacheService.GetOrSetAsync(
            CacheKeys.PantryItem(request.PantryItemId),
            async () =>
            {
                var item = await pantryService.GetAsync(request.PantryItemId, userId, cancellationToken);
                return PantryItemDto.FromEntity(item);
            },
            CacheExpiration.Short,
            cancellationToken);

        return cached!;
    }
}
