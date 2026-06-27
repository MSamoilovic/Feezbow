namespace Feezbow.Application.Features.MealPlans.SuggestMealPlan;

public record SuggestMealPlanQueryResponse(
    long MealPlanId,
    IReadOnlyList<MealSuggestionDto> Suggestions);
