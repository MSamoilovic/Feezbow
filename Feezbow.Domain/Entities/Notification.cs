using Feezbow.Domain.Exceptions;

namespace Feezbow.Domain.Entities;

public class Notification : AuditableEntity
{
    public long UserId { get; private set; }
    public long ProjectId { get; private set; }
    public string Type { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public string? Body { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }

    private Notification() { }

    public static Notification Create(
        long userId, long projectId, string type, string title, string? body, long createdBy)
    {
        if (userId <= 0)
            throw new BusinessRuleValidationException("UserId must be a positive number.");
        if (projectId <= 0)
            throw new BusinessRuleValidationException("ProjectId must be a positive number.");
        if (string.IsNullOrWhiteSpace(type))
            throw new BusinessRuleValidationException("Notification type cannot be empty.");
        if (string.IsNullOrWhiteSpace(title))
            throw new BusinessRuleValidationException("Notification title cannot be empty.");

        return new Notification
        {
            UserId = userId,
            ProjectId = projectId,
            Type = type,
            Title = title,
            Body = string.IsNullOrWhiteSpace(body) ? null : body.Trim(),
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public void MarkAsRead()
    {
        if (IsRead) return;
        IsRead = true;
        ReadAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }
}
