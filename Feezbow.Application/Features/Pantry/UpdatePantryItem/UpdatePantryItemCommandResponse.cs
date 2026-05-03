using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Pantry.UpdatePantryItem;

public record UpdatePantryItemCommandResponse(Result<bool> Result);
