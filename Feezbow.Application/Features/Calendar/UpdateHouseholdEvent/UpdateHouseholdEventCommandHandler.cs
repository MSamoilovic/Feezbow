using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;
using MediatR;

namespace Feezbow.Application.Features.Calendar.UpdateHouseholdEvent;

public class UpdateHouseholdEventCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<UpdateHouseholdEventCommand, UpdateHouseholdEventCommandResponse>
{
    public async Task<UpdateHouseholdEventCommandResponse> Handle(
        UpdateHouseholdEventCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var ev = await unitOfWork.HouseholdEvents.GetByIdAsync(request.EventId, cancellationToken)
            ?? throw new NotFoundException("HouseholdEvent", request.EventId);

        var project = await unitOfWork.Projects.GetProjectWithMembersAsync(ev.ProjectId, cancellationToken)
            ?? throw new NotFoundException("Project", ev.ProjectId);

        if (!project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        ev.Update(
            request.Title,
            request.Description,
            request.StartDate,
            request.EndDate,
            request.IsAllDay,
            request.Category,
            request.Color,
            request.AssignedToId,
            userId);

        await unitOfWork.CompleteAsync(cancellationToken);

        await Task.WhenAll(
            cacheService.RemoveByPrefixAsync(CacheKeys.ProjectCalendarPrefix(ev.ProjectId), cancellationToken),
            cacheService.RemoveAsync(CacheKeys.HouseholdEvent(request.EventId), cancellationToken),
            cacheService.RemoveAsync(CacheKeys.ProjectHouseholdEvents(ev.ProjectId), cancellationToken));

        return new UpdateHouseholdEventCommandResponse(
            Result<long>.Success(ev.Id, "Event updated successfully."));
    }
}
