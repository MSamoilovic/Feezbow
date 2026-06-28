using FluentValidation;

namespace Feezbow.Application.Features.ShoppingLists.CleanShoppingList;

public class CleanShoppingListQueryValidator : AbstractValidator<CleanShoppingListQuery>
{
    public CleanShoppingListQueryValidator()
    {
        RuleFor(x => x.ShoppingListId).GreaterThan(0);
    }
}
