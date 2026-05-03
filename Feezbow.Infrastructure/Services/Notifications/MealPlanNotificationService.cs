using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Polly;
using Feezbow.Application.Common.Interfaces;
using Feezbow.Domain.Enums;
using Feezbow.Infrastructure.SignalR.Contracts;
using Feezbow.Infrastructure.SignalR.Hubs;
using Feezbow.Infrastructure.SignalR.Resilience;

namespace Feezbow.Infrastructure.Services.Notifications;

public class MealPlanNotificationService(
    IHubContext<ProjectHub, IProjectHubClient> projectHub,
    ResiliencePipeline pipeline,
    ILogger<MealPlanNotificationService> logger)
    : ResilientNotificationBase(pipeline, logger), IMealPlanNotificationService
{
    private static string Group(long projectId) => $"Project:{projectId}";

    public Task NotifyCreated(long mealPlanId, long projectId, DateTime weekStart, long createdBy, CancellationToken cancellationToken = default)
        => SendAsync(_ => new ValueTask(projectHub.Clients.Group(Group(projectId))
            .MealPlanCreated(new MealPlanCreatedNotification
            {
                MealPlanId = mealPlanId, ProjectId = projectId, WeekStart = weekStart, CreatedBy = createdBy
            })), cancellationToken);

    public Task NotifyUpdated(long mealPlanId, long projectId, long modifiedBy, CancellationToken cancellationToken = default)
        => SendAsync(_ => new ValueTask(projectHub.Clients.Group(Group(projectId))
            .MealPlanUpdated(new MealPlanUpdatedNotification
            {
                MealPlanId = mealPlanId, ProjectId = projectId, ModifiedBy = modifiedBy
            })), cancellationToken);

    public Task NotifyDeleted(long mealPlanId, long projectId, long deletedBy, CancellationToken cancellationToken = default)
        => SendAsync(_ => new ValueTask(projectHub.Clients.Group(Group(projectId))
            .MealPlanDeleted(new MealPlanDeletedNotification
            {
                MealPlanId = mealPlanId, ProjectId = projectId, DeletedBy = deletedBy
            })), cancellationToken);

    public Task NotifyItemSet(
        long mealPlanId, long projectId, long itemId,
        DayOfWeek dayOfWeek, MealType mealType, string title,
        long? assignedCookId, bool isReplacement, long modifiedBy,
        CancellationToken cancellationToken = default)
        => SendAsync(_ => new ValueTask(projectHub.Clients.Group(Group(projectId))
            .MealPlanItemSet(new MealPlanItemSetNotification
            {
                MealPlanId = mealPlanId,
                ProjectId = projectId,
                ItemId = itemId,
                DayOfWeek = dayOfWeek,
                MealType = mealType,
                Title = title,
                AssignedCookId = assignedCookId,
                IsReplacement = isReplacement,
                ModifiedBy = modifiedBy
            })), cancellationToken);

    public Task NotifyItemRemoved(
        long mealPlanId, long projectId,
        DayOfWeek dayOfWeek, MealType mealType, long removedBy,
        CancellationToken cancellationToken = default)
        => SendAsync(_ => new ValueTask(projectHub.Clients.Group(Group(projectId))
            .MealPlanItemRemoved(new MealPlanItemRemovedNotification
            {
                MealPlanId = mealPlanId,
                ProjectId = projectId,
                DayOfWeek = dayOfWeek,
                MealType = mealType,
                RemovedBy = removedBy
            })), cancellationToken);
}
