using Feezbow.Domain.Exceptions;

namespace Feezbow.Domain.Entities;

public class Recipe : AuditableEntity
{
    public long ProjectId { get; private set; }
    public Project Project { get; private set; } = null!;

    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public int Servings { get; private set; } = 4;
    public int? PrepTimeMinutes { get; private set; }
    public int? CookTimeMinutes { get; private set; }

    /// <summary>Free-form steps, typically newline-separated.</summary>
    public string? Instructions { get; private set; }

    public string? SourceUrl { get; private set; }

    private readonly List<RecipeIngredient> _ingredients = [];
    public IReadOnlyCollection<RecipeIngredient> Ingredients => _ingredients.AsReadOnly();

    private Recipe() { }

    public static Recipe Create(
        long projectId,
        string name,
        string? description,
        int servings,
        int? prepTimeMinutes,
        int? cookTimeMinutes,
        string? instructions,
        string? sourceUrl,
        long createdBy)
    {
        if (projectId <= 0)
            throw new BusinessRuleValidationException("ProjectId must be a positive number.");

        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleValidationException("Recipe name cannot be empty.");

        if (servings <= 0)
            throw new BusinessRuleValidationException("Servings must be greater than zero.");

        if (prepTimeMinutes is < 0)
            throw new BusinessRuleValidationException("Prep time cannot be negative.");

        if (cookTimeMinutes is < 0)
            throw new BusinessRuleValidationException("Cook time cannot be negative.");

        if (createdBy <= 0)
            throw new BusinessRuleValidationException("CreatedBy must be a positive number.");

        return new Recipe
        {
            ProjectId = projectId,
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            Servings = servings,
            PrepTimeMinutes = prepTimeMinutes,
            CookTimeMinutes = cookTimeMinutes,
            Instructions = string.IsNullOrWhiteSpace(instructions) ? null : instructions.Trim(),
            SourceUrl = string.IsNullOrWhiteSpace(sourceUrl) ? null : sourceUrl.Trim(),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public void Update(
        string? name,
        string? description,
        int? servings,
        int? prepTimeMinutes,
        int? cookTimeMinutes,
        string? instructions,
        string? sourceUrl,
        long modifiedBy)
    {
        if (modifiedBy <= 0)
            throw new BusinessRuleValidationException("ModifiedBy must be a positive number.");

        var changed = false;

        if (!string.IsNullOrWhiteSpace(name) && !string.Equals(Name, name.Trim(), StringComparison.Ordinal))
        {
            Name = name.Trim();
            changed = true;
        }

        if (description is not null)
        {
            var normalized = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            if (!string.Equals(Description, normalized, StringComparison.Ordinal))
            {
                Description = normalized;
                changed = true;
            }
        }

        if (servings.HasValue)
        {
            if (servings.Value <= 0)
                throw new BusinessRuleValidationException("Servings must be greater than zero.");
            if (Servings != servings.Value)
            {
                Servings = servings.Value;
                changed = true;
            }
        }

        if (prepTimeMinutes is < 0)
            throw new BusinessRuleValidationException("Prep time cannot be negative.");
        if (PrepTimeMinutes != prepTimeMinutes)
        {
            PrepTimeMinutes = prepTimeMinutes;
            changed = true;
        }

        if (cookTimeMinutes is < 0)
            throw new BusinessRuleValidationException("Cook time cannot be negative.");
        if (CookTimeMinutes != cookTimeMinutes)
        {
            CookTimeMinutes = cookTimeMinutes;
            changed = true;
        }

        if (instructions is not null)
        {
            var normalized = string.IsNullOrWhiteSpace(instructions) ? null : instructions.Trim();
            if (!string.Equals(Instructions, normalized, StringComparison.Ordinal))
            {
                Instructions = normalized;
                changed = true;
            }
        }

        if (sourceUrl is not null)
        {
            var normalized = string.IsNullOrWhiteSpace(sourceUrl) ? null : sourceUrl.Trim();
            if (!string.Equals(SourceUrl, normalized, StringComparison.Ordinal))
            {
                SourceUrl = normalized;
                changed = true;
            }
        }

        if (!changed) return;

        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
    }

    /// <summary>
    /// Replaces the entire ingredient list. Each entry's <c>OrderIndex</c> is assigned by position
    /// in the supplied collection so the user-defined ordering is preserved.
    /// </summary>
    public void ReplaceIngredients(
        IReadOnlyList<(string Name, decimal Quantity, string? Unit, string? Notes)> ingredients,
        long modifiedBy)
    {
        if (modifiedBy <= 0)
            throw new BusinessRuleValidationException("ModifiedBy must be a positive number.");

        _ingredients.Clear();

        for (var i = 0; i < ingredients.Count; i++)
        {
            var (name, quantity, unit, notes) = ingredients[i];
            _ingredients.Add(RecipeIngredient.Create(Id, name, quantity, unit, notes, i, modifiedBy));
        }

        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
    }
}
