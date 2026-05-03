using MediatR;

namespace Feezbow.Application.Features.Pantry.AdjustPantryItemQuantity;

public record AdjustPantryItemQuantityCommand(long PantryItemId, decimal Delta)
    : IRequest<AdjustPantryItemQuantityCommandResponse>;
