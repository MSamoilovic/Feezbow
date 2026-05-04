using FluentValidation;

namespace Feezbow.Application.Features.Calendar.CreateHouseholdEvent;

public class CreateHouseholdEventCommandValidator : AbstractValidator<CreateHouseholdEventCommand>
{
    public CreateHouseholdEventCommandValidator()
    {
        RuleFor(x => x.ProjectId).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description is not null);
        RuleFor(x => x.Category).MaximumLength(50).When(x => x.Category is not null);
        RuleFor(x => x.Color).MaximumLength(20).When(x => x.Color is not null);
        RuleFor(x => x.AssignedToId).GreaterThan(0L).When(x => x.AssignedToId.HasValue);
        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.EndDate.HasValue)
            .WithMessage("End date must be on or after start date.");
    }
}
