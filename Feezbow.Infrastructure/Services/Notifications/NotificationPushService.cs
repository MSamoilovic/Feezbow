using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Polly;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Infrastructure.SignalR.Contracts;
using Feezbow.Infrastructure.SignalR.Hubs;
using Feezbow.Infrastructure.SignalR.Resilience;

namespace Feezbow.Infrastructure.Services.Notifications;

public class NotificationPushService(
    IHubContext<NotificationHub, INotificationHubClient> notificationHub,
    ResiliencePipeline pipeline,
    ILogger<NotificationPushService> logger)
    : ResilientNotificationBase(pipeline, logger), INotificationPushService
{
    public async Task PushAsync(
        long userId, long notificationId, string type, string title, string? body,
        CancellationToken cancellationToken = default)
    {
        var payload = new NotificationReceivedPayload
        {
            NotificationId = notificationId,
            Type = type,
            Title = title,
            Body = body
        };

        await SendAsync(_ => new ValueTask(
            notificationHub.Clients.Group($"User:{userId}").NotificationReceived(payload)),
            cancellationToken);
    }

    public async Task PushUnreadCountAsync(long userId, int unreadCount, CancellationToken cancellationToken = default)
    {
        await SendAsync(_ => new ValueTask(
            notificationHub.Clients.Group($"User:{userId}").UnreadCountChanged(unreadCount)),
            cancellationToken);
    }
}
