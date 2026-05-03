using Feezbow.Domain.Entities;

namespace Feezbow.Domain.Services;

/// <summary>
/// Pure aggregation of recipe ingredients across the meal plan's slots.
/// Aggregation key is (Name lowercased+trimmed, Unit lowercased+trimmed). Same key sums quantities;
/// different keys (e.g., same ingredient in different units) stay as separate entries.
/// </summary>
public static class MealPlanShoppingListAggregator
{
    public record AggregatedIngredient(string Name, decimal Quantity, string? Unit);

    public static IReadOnlyList<AggregatedIngredient> Aggregate(MealPlan plan)
    {
        var buckets = new Dictionary<(string nameKey, string? unitKey), Bucket>();

        foreach (var slot in plan.Items)
        {
            if (slot.Recipe is null) continue;

            foreach (var ingredient in slot.Recipe.Ingredients)
            {
                var nameKey = ingredient.Name.Trim().ToLowerInvariant();
                var unitKey = string.IsNullOrWhiteSpace(ingredient.Unit) ? null : ingredient.Unit.Trim().ToLowerInvariant();
                var key = (nameKey, unitKey);

                if (buckets.TryGetValue(key, out var existing))
                {
                    existing.Quantity += ingredient.Quantity;
                }
                else
                {
                    buckets[key] = new Bucket
                    {
                        DisplayName = ingredient.Name.Trim(),
                        DisplayUnit = string.IsNullOrWhiteSpace(ingredient.Unit) ? null : ingredient.Unit.Trim(),
                        Quantity = ingredient.Quantity
                    };
                }
            }
        }

        return buckets.Values
            .OrderBy(b => b.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(b => b.DisplayUnit, StringComparer.OrdinalIgnoreCase)
            .Select(b => new AggregatedIngredient(b.DisplayName, b.Quantity, b.DisplayUnit))
            .ToList();
    }

    private sealed class Bucket
    {
        public string DisplayName { get; set; } = null!;
        public string? DisplayUnit { get; set; }
        public decimal Quantity { get; set; }
    }
}
