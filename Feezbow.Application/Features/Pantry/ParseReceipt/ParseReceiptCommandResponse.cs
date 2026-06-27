namespace Feezbow.Application.Features.Pantry.ParseReceipt;

public record ParseReceiptCommandResponse(IReadOnlyList<ParsedPantryItemDto> Items);

public record ParsedPantryItemDto(
    string Name,
    decimal Quantity,
    string? Unit,
    DateTime? ExpirationDate
);
