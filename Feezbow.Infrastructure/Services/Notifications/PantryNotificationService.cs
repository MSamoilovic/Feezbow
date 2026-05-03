using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Polly;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Infrastructure.SignalR.Contracts;
using Feezbow.Infrastructure.SignalR.Hubs;
using Feezbow.Infrastructure.SignalR.Resilience;

namespace Feezbow.Infrastructure.Services.Notifications;

public class PantryNotificationService(
    IHubContext<ProjectHub, IProjectHubClient> projectHub,
    ResiliencePipeline pipeline,
    ILogger<PantryNotificationService> logger)
    : ResilientNotificationBase(pipeline, logger), IPantryNotificationService
{
    private static string Group(long projectId) => $"Project:{projectId}";

    public Task NotifyAdded(long pantryItemId, long projectId, string name, decimal quantity, string? unit, long addedBy, CancellationToken cancellationToken = default)
        => SendAsync(_ => new ValueTask(projectHub.Clients.Group(Group(projectId))
            .PantryItemAdded(new PantryItemAddedNotification
            {
                PantryItemId = pantryItemId, ProjectId = projectId, Name = name, Quantity = quantity, Unit = unit, AddedBy = addedBy
            })), cancellationToken);

    public Task NotifyUpdated(long pantryItemId, long projectId, long modifiedBy, CancellationToken cancellationToken = default)
        => SendAsync(_ => new ValueTask(projectHub.Clients.Group(Group(projectId))
            .PantryItemUpdated(new PantryItemUpdatedNotification
            {
                PantryItemId = pantryItemId, ProjectId = projectId, ModifiedBy = modifiedBy
            })), cancellationToken);

    public Task NotifyQuantityChanged(long pantryItemId, long projectId, decimal previousQuantity, decimal newQuantity, decimal delta, long modifiedBy, CancellationToken cancellationToken = default)
        => SendAsync(_ => new ValueTask(projectHub.Clients.Group(Group(projectId))
            .PantryItemQuantityChanged(new PantryItemQuantityChangedNotification
            {
                PantryItemId = pantryItemId,
                ProjectId = projectId,
                PreviousQuantity = previousQuantity,
                NewQuantity = newQuantity,
                Delta = delta,
                ModifiedBy = modifiedBy
            })), cancellationToken);

    public Task NotifyDepleted(long pantryItemId, long projectId, string name, long modifiedBy, CancellationToken cancellationToken = default)
        => SendAsync(_ => new ValueTask(projectHub.Clients.Group(Group(projectId))
            .PantryItemDepleted(new PantryItemDepletedNotification
            {
                PantryItemId = pantryItemId, ProjectId = projectId, Name = name, ModifiedBy = modifiedBy
            })), cancellationToken);

    public Task NotifyRemoved(long pantryItemId, long projectId, long removedBy, CancellationToken cancellationToken = default)
        => SendAsync(_ => new ValueTask(projectHub.Clients.Group(Group(projectId))
            .PantryItemRemoved(new PantryItemRemovedNotification
            {
                PantryItemId = pantryItemId, ProjectId = projectId, RemovedBy = removedBy
            })), cancellationToken);
}
