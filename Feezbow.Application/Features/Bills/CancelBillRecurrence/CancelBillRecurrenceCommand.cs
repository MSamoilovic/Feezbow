using MediatR;

namespace Feezbow.Application.Features.Bills.CancelBillRecurrence;

public record CancelBillRecurrenceCommand(long BillId) : IRequest<CancelBillRecurrenceCommandResponse>;

