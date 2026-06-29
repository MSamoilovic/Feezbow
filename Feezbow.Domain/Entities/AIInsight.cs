using Feezbow.Domain.Enums;

namespace Feezbow.Domain.Entities;

public class AIInsight
{
    public long Id { get; private set; }
    public long ProjectId { get; private set; }
    public AIInsightType Type { get; private set; }
    public AIInsightSeverity Severity { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTimeOffset GeneratedAt { get; private set; }
    public DateTimeOffset? DismissedAt { get; private set; }
    public long? DismissedBy { get; private set; }

    private AIInsight() { }

    public static AIInsight Create(
        long projectId,
        AIInsightType type,
        AIInsightSeverity severity,
        string title,
        string description
    ) =>
        new()
        {
            ProjectId = projectId,
            Type = type,
            Severity = severity,
            Title = title,
            Description = description,
            GeneratedAt = DateTimeOffset.UtcNow
        };

    public void Dismiss(long userId)
    {
        DismissedAt = DateTimeOffset.UtcNow;
        DismissedBy = userId;
    }
}
