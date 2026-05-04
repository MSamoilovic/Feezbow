using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Calendar.CreateHouseholdEvent;

public record CreateHouseholdEventCommandResponse(Result<long> Result);
