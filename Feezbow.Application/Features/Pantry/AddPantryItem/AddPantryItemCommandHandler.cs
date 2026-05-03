using MediatR;
using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Application.Features.Pantry.Common;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Application.Features.Pantry.AddPantryItem;

public class AddPantryItemCommandHandler(
    IPantryService pantryService,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<AddPantryItemCommand, AddPantryItemCommandResponse>
{
    public async Task<AddPantryItemCommandResponse> Handle(
        AddPantryItemCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var item = await pantryService.AddAsync(
            request.ProjectId, userId,
            request.Name, request.Quantity, request.Unit,
            request.Location, request.ExpirationDate, request.Notes,
            cancellationToken);

        await cacheService.RemoveByPrefixAsync(
            CacheKeys.ProjectPantryPrefix(request.ProjectId), cancellationToken);

        return new AddPantryItemCommandResponse(
            Result<PantryItemDto>.Success(PantryItemDto.FromEntity(item), "Pantry item added."));
    }
}
