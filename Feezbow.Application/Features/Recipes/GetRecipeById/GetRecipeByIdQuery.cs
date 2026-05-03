using MediatR;
using Feezbow.Application.Features.Recipes.Common;

namespace Feezbow.Application.Features.Recipes.GetRecipeById;

public record GetRecipeByIdQuery(long RecipeId) : IRequest<RecipeDto>;
