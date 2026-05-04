using MediatR;

namespace Feezbow.Application.Features.Calendar.DeleteHouseholdEvent;

public record DeleteHouseholdEventCommand(long EventId) : IRequest<DeleteHouseholdEventCommandResponse>;
