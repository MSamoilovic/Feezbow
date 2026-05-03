using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.MealPlans.GenerateShoppingList;

public record GeneratedShoppingListSummary(
    long ShoppingListId,
    string ShoppingListName,
    long ProjectId,
    int ItemsAdded);

public record GenerateShoppingListCommandResponse(Result<GeneratedShoppingListSummary> Result);
