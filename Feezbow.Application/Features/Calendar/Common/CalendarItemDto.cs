using Feezbow.Domain.Entities;
using Feezbow.Domain.Enums;

namespace Feezbow.Application.Features.Calendar.Common;

public record CalendarItemDto(
    long SourceId,
    string Title,
    string? Description,
    DateTime Date,
    DateTime? EndDate,
    CalendarItemType Type,
    bool IsAllDay,
    string? Status,
    long? AssignedToId,
    string? Color,
    IReadOnlyDictionary<string, string>? Metadata)
{
    public static CalendarItemDto FromBill(Bill bill)
    {
        var now = DateTime.UtcNow;
        var status = bill.IsPaid ? "paid"
            : bill.DueDate < now ? "overdue"
            : "pending";

        var meta = new Dictionary<string, string>
        {
            ["amount"] = bill.Amount.ToString("F2"),
            ["currency"] = bill.Currency
        };
        if (bill.Category is not null)
            meta["category"] = bill.Category;

        return new CalendarItemDto(
            SourceId: bill.Id,
            Title: bill.Title,
            Description: bill.Description,
            Date: bill.DueDate,
            EndDate: null,
            Type: CalendarItemType.Bill,
            IsAllDay: true,
            Status: status,
            AssignedToId: bill.PaidBy,
            Color: null,
            Metadata: meta);
    }

    public static CalendarItemDto FromChore(HouseholdChore chore)
    {
        var now = DateTime.UtcNow;
        var status = chore.IsCompleted ? "completed"
            : chore.DueDate.HasValue && chore.DueDate.Value < now ? "overdue"
            : "pending";

        var meta = new Dictionary<string, string>
        {
            ["priority"] = chore.Priority.ToString()
        };

        return new CalendarItemDto(
            SourceId: chore.Id,
            Title: chore.Title,
            Description: chore.Description,
            Date: chore.DueDate!.Value,
            EndDate: null,
            Type: CalendarItemType.Chore,
            IsAllDay: true,
            Status: status,
            AssignedToId: chore.AssignedToUserId,
            Color: null,
            Metadata: meta);
    }

    public static CalendarItemDto FromMealItem(MealPlan plan, MealPlanItem item)
    {
        // WeekStart is Monday (DayOfWeek=1). Offset = (dow + 6) % 7.
        var itemDate = plan.WeekStart.AddDays(((int)item.DayOfWeek + 6) % 7);

        var meta = new Dictionary<string, string>
        {
            ["mealType"] = item.MealType.ToString(),
            ["dayOfWeek"] = item.DayOfWeek.ToString()
        };
        if (item.RecipeId.HasValue)
            meta["recipeId"] = item.RecipeId.Value.ToString();

        return new CalendarItemDto(
            SourceId: item.Id,
            Title: $"{item.Title} ({item.MealType})",
            Description: item.Notes,
            Date: itemDate,
            EndDate: null,
            Type: CalendarItemType.MealItem,
            IsAllDay: true,
            Status: null,
            AssignedToId: item.AssignedCookId,
            Color: null,
            Metadata: meta);
    }

    public static CalendarItemDto FromHouseholdEvent(HouseholdEvent ev)
    {
        return new CalendarItemDto(
            SourceId: ev.Id,
            Title: ev.Title,
            Description: ev.Description,
            Date: ev.StartDate,
            EndDate: ev.EndDate,
            Type: CalendarItemType.HouseholdEvent,
            IsAllDay: ev.IsAllDay,
            Status: null,
            AssignedToId: ev.AssignedToId,
            Color: ev.Color,
            Metadata: ev.Category is not null
                ? new Dictionary<string, string> { ["category"] = ev.Category }
                : null);
    }
}
