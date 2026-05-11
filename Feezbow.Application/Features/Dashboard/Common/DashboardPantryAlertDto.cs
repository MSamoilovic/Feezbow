using Feezbow.Domain.Entities;

namespace Feezbow.Application.Features.Dashboard.Common;

public record DashboardPantryAlertDto(
    long Id,
    string Name,
    decimal Quantity,
    string? Unit,
    DateTime ExpirationDate,
    int DaysUntilExpiry)
{
    public static DashboardPantryAlertDto FromEntity(PantryItem item, DateTime now) => new(
        item.Id,
        item.Name,
        item.Quantity,
        item.Unit,
        item.ExpirationDate!.Value,
        (int)(item.ExpirationDate.Value.Date - now.Date).TotalDays);
}
