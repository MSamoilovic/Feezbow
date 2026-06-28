namespace Feezbow.Application.Features.ShoppingLists.CleanShoppingList;

public record MergeSurvivorDto(long Id, string Name, decimal? Quantity, string? Unit);

public record MergeGroupDto(IReadOnlyList<MergeSurvivorDto> Survivors, IReadOnlyList<long> Removed);

public record RenameDto(long Id, string OldName, string NewName);
