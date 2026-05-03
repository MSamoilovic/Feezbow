using Feezbow.Application.Features.MealPlans.Common;
using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.MealPlans.CreateMealPlan;

public record CreateMealPlanCommandResponse(Result<MealPlanDto> Result);
