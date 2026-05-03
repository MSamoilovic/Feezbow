using Feezbow.Domain.Events.Pantry_Events;
using Feezbow.Domain.Exceptions;

namespace Feezbow.Domain.Entities;

/// <summary>
/// A single pantry/inventory entry tied to a project (household). Tracks current quantity and optional
/// expiration / location metadata. Quantity changes are first-class operations and emit a dedicated event
/// so subscribers (notifications, cache) can react without parsing diffs.
/// </summary>
public class PantryItem : AuditableEntity
{
    public long ProjectId { get; private set; }
    public Project Project { get; private set; } = null!;

    public string Name { get; private set; } = null!;
    public decimal Quantity { get; private set; }
    public string? Unit { get; private set; }
    public string? Location { get; private set; }
    public DateTime? ExpirationDate { get; private set; }
    public string? Notes { get; private set; }

    private PantryItem() { }

    public static PantryItem Create(
        long projectId,
        string name,
        decimal quantity,
        string? unit,
        string? location,
        DateTime? expirationDate,
        string? notes,
        long createdBy)
    {
        if (projectId <= 0)
            throw new BusinessRuleValidationException("ProjectId must be a positive number.");

        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleValidationException("Pantry item name cannot be empty.");

        if (quantity < 0)
            throw new BusinessRuleValidationException("Quantity cannot be negative.");

        if (createdBy <= 0)
            throw new BusinessRuleValidationException("CreatedBy must be a positive number.");

        var item = new PantryItem
        {
            ProjectId = projectId,
            Name = name.Trim(),
            Quantity = quantity,
            Unit = string.IsNullOrWhiteSpace(unit) ? null : unit.Trim(),
            Location = string.IsNullOrWhiteSpace(location) ? null : location.Trim(),
            ExpirationDate = expirationDate,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        item.AddDomainEvent(new PantryItemAddedEvent(
            item.Id, projectId, item.Name, quantity, item.Unit, createdBy));

        return item;
    }

    public void Update(
        string? name,
        string? unit,
        string? location,
        DateTime? expirationDate,
        string? notes,
        long modifiedBy)
    {
        if (modifiedBy <= 0)
            throw new BusinessRuleValidationException("ModifiedBy must be a positive number.");

        var changed = false;

        if (!string.IsNullOrWhiteSpace(name))
        {
            var normalized = name.Trim();
            if (!string.Equals(Name, normalized, StringComparison.Ordinal))
            {
                Name = normalized;
                changed = true;
            }
        }

        if (unit is not null)
        {
            var normalized = string.IsNullOrWhiteSpace(unit) ? null : unit.Trim();
            if (!string.Equals(Unit, normalized, StringComparison.Ordinal))
            {
                Unit = normalized;
                changed = true;
            }
        }

        if (location is not null)
        {
            var normalized = string.IsNullOrWhiteSpace(location) ? null : location.Trim();
            if (!string.Equals(Location, normalized, StringComparison.Ordinal))
            {
                Location = normalized;
                changed = true;
            }
        }

        if (ExpirationDate != expirationDate)
        {
            ExpirationDate = expirationDate;
            changed = true;
        }

        if (notes is not null)
        {
            var normalized = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
            if (!string.Equals(Notes, normalized, StringComparison.Ordinal))
            {
                Notes = normalized;
                changed = true;
            }
        }

        if (!changed) return;

        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;

        AddDomainEvent(new PantryItemUpdatedEvent(Id, ProjectId, modifiedBy));
    }

    /// <summary>
    /// Adjusts quantity by <paramref name="delta"/> (negative to consume, positive to restock).
    /// Final quantity is clamped at 0 and a depletion event is raised when the item runs out.
    /// </summary>
    public void AdjustQuantity(decimal delta, long modifiedBy)
    {
        if (modifiedBy <= 0)
            throw new BusinessRuleValidationException("ModifiedBy must be a positive number.");

        if (delta == 0) return;

        var previous = Quantity;
        var next = Quantity + delta;
        if (next < 0) next = 0;
        if (previous == next) return;

        Quantity = next;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;

        AddDomainEvent(new PantryItemQuantityChangedEvent(
            Id, ProjectId, previous, Quantity, Quantity - previous, modifiedBy));

        if (previous > 0 && Quantity == 0)
            AddDomainEvent(new PantryItemDepletedEvent(Id, ProjectId, Name, modifiedBy));
    }

    public void MarkForRemoval(long removedBy)
    {
        if (removedBy <= 0)
            throw new BusinessRuleValidationException("RemovedBy must be a positive number.");

        AddDomainEvent(new PantryItemRemovedEvent(Id, ProjectId, removedBy));
    }
}
