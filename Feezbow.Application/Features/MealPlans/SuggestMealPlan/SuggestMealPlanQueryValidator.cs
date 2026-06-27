using FluentValidation;

namespace Feezbow.Application.Features.MealPlans.SuggestMealPlan;

public class SuggestMealPlanQueryValidator : AbstractValidator<SuggestMealPlanQuery>
{
    public SuggestMealPlanQueryValidator()
    {
        RuleFor(x => x.ProjectId).GreaterThan(0);
        RuleFor(x => x.MealPlanId).GreaterThan(0);
        RuleFor(x => x.Preferences).MaximumLength(200).When(x => x.Preferences != null);
        RuleFor(x => x.Servings).InclusiveBetween(1, 20).When(x => x.Servings.HasValue);
    }
}
