using Feezbow.Application.Common.Interfaces;
using Feezbow.Infrastructure.SignalR.Contracts;
using Feezbow.Infrastructure.SignalR.Hubs;
using Feezbow.Infrastructure.SignalR.Resilience;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Polly;

namespace Feezbow.Infrastructure.Services.Notifications;

public class DigestNotificationService(
    IHubContext<ProjectHub, IProjectHubClient> projectHub,
    ResiliencePipeline pipeline,
    ILogger<DigestNotificationService> logger)
    : ResilientNotificationBase(pipeline, logger), IDigestNotificationService
{
    private static string Group(long projectId) => $"Project:{projectId}";

    public Task BroadcastAsync(long projectId, string markdown, CancellationToken cancellationToken = default)
        => SendAsync(
            _ => new ValueTask(
                projectHub.Clients.Group(Group(projectId))
                    .ReceiveDigest(new DigestReadyNotification
                    {
                        ProjectId = projectId,
                        Markdown = markdown,
                        GeneratedAt = DateTime.UtcNow,
                    })
            ),
            cancellationToken
        );
}
