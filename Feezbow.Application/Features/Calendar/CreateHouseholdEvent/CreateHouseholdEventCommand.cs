using MediatR;

namespace Feezbow.Application.Features.Calendar.CreateHouseholdEvent;

public record CreateHouseholdEventCommand(
    long ProjectId,
    string Title,
    DateTime StartDate,
    string? Description = null,
    DateTime? EndDate = null,
    bool IsAllDay = false,
    string? Category = null,
    string? Color = null,
    long? AssignedToId = null) : IRequest<CreateHouseholdEventCommandResponse>;
