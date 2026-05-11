using Feezbow.Domain.Entities;

namespace Feezbow.Application.Features.Dashboard.Common;

public record DashboardChoreDto(
    long Id,
    string Title,
    DateTime? DueDate,
    bool IsOverdue,
    string Priority,
    long? AssignedToId)
{
    public static DashboardChoreDto FromEntity(HouseholdChore chore, DateTime now) => new(
        chore.Id,
        chore.Title,
        chore.DueDate,
        chore.DueDate.HasValue && chore.DueDate.Value < now,
        chore.Priority.ToString(),
        chore.AssignedToUserId);
}
