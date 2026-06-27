using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;
using Feezbow.Domain.Interfaces.Repositories;
using Feezbow.Domain.Interfaces.Services;
using Feezbow.Domain.Models.MealPlanSuggestions;

namespace Feezbow.Domain.Services;

public class MealPlanSuggestionService(
    IUnitOfWork unitOfWork,
    IPantryRepository pantryRepository,
    IRecipeRepository recipeRepository) : IMealPlanSuggestionService
{
    public async Task<MealPlanSuggestionContext> GetContextAsync(
        long projectId,
        long mealPlanId,
        long userId,
        CancellationToken cancellationToken = default)
    {
        var project = await unitOfWork.Projects.GetProjectWithMembersAsync(projectId, cancellationToken)
            ?? throw new NotFoundException("Project", projectId);

        if (!project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        var mealPlan = await unitOfWork.MealPlans.GetByIdAsync(mealPlanId, cancellationToken)
            ?? throw new NotFoundException("MealPlan", mealPlanId);

        if (mealPlan.ProjectId != projectId)
            throw new AccessDeniedException("Meal plan does not belong to this project.");

        var pantryTask = pantryRepository.GetByProjectAsync(projectId, null, null, null, cancellationToken);
        var recipesTask = recipeRepository.GetByProjectAsync(projectId, 0, 200, cancellationToken);

        await Task.WhenAll(pantryTask, recipesTask);

        var filledSlots = mealPlan.Items
            .Select(i => (i.DayOfWeek, i.MealType))
            .ToHashSet();

        var allSlots = Enum.GetValues<System.DayOfWeek>()
            .SelectMany(d => Enum.GetValues<Feezbow.Domain.Enums.MealType>()
                .Select(m => new EmptySlot(d, m)))
            .Where(s => !filledSlots.Contains((s.DayOfWeek, s.MealType)))
            .ToList();

        var pantryEntries = (await pantryTask)
            .Select(p => new PantryEntry(p.Name, p.Quantity, p.Unit))
            .ToList();

        var recipeEntries = (await recipesTask)
            .Select(r => new RecipeEntry(
                r.Id,
                r.Name,
                r.Ingredients.OrderBy(i => i.OrderIndex).Select(i => i.Name).ToList()))
            .ToList();

        return new MealPlanSuggestionContext
        {
            MealPlanId = mealPlanId,
            WeekStart = mealPlan.WeekStart,
            EmptySlots = allSlots,
            PantryItems = pantryEntries,
            Recipes = recipeEntries,
        };
    }
}
