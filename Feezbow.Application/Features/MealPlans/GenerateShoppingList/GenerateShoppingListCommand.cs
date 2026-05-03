using MediatR;

namespace Feezbow.Application.Features.MealPlans.GenerateShoppingList;

public record GenerateShoppingListCommand(long MealPlanId, string? Name = null)
    : IRequest<GenerateShoppingListCommandResponse>;
