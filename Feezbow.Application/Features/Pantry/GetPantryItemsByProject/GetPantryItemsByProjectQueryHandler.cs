using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Pantry.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Pantry.GetPantryItemsByProject;

public class GetPantryItemsByProjectQueryHandler(
    IPantryService pantryService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<GetPantryItemsByProjectQuery, IReadOnlyList<PantryItemDto>>
{
    public async Task<IReadOnlyList<PantryItemDto>> Handle(
        GetPantryItemsByProjectQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var cached = await cacheService.GetOrSetAsync(
            CacheKeys.ProjectPantryList(request.ProjectId, request.Search, request.Location, request.ExpiringWithinDays),
            async () =>
            {
                var items = await pantryService.GetByProjectAsync(
                    request.ProjectId, userId, request.Search, request.Location, request.ExpiringWithinDays, cancellationToken);
                return items.Select(PantryItemDto.FromEntity).ToList();
            },
            CacheExpiration.Short,
            cancellationToken);

        return cached ?? new List<PantryItemDto>();
    }
}
