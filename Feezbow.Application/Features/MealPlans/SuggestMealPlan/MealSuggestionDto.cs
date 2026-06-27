namespace Feezbow.Application.Features.MealPlans.SuggestMealPlan;

public record MealSuggestionDto(
    string DayOfWeek,
    string MealType,
    string RecipeName,
    long? RecipeId,
    IReadOnlyList<string> MatchedIngredients,
    IReadOnlyList<string> MissingIngredients);
