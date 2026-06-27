using MediatR;

namespace Feezbow.Application.Features.Bills.ParseBill;

public record ParseBillCommand(
    long ProjectId,
    byte[] FileData,
    string MediaType) : IRequest<ParseBillCommandResponse>;
