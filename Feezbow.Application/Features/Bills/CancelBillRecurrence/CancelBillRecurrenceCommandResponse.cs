using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Bills.CancelBillRecurrence;

public record CancelBillRecurrenceCommandResponse(Result<long> Result);
