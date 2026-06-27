using MediatR;

namespace Feezbow.Application.Features.MealPlans.SuggestMealPlan;

public record SuggestMealPlanQuery(
    long ProjectId,
    long MealPlanId,
    string? Preferences,
    int? Servings) : IRequest<SuggestMealPlanQueryResponse>;
