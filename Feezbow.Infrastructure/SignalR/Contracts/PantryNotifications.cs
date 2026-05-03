namespace Feezbow.Infrastructure.SignalR.Contracts;

public record PantryItemAddedNotification
{
    public required long PantryItemId { get; init; }
    public required long ProjectId { get; init; }
    public required string Name { get; init; }
    public required decimal Quantity { get; init; }
    public string? Unit { get; init; }
    public required long AddedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record PantryItemUpdatedNotification
{
    public required long PantryItemId { get; init; }
    public required long ProjectId { get; init; }
    public required long ModifiedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record PantryItemQuantityChangedNotification
{
    public required long PantryItemId { get; init; }
    public required long ProjectId { get; init; }
    public required decimal PreviousQuantity { get; init; }
    public required decimal NewQuantity { get; init; }
    public required decimal Delta { get; init; }
    public required long ModifiedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record PantryItemDepletedNotification
{
    public required long PantryItemId { get; init; }
    public required long ProjectId { get; init; }
    public required string Name { get; init; }
    public required long ModifiedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}

public record PantryItemRemovedNotification
{
    public required long PantryItemId { get; init; }
    public required long ProjectId { get; init; }
    public required long RemovedBy { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}
