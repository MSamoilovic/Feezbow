using Feezbow.Domain.Entities;
using Feezbow.Domain.Enums;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;
using Feezbow.Domain.Interfaces.Repositories;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Domain.Services;

public class MealPlanService(
    IMealPlanRepository mealPlanRepository,
    IProjectRepository projectRepository,
    IRecipeRepository recipeRepository,
    IShoppingListService shoppingListService,
    IUnitOfWork unitOfWork) : IMealPlanService
{
    public async Task<MealPlan> CreateAsync(
        long projectId,
        long userId,
        DateTime weekStart,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        var project = await EnsureMemberAsync(projectId, userId, cancellationToken);

        var plan = MealPlan.Create(projectId, weekStart, notes, userId);

        if (await mealPlanRepository.ExistsForWeekAsync(projectId, plan.WeekStart, cancellationToken))
            throw new BusinessRuleValidationException("A meal plan already exists for this week.");

        await mealPlanRepository.AddAsync(plan, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        _ = project; // suppress unused-warning; member check is the contract above
        return plan;
    }

    public async Task<MealPlan?> GetByWeekAsync(
        long projectId,
        long userId,
        DateTime weekStart,
        CancellationToken cancellationToken = default)
    {
        await EnsureMemberAsync(projectId, userId, cancellationToken);

        var monday = SnapToMonday(weekStart);
        return await mealPlanRepository.GetByProjectAndWeekAsync(projectId, monday, cancellationToken);
    }

    public async Task<IReadOnlyList<MealPlan>> GetRecentAsync(
        long projectId,
        long userId,
        int count,
        CancellationToken cancellationToken = default)
    {
        if (count <= 0)
            throw new BusinessRuleValidationException("Count must be greater than zero.");

        await EnsureMemberAsync(projectId, userId, cancellationToken);
        return await mealPlanRepository.GetRecentByProjectAsync(projectId, count, cancellationToken);
    }

    public async Task<long> UpdateAsync(
        long mealPlanId,
        long userId,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        var plan = await LoadOwnedPlanAsync(mealPlanId, userId, cancellationToken);
        plan.Update(notes, userId);
        await unitOfWork.CompleteAsync(cancellationToken);
        return plan.ProjectId;
    }

    public async Task<long> DeleteAsync(
        long mealPlanId,
        long userId,
        CancellationToken cancellationToken = default)
    {
        var plan = await LoadOwnedPlanAsync(mealPlanId, userId, cancellationToken);
        var projectId = plan.ProjectId;

        plan.MarkForDeletion(userId);
        mealPlanRepository.Remove(plan);
        await unitOfWork.CompleteAsync(cancellationToken);

        return projectId;
    }

    public async Task<(MealPlanItem Item, long ProjectId)> SetSlotAsync(
        long mealPlanId,
        long userId,
        DayOfWeek dayOfWeek,
        MealType mealType,
        string title,
        string? notes,
        long? assignedCookId,
        long? recipeId,
        CancellationToken cancellationToken = default)
    {
        var plan = await LoadOwnedPlanAsync(mealPlanId, userId, cancellationToken);

        if (assignedCookId.HasValue && !plan.Project.IsMember(assignedCookId.Value))
            throw new BusinessRuleValidationException("Assigned cook must be a member of the project.");

        if (recipeId.HasValue
            && !await recipeRepository.ExistsInProjectAsync(recipeId.Value, plan.ProjectId, cancellationToken))
            throw new BusinessRuleValidationException("Recipe must belong to the same project as the meal plan.");

        var item = plan.SetSlot(dayOfWeek, mealType, title, notes, assignedCookId, recipeId, modifiedBy: userId);
        await unitOfWork.CompleteAsync(cancellationToken);
        return (item, plan.ProjectId);
    }

    public async Task<long> RemoveSlotAsync(
        long mealPlanId,
        long userId,
        DayOfWeek dayOfWeek,
        MealType mealType,
        CancellationToken cancellationToken = default)
    {
        var plan = await LoadOwnedPlanAsync(mealPlanId, userId, cancellationToken);
        plan.RemoveSlot(dayOfWeek, mealType, userId);
        await unitOfWork.CompleteAsync(cancellationToken);
        return plan.ProjectId;
    }

    public async Task<ShoppingList> GenerateShoppingListAsync(
        long mealPlanId,
        long userId,
        string? listName,
        CancellationToken cancellationToken = default)
    {
        var plan = await mealPlanRepository.GetByIdWithRecipeIngredientsAsync(mealPlanId, cancellationToken)
            ?? throw new NotFoundException(nameof(MealPlan), mealPlanId);

        if (!plan.Project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        var aggregated = MealPlanShoppingListAggregator.Aggregate(plan);
        if (aggregated.Count == 0)
            throw new BusinessRuleValidationException(
                "No recipe-linked ingredients found in this meal plan. Add at least one slot with a recipe before generating a shopping list.");

        var name = string.IsNullOrWhiteSpace(listName)
            ? $"Plan ishrane: nedelja od {plan.WeekStart:dd.MM.yyyy}"
            : listName.Trim();

        var list = await shoppingListService.CreateListAsync(plan.ProjectId, name, userId, cancellationToken);

        foreach (var ingredient in aggregated)
        {
            await shoppingListService.AddItemAsync(
                list.Id, ingredient.Name, ingredient.Quantity, ingredient.Unit, notes: null,
                userId, cancellationToken);
        }

        return list;
    }

    private async Task<Project> EnsureMemberAsync(long projectId, long userId, CancellationToken ct)
    {
        var project = await projectRepository.GetProjectWithMembersAsync(projectId, ct)
            ?? throw new NotFoundException(nameof(Project), projectId);

        if (!project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        return project;
    }

    private async Task<MealPlan> LoadOwnedPlanAsync(long mealPlanId, long userId, CancellationToken ct)
    {
        var plan = await mealPlanRepository.GetByIdAsync(mealPlanId, ct)
            ?? throw new NotFoundException(nameof(MealPlan), mealPlanId);

        if (!plan.Project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        return plan;
    }

    private static DateTime SnapToMonday(DateTime input)
    {
        var date = input.Date;
        var diff = ((int)date.DayOfWeek - (int)System.DayOfWeek.Monday + 7) % 7;
        var monday = date.AddDays(-diff);
        return DateTime.SpecifyKind(monday, DateTimeKind.Utc);
    }
}
