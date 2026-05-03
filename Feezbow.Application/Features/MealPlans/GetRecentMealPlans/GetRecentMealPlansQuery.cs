using MediatR;
using Feezbow.Application.Features.MealPlans.Common;

namespace Feezbow.Application.Features.MealPlans.GetRecentMealPlans;

public record GetRecentMealPlansQuery(long ProjectId, int Count = 4)
    : IRequest<IReadOnlyList<MealPlanDto>>;
