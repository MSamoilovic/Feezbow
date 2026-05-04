using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Calendar.DeleteHouseholdEvent;

public record DeleteHouseholdEventCommandResponse(Result<bool> Result);
