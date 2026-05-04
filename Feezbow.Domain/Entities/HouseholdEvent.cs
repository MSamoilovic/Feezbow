using Feezbow.Domain.Events.Calendar_Events;
using Feezbow.Domain.Exceptions;

namespace Feezbow.Domain.Entities;

public class HouseholdEvent : AuditableEntity
{
    public long ProjectId { get; private set; }
    public Project Project { get; private set; } = null!;

    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }

    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public bool IsAllDay { get; private set; }

    public string? Category { get; private set; }
    public string? Color { get; private set; }

    public long? AssignedToId { get; private set; }
    public User? AssignedTo { get; private set; }

    private HouseholdEvent() { }

    public static HouseholdEvent Create(
        long projectId,
        string title,
        DateTime startDate,
        long createdBy,
        string? description = null,
        DateTime? endDate = null,
        bool isAllDay = false,
        string? category = null,
        string? color = null,
        long? assignedToId = null)
    {
        if (projectId <= 0)
            throw new BusinessRuleValidationException("ProjectId must be a positive number.");

        if (string.IsNullOrWhiteSpace(title))
            throw new BusinessRuleValidationException("Event title cannot be empty.");

        if (createdBy <= 0)
            throw new BusinessRuleValidationException("CreatedBy must be a positive number.");

        if (endDate.HasValue && endDate.Value < startDate)
            throw new BusinessRuleValidationException("End date cannot be before start date.");

        var ev = new HouseholdEvent
        {
            ProjectId = projectId,
            Title = title.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            StartDate = startDate,
            EndDate = endDate,
            IsAllDay = isAllDay,
            Category = string.IsNullOrWhiteSpace(category) ? null : category.Trim().ToLowerInvariant(),
            Color = string.IsNullOrWhiteSpace(color) ? null : color.Trim(),
            AssignedToId = assignedToId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        ev.AddDomainEvent(new HouseholdEventCreatedEvent(ev.Id, projectId, ev.Title, startDate, createdBy));
        return ev;
    }

    public void Delete(long deletedBy)
    {
        if (deletedBy <= 0)
            throw new BusinessRuleValidationException("DeletedBy must be a positive number.");

        AddDomainEvent(new HouseholdEventDeletedEvent(Id, ProjectId, deletedBy));
    }

    public void Update(
        string? title,
        string? description,
        DateTime? startDate,
        DateTime? endDate,
        bool? isAllDay,
        string? category,
        string? color,
        long? assignedToId,
        long updatedBy)
    {
        if (updatedBy <= 0)
            throw new BusinessRuleValidationException("UpdatedBy must be a positive number.");

        var changed = false;

        if (!string.IsNullOrWhiteSpace(title))
        {
            var normalized = title.Trim();
            if (!string.Equals(Title, normalized, StringComparison.Ordinal))
            {
                Title = normalized;
                changed = true;
            }
        }

        if (description is not null)
        {
            var normalized = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            if (!string.Equals(Description, normalized, StringComparison.Ordinal))
            {
                Description = normalized;
                changed = true;
            }
        }

        var effectiveStart = startDate ?? StartDate;
        var effectiveEnd = endDate ?? EndDate;

        if (effectiveEnd.HasValue && effectiveEnd.Value < effectiveStart)
            throw new BusinessRuleValidationException("End date cannot be before start date.");

        if (startDate.HasValue && StartDate != startDate.Value)
        {
            StartDate = startDate.Value;
            changed = true;
        }

        if (endDate is not null && EndDate != endDate)
        {
            EndDate = endDate;
            changed = true;
        }

        if (isAllDay.HasValue && IsAllDay != isAllDay.Value)
        {
            IsAllDay = isAllDay.Value;
            changed = true;
        }

        if (category is not null)
        {
            var normalized = string.IsNullOrWhiteSpace(category) ? null : category.Trim().ToLowerInvariant();
            if (!string.Equals(Category, normalized, StringComparison.Ordinal))
            {
                Category = normalized;
                changed = true;
            }
        }

        if (color is not null)
        {
            var normalized = string.IsNullOrWhiteSpace(color) ? null : color.Trim();
            if (!string.Equals(Color, normalized, StringComparison.Ordinal))
            {
                Color = normalized;
                changed = true;
            }
        }

        if (assignedToId != AssignedToId)
        {
            AssignedToId = assignedToId;
            changed = true;
        }

        if (!changed) return;

        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = updatedBy;

        AddDomainEvent(new HouseholdEventUpdatedEvent(Id, ProjectId, updatedBy));
    }
}
