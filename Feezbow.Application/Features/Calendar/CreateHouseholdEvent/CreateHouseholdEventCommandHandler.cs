using Feezbow.Application.Common.Caching;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Entities;
using Feezbow.Domain.Entities.Common;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;
using MediatR;

namespace Feezbow.Application.Features.Calendar.CreateHouseholdEvent;

public class CreateHouseholdEventCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    ICacheService cacheService)
    : IRequestHandler<CreateHouseholdEventCommand, CreateHouseholdEventCommandResponse>
{
    public async Task<CreateHouseholdEventCommandResponse> Handle(
        CreateHouseholdEventCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var userId = currentUserService.UserId ?? 0;

        var project = await unitOfWork.Projects.GetProjectWithMembersAsync(request.ProjectId, cancellationToken)
            ?? throw new NotFoundException("Project", request.ProjectId);

        if (!project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        var ev = HouseholdEvent.Create(
            request.ProjectId,
            request.Title,
            request.StartDate,
            userId,
            request.Description,
            request.EndDate,
            request.IsAllDay,
            request.Category,
            request.Color,
            request.AssignedToId);

        await unitOfWork.HouseholdEvents.AddAsync(ev, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        await Task.WhenAll(
            cacheService.RemoveByPrefixAsync(CacheKeys.ProjectCalendarPrefix(request.ProjectId), cancellationToken),
            cacheService.RemoveAsync(CacheKeys.ProjectHouseholdEvents(request.ProjectId), cancellationToken));

        return new CreateHouseholdEventCommandResponse(
            Result<long>.Success(ev.Id, "Event created successfully."));
    }
}
