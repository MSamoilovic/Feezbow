using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.Pantry.RemovePantryItem;

public record RemovePantryItemCommandResponse(Result<bool> Result);
