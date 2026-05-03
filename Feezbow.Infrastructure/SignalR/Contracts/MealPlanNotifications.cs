using Feezbow.Domain.Enums;

namespace Feezbow.Infrastructure.SignalR.Contracts;

public record MealPlanCreatedNotification
{
    public required long MealPlanId { get; init; }
    public required long ProjectId { get; init; }
    public required DateTime WeekStart { get; init; }
    public required long CreatedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record MealPlanUpdatedNotification
{
    public required long MealPlanId { get; init; }
    public required long ProjectId { get; init; }
    public required long ModifiedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record MealPlanDeletedNotification
{
    public required long MealPlanId { get; init; }
    public required long ProjectId { get; init; }
    public required long DeletedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record MealPlanItemSetNotification
{
    public required long MealPlanId { get; init; }
    public required long ProjectId { get; init; }
    public required long ItemId { get; init; }
    public required DayOfWeek DayOfWeek { get; init; }
    public required MealType MealType { get; init; }
    public required string Title { get; init; }
    public long? AssignedCookId { get; init; }
    public required bool IsReplacement { get; init; }
    public required long ModifiedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record MealPlanItemRemovedNotification
{
    public required long MealPlanId { get; init; }
    public required long ProjectId { get; init; }
    public required DayOfWeek DayOfWeek { get; init; }
    public required MealType MealType { get; init; }
    public required long RemovedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}
