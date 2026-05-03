using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Pantry.AdjustPantryItemQuantity;

public record AdjustPantryItemQuantityCommandResponse(Result<bool> Result);
