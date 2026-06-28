namespace Feezbow.Application.Features.ShoppingLists.CleanShoppingList;

public record CleanShoppingListQueryResponse(
    long ShoppingListId,
    IReadOnlyList<MergeGroupDto> Merges,
    IReadOnlyList<RenameDto> Renames,
    bool IsAlreadyClean
);
