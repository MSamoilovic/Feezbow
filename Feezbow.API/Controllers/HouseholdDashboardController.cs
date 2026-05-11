using Asp.Versioning;
using Feezbow.Application.Features.Dashboard.GetDashboard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Feezbow.Controllers;

[Authorize(Policy = "EmailConfirmed")]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/projects/{projectId:long}/household/dashboard")]
public class HouseholdDashboardController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Returns an aggregated household dashboard for the project:
    /// overdue and upcoming bills (14 days), active chores, this week's meal plan,
    /// pantry items expiring within 7 days, current-month budget summary,
    /// and upcoming calendar events (14 days).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDashboard(
        long projectId,
        CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetDashboardQuery(projectId), cancellationToken));
    }
}
