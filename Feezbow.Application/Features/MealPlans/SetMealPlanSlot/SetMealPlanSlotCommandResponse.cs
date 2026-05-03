using Feezbow.Application.Features.MealPlans.Common;
using Feezbow.Domain.Entities.Common;

namespace Feezbow.Application.Features.MealPlans.SetMealPlanSlot;

public record SetMealPlanSlotCommandResponse(Result<MealPlanItemDto> Result);
