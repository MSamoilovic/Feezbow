using MediatR;
using Feezbow.Application.Features.Recipes.Common;

namespace Feezbow.Application.Features.Recipes.GetRecipesByProject;

public record GetRecipesByProjectQuery(long ProjectId, int Skip = 0, int Take = 50)
    : IRequest<IReadOnlyList<RecipeDto>>;
