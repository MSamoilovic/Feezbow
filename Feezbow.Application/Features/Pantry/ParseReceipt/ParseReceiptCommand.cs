using MediatR;

namespace Feezbow.Application.Features.Pantry.ParseReceipt;

public record ParseReceiptCommand(long ProjectId, byte[] ImageData, string MediaType)
    : IRequest<ParseReceiptCommandResponse>;
