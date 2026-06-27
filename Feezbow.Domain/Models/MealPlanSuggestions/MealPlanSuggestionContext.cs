using Feezbow.Domain.Enums;

namespace Feezbow.Domain.Models.MealPlanSuggestions;

public class MealPlanSuggestionContext
{
    public long MealPlanId { get; init; }
    public DateTime WeekStart { get; init; }
    public IReadOnlyList<EmptySlot> EmptySlots { get; init; } = [];
    public IReadOnlyList<PantryEntry> PantryItems { get; init; } = [];
    public IReadOnlyList<RecipeEntry> Recipes { get; init; } = [];
}

public record EmptySlot(DayOfWeek DayOfWeek, MealType MealType);

public record PantryEntry(string Name, decimal Quantity, string? Unit);

public record RecipeEntry(long Id, string Name, IReadOnlyList<string> Ingredients);
