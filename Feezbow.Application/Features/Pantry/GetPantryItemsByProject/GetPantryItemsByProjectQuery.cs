using MediatR;
using Feezbow.Application.Features.Pantry.Common;

namespace Feezbow.Application.Features.Pantry.GetPantryItemsByProject;

public record GetPantryItemsByProjectQuery(
    long ProjectId,
    string? Search = null,
    string? Location = null,
    int? ExpiringWithinDays = null) : IRequest<IReadOnlyList<PantryItemDto>>;
