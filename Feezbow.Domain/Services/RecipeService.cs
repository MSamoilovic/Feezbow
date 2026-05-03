using Feezbow.Domain.Entities;
using Feezbow.Domain.Exceptions;
using Feezbow.Domain.Interfaces;
using Feezbow.Domain.Interfaces.Repositories;
using Feezbow.Domain.Interfaces.Services;

namespace Feezbow.Domain.Services;

public class RecipeService(
    IRecipeRepository recipeRepository,
    IProjectRepository projectRepository,
    IUnitOfWork unitOfWork) : IRecipeService
{
    public async Task<Recipe> CreateAsync(
        long projectId,
        long userId,
        string name,
        string? description,
        int servings,
        int? prepTimeMinutes,
        int? cookTimeMinutes,
        string? instructions,
        string? sourceUrl,
        IReadOnlyList<(string Name, decimal Quantity, string? Unit, string? Notes)> ingredients,
        CancellationToken cancellationToken = default)
    {
        await EnsureMemberAsync(projectId, userId, cancellationToken);

        var recipe = Recipe.Create(
            projectId, name, description, servings,
            prepTimeMinutes, cookTimeMinutes, instructions, sourceUrl, userId);

        if (ingredients is { Count: > 0 })
            recipe.ReplaceIngredients(ingredients, userId);

        await recipeRepository.AddAsync(recipe, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        return recipe;
    }

    public async Task<Recipe> GetAsync(
        long recipeId, long userId, CancellationToken cancellationToken = default)
    {
        var recipe = await recipeRepository.GetByIdAsync(recipeId, cancellationToken)
            ?? throw new NotFoundException(nameof(Recipe), recipeId);

        if (!recipe.Project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        return recipe;
    }

    public async Task<IReadOnlyList<Recipe>> GetByProjectAsync(
        long projectId, long userId, int skip, int take, CancellationToken cancellationToken = default)
    {
        if (skip < 0)
            throw new BusinessRuleValidationException("Skip cannot be negative.");

        if (take is <= 0 or > 200)
            throw new BusinessRuleValidationException("Take must be between 1 and 200.");

        await EnsureMemberAsync(projectId, userId, cancellationToken);
        return await recipeRepository.GetByProjectAsync(projectId, skip, take, cancellationToken);
    }

    public async Task<long> UpdateAsync(
        long recipeId,
        long userId,
        string? name,
        string? description,
        int? servings,
        int? prepTimeMinutes,
        int? cookTimeMinutes,
        string? instructions,
        string? sourceUrl,
        CancellationToken cancellationToken = default)
    {
        var recipe = await LoadOwnedRecipeAsync(recipeId, userId, cancellationToken);

        recipe.Update(name, description, servings, prepTimeMinutes, cookTimeMinutes, instructions, sourceUrl, userId);
        await unitOfWork.CompleteAsync(cancellationToken);

        return recipe.ProjectId;
    }

    public async Task<long> ReplaceIngredientsAsync(
        long recipeId,
        long userId,
        IReadOnlyList<(string Name, decimal Quantity, string? Unit, string? Notes)> ingredients,
        CancellationToken cancellationToken = default)
    {
        var recipe = await LoadOwnedRecipeAsync(recipeId, userId, cancellationToken);
        recipe.ReplaceIngredients(ingredients, userId);
        await unitOfWork.CompleteAsync(cancellationToken);
        return recipe.ProjectId;
    }

    public async Task<long> DeleteAsync(
        long recipeId, long userId, CancellationToken cancellationToken = default)
    {
        var recipe = await LoadOwnedRecipeAsync(recipeId, userId, cancellationToken);
        var projectId = recipe.ProjectId;

        recipeRepository.Remove(recipe);
        await unitOfWork.CompleteAsync(cancellationToken);

        return projectId;
    }

    private async Task EnsureMemberAsync(long projectId, long userId, CancellationToken ct)
    {
        var project = await projectRepository.GetProjectWithMembersAsync(projectId, ct)
            ?? throw new NotFoundException(nameof(Project), projectId);

        if (!project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");
    }

    private async Task<Recipe> LoadOwnedRecipeAsync(long recipeId, long userId, CancellationToken ct)
    {
        var recipe = await recipeRepository.GetByIdAsync(recipeId, ct)
            ?? throw new NotFoundException(nameof(Recipe), recipeId);

        if (!recipe.Project.IsMember(userId))
            throw new AccessDeniedException("You are not a member of this project.");

        return recipe;
    }
}
