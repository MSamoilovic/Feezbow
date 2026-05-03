using MediatR;

namespace Feezbow.Application.Features.Pantry.AddPantryItem;

public record AddPantryItemCommand(
    long ProjectId,
    string Name,
    decimal Quantity,
    string? Unit = null,
    string? Location = null,
    DateTime? ExpirationDate = null,
    string? Notes = null) : IRequest<AddPantryItemCommandResponse>;
