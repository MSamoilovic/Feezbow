using Feezbow.Domain.Entities;

namespace Feezbow.Application.Features.Pantry.Common;

public record PantryItemDto(
    long Id,
    long ProjectId,
    string Name,
    decimal Quantity,
    string? Unit,
    string? Location,
    DateTime? ExpirationDate,
    string? Notes,
    DateTime CreatedAt,
    DateTime? LastModifiedAt)
{
    public static PantryItemDto FromEntity(PantryItem p) => new(
        p.Id,
        p.ProjectId,
        p.Name,
        p.Quantity,
        p.Unit,
        p.Location,
        p.ExpirationDate,
        p.Notes,
        p.CreatedAt,
        p.LastModifiedAt);
}
