using MediatR;
using Feezbow.Domain.Enums;

namespace Feezbow.Application.Features.MealPlans.SetMealPlanSlot;

public record SetMealPlanSlotCommand(
    long MealPlanId,
    DayOfWeek DayOfWeek,
    MealType MealType,
    string Title,
    string? Notes = null,
    long? AssignedCookId = null,
    long? RecipeId = null) : IRequest<SetMealPlanSlotCommandResponse>;
