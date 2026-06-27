using Feezbow.Domain.Models.MealPlanSuggestions;

namespace Feezbow.Domain.Interfaces.Services;

public interface IMealPlanSuggestionService
{
    Task<MealPlanSuggestionContext> GetContextAsync(
        long projectId,
        long mealPlanId,
        long userId,
        CancellationToken cancellationToken = default);
}
