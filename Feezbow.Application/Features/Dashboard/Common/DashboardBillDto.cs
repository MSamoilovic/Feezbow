using Feezbow.Domain.Entities;

namespace Feezbow.Application.Features.Dashboard.Common;

public record DashboardBillDto(
    long Id,
    string Title,
    decimal Amount,
    string Currency,
    DateTime DueDate,
    bool IsOverdue,
    string? Category)
{
    public static DashboardBillDto FromEntity(Bill bill, DateTime now) => new(
        bill.Id,
        bill.Title,
        bill.Amount,
        bill.Currency,
        bill.DueDate,
        bill.DueDate < now,
        bill.Category);
}
