using Feezbow.Domain.Entities;

namespace Feezbow.Application.Features.Notifications.Common;

public record NotificationDto(
    long Id,
    long ProjectId,
    string Type,
    string Title,
    string? Body,
    bool IsRead,
    DateTime? ReadAt,
    DateTime CreatedAt)
{
    public static NotificationDto FromEntity(Notification n) => new(
        n.Id, n.ProjectId, n.Type, n.Title, n.Body, n.IsRead, n.ReadAt, n.CreatedAt);
}
