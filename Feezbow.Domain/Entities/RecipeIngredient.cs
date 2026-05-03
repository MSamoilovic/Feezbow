using Feezbow.Domain.Exceptions;

namespace Feezbow.Domain.Entities;

public class RecipeIngredient : AuditableEntity
{
    public long RecipeId { get; private set; }

    public string Name { get; private set; } = null!;
    public decimal Quantity { get; private set; }
    public string? Unit { get; private set; }
    public string? Notes { get; private set; }

    /// <summary>Display order as supplied by the user. Stable sort key for the ingredient list.</summary>
    public int OrderIndex { get; private set; }

    private RecipeIngredient() { }

    internal static RecipeIngredient Create(
        long recipeId,
        string name,
        decimal quantity,
        string? unit,
        string? notes,
        int orderIndex,
        long createdBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleValidationException("Ingredient name cannot be empty.");

        if (quantity < 0)
            throw new BusinessRuleValidationException("Ingredient quantity cannot be negative.");

        return new RecipeIngredient
        {
            RecipeId = recipeId,
            Name = name.Trim(),
            Quantity = quantity,
            Unit = string.IsNullOrWhiteSpace(unit) ? null : unit.Trim(),
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            OrderIndex = orderIndex,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }
}
