using MediatR;

namespace Feezbow.Application.Features.Pantry.UpdatePantryItem;

public record UpdatePantryItemCommand(
    long PantryItemId,
    string? Name = null,
    string? Unit = null,
    string? Location = null,
    DateTime? ExpirationDate = null,
    string? Notes = null) : IRequest<UpdatePantryItemCommandResponse>;
