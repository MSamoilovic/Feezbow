using MediatR;
using Feezbow.Domain.Enums;

namespace Feezbow.Application.Features.MealPlans.RemoveMealPlanSlot;

public record RemoveMealPlanSlotCommand(
    long MealPlanId,
    DayOfWeek DayOfWeek,
    MealType MealType) : IRequest<RemoveMealPlanSlotCommandResponse>;
