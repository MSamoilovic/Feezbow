using Feezbow.Domain.Entities;

namespace Feezbow.Application.Features.Dashboard.Common;

public record DashboardEventDto(
    long Id,
    string Title,
    DateTime StartDate,
    DateTime? EndDate,
    bool IsAllDay,
    string? Category,
    string? Color)
{
    public static DashboardEventDto FromEntity(HouseholdEvent ev) => new(
        ev.Id,
        ev.Title,
        ev.StartDate,
        ev.EndDate,
        ev.IsAllDay,
        ev.Category,
        ev.Color);
}
