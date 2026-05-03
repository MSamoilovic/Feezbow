using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Feezbow.Application.Features.MealPlans.CreateMealPlan;
using Feezbow.Application.Features.MealPlans.DeleteMealPlan;
using Feezbow.Application.Features.MealPlans.GenerateShoppingList;
using Feezbow.Application.Features.MealPlans.GetMealPlanByWeek;
using Feezbow.Application.Features.MealPlans.GetRecentMealPlans;
using Feezbow.Application.Features.MealPlans.RemoveMealPlanSlot;
using Feezbow.Application.Features.MealPlans.SetMealPlanSlot;
using Feezbow.Application.Features.MealPlans.UpdateMealPlan;
using Feezbow.Domain.Enums;

namespace Feezbow.Controllers;

[Authorize(Policy = "EmailConfirmed")]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class MealPlansController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Creates a meal plan for the given week (auto-snapped to Monday).
    /// </summary>
    [HttpPost("projects/{projectId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(long projectId, [FromBody] CreateMealPlanCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(command with { ProjectId = projectId }, cancellationToken));
    }

    /// <summary>
    /// Returns the meal plan for the given week (defaults to current week if weekStart is omitted).
    /// </summary>
    [HttpGet("projects/{projectId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByWeek(long projectId, [FromQuery] DateTime? weekStart,
        CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetMealPlanByWeekQuery(projectId, weekStart), cancellationToken));
    }

    /// <summary>
    /// Returns the most recent meal plans for the project.
    /// </summary>
    [HttpGet("projects/{projectId:long}/recent")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRecent(long projectId, [FromQuery] int count = 4,
        CancellationToken cancellationToken = default)
    {
        return Ok(await mediator.Send(new GetRecentMealPlansQuery(projectId, count), cancellationToken));
    }

    /// <summary>
    /// Updates plan-level metadata (notes).
    /// </summary>
    [HttpPut("{mealPlanId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(long mealPlanId, [FromBody] UpdateMealPlanCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(command with { MealPlanId = mealPlanId }, cancellationToken));
    }

    /// <summary>
    /// Deletes the meal plan and all its slots.
    /// </summary>
    [HttpDelete("{mealPlanId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long mealPlanId, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new DeleteMealPlanCommand(mealPlanId), cancellationToken));
    }

    /// <summary>
    /// Upserts a single meal slot — creates or replaces the dish at (day, mealType).
    /// </summary>
    [HttpPut("{mealPlanId:long}/slots")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetSlot(long mealPlanId, [FromBody] SetMealPlanSlotCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(command with { MealPlanId = mealPlanId }, cancellationToken));
    }

    /// <summary>
    /// Removes the meal slot at (day, mealType).
    /// </summary>
    [HttpDelete("{mealPlanId:long}/slots")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveSlot(long mealPlanId,
        [FromQuery] DayOfWeek day, [FromQuery] MealType mealType,
        CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new RemoveMealPlanSlotCommand(mealPlanId, day, mealType), cancellationToken));
    }

    /// <summary>
    /// Generates a brand-new shopping list from this meal plan: aggregates ingredients across all
    /// recipe-linked slots (same name + unit are summed). Free-text slots without a recipe are skipped.
    /// </summary>
    [HttpPost("{mealPlanId:long}/generate-shopping-list")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerateShoppingList(long mealPlanId,
        [FromBody] GenerateShoppingListCommand? command,
        CancellationToken cancellationToken)
    {
        var cmd = (command ?? new GenerateShoppingListCommand(mealPlanId)) with { MealPlanId = mealPlanId };
        return Ok(await mediator.Send(cmd, cancellationToken));
    }
}
