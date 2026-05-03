using MediatR;
using Feezbow.Application.Features.Pantry.Common;

namespace Feezbow.Application.Features.Pantry.GetPantryItemById;

public record GetPantryItemByIdQuery(long PantryItemId) : IRequest<PantryItemDto>;
