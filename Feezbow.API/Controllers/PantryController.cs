using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Feezbow.Application.Features.Pantry.AddPantryItem;
using Feezbow.Application.Features.Pantry.AdjustPantryItemQuantity;
using Feezbow.Application.Features.Pantry.GetPantryItemById;
using Feezbow.Application.Features.Pantry.GetPantryItemsByProject;
using Feezbow.Application.Features.Pantry.RemovePantryItem;
using Feezbow.Application.Features.Pantry.UpdatePantryItem;

namespace Feezbow.Controllers;

[Authorize(Policy = "EmailConfirmed")]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class PantryController(IMediator mediator) : ControllerBase
{
    /// <summary>Adds a new pantry/inventory item to the project.</summary>
    [HttpPost("projects/{projectId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Add(long projectId, [FromBody] AddPantryItemCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(command with { ProjectId = projectId }, cancellationToken));
    }

    /// <summary>
    /// Lists pantry items for the project. Supports optional filters: <c>search</c> (ILIKE on name),
    /// <c>location</c> (exact match), <c>expiringWithinDays</c> (items expiring on or before today + N days).
    /// </summary>
    [HttpGet("projects/{projectId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByProject(long projectId,
        [FromQuery] string? search,
        [FromQuery] string? location,
        [FromQuery] int? expiringWithinDays,
        CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(
            new GetPantryItemsByProjectQuery(projectId, search, location, expiringWithinDays), cancellationToken));
    }

    /// <summary>Returns a single pantry item.</summary>
    [HttpGet("{pantryItemId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long pantryItemId, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetPantryItemByIdQuery(pantryItemId), cancellationToken));
    }

    /// <summary>Updates pantry item metadata (name, unit, location, expiration, notes). Quantity is changed via the dedicated endpoint.</summary>
    [HttpPut("{pantryItemId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(long pantryItemId, [FromBody] UpdatePantryItemCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(command with { PantryItemId = pantryItemId }, cancellationToken));
    }

    /// <summary>
    /// Adjusts pantry item quantity by <c>delta</c> (negative consumes, positive restocks).
    /// Quantity is clamped at zero; if it falls to zero a depletion event is emitted.
    /// </summary>
    [HttpPut("{pantryItemId:long}/quantity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AdjustQuantity(long pantryItemId, [FromQuery] decimal delta,
        CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(
            new AdjustPantryItemQuantityCommand(pantryItemId, delta), cancellationToken));
    }

    /// <summary>Removes the pantry item.</summary>
    [HttpDelete("{pantryItemId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove(long pantryItemId, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new RemovePantryItemCommand(pantryItemId), cancellationToken));
    }
}
