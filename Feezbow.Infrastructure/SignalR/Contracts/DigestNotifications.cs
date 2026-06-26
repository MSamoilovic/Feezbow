namespace Feezbow.Infrastructure.SignalR.Contracts;

public record DigestReadyNotification
{
    public required long ProjectId { get; init; }
    public required string Markdown { get; init; }
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
}
