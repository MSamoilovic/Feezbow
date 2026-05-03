using MediatR;
using Feezbow.Application.Features.MealPlans.Common;

namespace Feezbow.Application.Features.MealPlans.GetMealPlanByWeek;

public record GetMealPlanByWeekQuery(long ProjectId, DateTime? WeekStart = null)
    : IRequest<MealPlanDto?>;
