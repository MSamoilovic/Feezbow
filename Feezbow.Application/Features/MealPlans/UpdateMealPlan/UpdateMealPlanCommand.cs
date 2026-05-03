using MediatR;

namespace Feezbow.Application.Features.MealPlans.UpdateMealPlan;

public record UpdateMealPlanCommand(long MealPlanId, string? Notes)
    : IRequest<UpdateMealPlanCommandResponse>;
