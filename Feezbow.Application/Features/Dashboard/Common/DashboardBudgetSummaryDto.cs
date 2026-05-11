using Feezbow.Domain.Entities;

namespace Feezbow.Application.Features.Dashboard.Common;

public record DashboardBudgetSummaryDto(
    string Currency,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal PendingAmount,
    int TotalBills,
    int PaidBills,
    int OverdueBills)
{
    public static DashboardBudgetSummaryDto FromBills(IReadOnlyList<Bill> bills, DateTime now)
    {
        var currency = bills
            .GroupBy(b => b.Currency)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault()?.Key ?? "EUR";

        var inCurrency = bills.Where(b => b.Currency == currency).ToList();

        return new DashboardBudgetSummaryDto(
            currency,
            inCurrency.Sum(b => b.Amount),
            inCurrency.Where(b => b.IsPaid).Sum(b => b.Amount),
            inCurrency.Where(b => !b.IsPaid).Sum(b => b.Amount),
            bills.Count,
            bills.Count(b => b.IsPaid),
            bills.Count(b => !b.IsPaid && b.DueDate < now));
    }
}
