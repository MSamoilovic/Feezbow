using Feezbow.Domain.Enums;
using Feezbow.Domain.Exceptions;

namespace Feezbow.Domain.Entities;

public class MealPlanItem : AuditableEntity
{
    public long MealPlanId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public MealType MealType { get; private set; }
    public string Title { get; private set; } = null!;
    public string? Notes { get; private set; }
    public long? AssignedCookId { get; private set; }
    public User? AssignedCook { get; private set; }

    /// <summary>Optional link to a recipe in the same project. Set to null when the dish is free-text.</summary>
    public long? RecipeId { get; private set; }
    public Recipe? Recipe { get; private set; }

    private MealPlanItem() { }

    internal static MealPlanItem Create(
        long mealPlanId,
        DayOfWeek dayOfWeek,
        MealType mealType,
        string title,
        string? notes,
        long? assignedCookId,
        long? recipeId,
        long createdBy)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new BusinessRuleValidationException("Meal title cannot be empty.");

        if (assignedCookId is <= 0)
            throw new BusinessRuleValidationException("AssignedCookId must be a positive number.");

        if (recipeId is <= 0)
            throw new BusinessRuleValidationException("RecipeId must be a positive number.");

        return new MealPlanItem
        {
            MealPlanId = mealPlanId,
            DayOfWeek = dayOfWeek,
            MealType = mealType,
            Title = title.Trim(),
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            AssignedCookId = assignedCookId,
            RecipeId = recipeId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    internal void Update(string title, string? notes, long? assignedCookId, long? recipeId, long modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new BusinessRuleValidationException("Meal title cannot be empty.");

        if (assignedCookId is <= 0)
            throw new BusinessRuleValidationException("AssignedCookId must be a positive number.");

        if (recipeId is <= 0)
            throw new BusinessRuleValidationException("RecipeId must be a positive number.");

        Title = title.Trim();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        AssignedCookId = assignedCookId;
        RecipeId = recipeId;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;
    }
}
