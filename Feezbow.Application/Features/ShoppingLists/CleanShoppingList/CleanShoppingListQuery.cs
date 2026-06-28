using MediatR;

namespace Feezbow.Application.Features.ShoppingLists.CleanShoppingList;

public record CleanShoppingListQuery(long ShoppingListId) : IRequest<CleanShoppingListQueryResponse>;
