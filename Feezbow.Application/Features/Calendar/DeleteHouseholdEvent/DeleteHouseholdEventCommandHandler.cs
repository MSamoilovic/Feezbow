using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;
using MediatR;

namespace Feezbow.Application.Features.Calendar.DeleteHouseholdEvent;

public class DeleteHouseholdEventCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<DeleteHouseholdEventCommand, DeleteHouseholdEventCommandResponse>
{
    public async Task<DeleteHouseholdEventCommandResponse> Handle(
        DeleteHouseholdEventCommand request, CancellationToken cancellationToken)
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

        var projectId = ev.ProjectId;
        ev.Delete(userId);

        unitOfWork.HouseholdEvents.Remove(ev);
        await unitOfWork.CompleteAsync(cancellationToken);

        await Task.WhenAll(
            cacheService.RemoveByPrefixAsync(CacheKeys.ProjectCalendarPrefix(projectId), cancellationToken),
            cacheService.RemoveAsync(CacheKeys.HouseholdEvent(request.EventId), cancellationToken),
            cacheService.RemoveAsync(CacheKeys.ProjectHouseholdEvents(projectId), cancellationToken));

        return new DeleteHouseholdEventCommandResponse(
            Result<bool>.Success(true, "Event deleted successfully."));
    }
}
