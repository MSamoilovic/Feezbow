using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Feezbow.Application.Features.Calendar.CreateHouseholdEvent;
using Feezbow.Application.Features.Calendar.DeleteHouseholdEvent;
using Feezbow.Application.Features.Calendar.GetCalendar;
using Feezbow.Application.Features.Calendar.UpdateHouseholdEvent;

namespace Feezbow.Controllers;

[Authorize(Policy = "EmailConfirmed")]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/projects/{projectId:long}/calendar")]
public class HouseholdCalendarController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Returns an aggregated calendar view for the project — bills, chores, meal plan items and ad-hoc events
    /// whose dates fall within [from, to] (inclusive). Both parameters are required.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCalendar(
        long projectId,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetCalendarQuery(projectId, from, to), cancellationToken));
    }

    /// <summary>
    /// Creates a new ad-hoc calendar event for the household.
    /// </summary>
    [HttpPost("events")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateEvent(
        long projectId,
        [FromBody] CreateHouseholdEventCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(command with { ProjectId = projectId }, cancellationToken));
    }

    /// <summary>
    /// Updates an existing ad-hoc calendar event. Only provided fields are changed.
    /// </summary>
    [HttpPut("events/{eventId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEvent(
        long projectId,
        long eventId,
        [FromBody] UpdateHouseholdEventCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(command with { EventId = eventId }, cancellationToken));
    }

    /// <summary>
    /// Deletes an ad-hoc calendar event.
    /// </summary>
    [HttpDelete("events/{eventId:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEvent(
        long projectId,
        long eventId,
        CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new DeleteHouseholdEventCommand(eventId), cancellationToken));
    }
}
