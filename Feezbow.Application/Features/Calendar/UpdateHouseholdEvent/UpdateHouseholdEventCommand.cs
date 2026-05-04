using MediatR;

namespace Feezbow.Application.Features.Calendar.UpdateHouseholdEvent;

public record UpdateHouseholdEventCommand(
    long EventId,
    string? Title = null,
    string? Description = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    bool? IsAllDay = null,
    string? Category = null,
    string? Color = null,
    long? AssignedToId = null) : IRequest<UpdateHouseholdEventCommandResponse>;
