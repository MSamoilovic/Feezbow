using MediatR;

namespace Feezbow.Application.Features.Pantry.RemovePantryItem;

public record RemovePantryItemCommand(long PantryItemId)
    : IRequest<RemovePantryItemCommandResponse>;
