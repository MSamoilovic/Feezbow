using FluentValidation;

namespace Feezbow.Application.Features.Dashboard.GetDashboard;

public class GetDashboardQueryValidator : AbstractValidator<GetDashboardQuery>
{
    public GetDashboardQueryValidator()
    {
        RuleFor(x => x.ProjectId).GreaterThan(0);
    }
}
