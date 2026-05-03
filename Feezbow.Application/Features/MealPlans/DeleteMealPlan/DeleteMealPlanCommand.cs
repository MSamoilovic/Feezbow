using MediatR;

namespace Feezbow.Application.Features.MealPlans.DeleteMealPlan;

public record DeleteMealPlanCommand(long MealPlanId) : IRequest<DeleteMealPlanCommandResponse>;
