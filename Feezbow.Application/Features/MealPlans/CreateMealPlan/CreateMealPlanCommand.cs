using MediatR;

namespace Feezbow.Application.Features.MealPlans.CreateMealPlan;

public record CreateMealPlanCommand(
    long ProjectId,
    DateTime WeekStart,
    string? Notes = null) : IRequest<CreateMealPlanCommandResponse>;
