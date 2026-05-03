using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.MealPlans.UpdateMealPlan;

public record UpdateMealPlanCommandResponse(Result<bool> Result);
