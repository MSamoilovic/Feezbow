namespace Feezbow.Application.Features.Bills.ParseBill;

public record ParsedBillDto(
    string Title,
    decimal Amount,
    string Currency,
    DateTime? DueDate,
    string? Category,
    string? Description,
    double Confidence);

public record ParseBillCommandResponse(ParsedBillDto Bill);
