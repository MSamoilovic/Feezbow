using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.MealPlans.DeleteMealPlan;

public record DeleteMealPlanCommandResponse(Result<bool> Result);
