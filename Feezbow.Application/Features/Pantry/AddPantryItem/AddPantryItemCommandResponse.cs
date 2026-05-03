using Feezbow.Application.Features.Pantry.Common;
using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Pantry.AddPantryItem;

public record AddPantryItemCommandResponse(Result<PantryItemDto> Result);
