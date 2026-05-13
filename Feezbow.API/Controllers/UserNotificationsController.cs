using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Feezbow.Application.Features.Notifications.GetNotifications;
using Feezbow.Application.Features.Notifications.MarkAllNotificationsRead;
using Feezbow.Application.Features.Notifications.MarkNotificationRead;

namespace Feezbow.Controllers;

[Authorize(Policy = "EmailConfirmed")]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users/notifications")]
public class UserNotificationsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return Ok(await mediator.Send(new GetNotificationsQuery(unreadOnly, page, pageSize), cancellationToken));
    }

    [HttpPut("{notificationId:long}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkRead(
        long notificationId,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new MarkNotificationReadCommand(notificationId), cancellationToken);
        return NoContent();
    }

    [HttpPut("read-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        await mediator.Send(new MarkAllNotificationsReadCommand(), cancellationToken);
        return NoContent();
    }
}
