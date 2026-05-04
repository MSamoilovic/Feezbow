using FluentValidation;

namespace Feezbow.Application.Features.Calendar.UpdateHouseholdEvent;

public class UpdateHouseholdEventCommandValidator : AbstractValidator<UpdateHouseholdEventCommand>
{
    public UpdateHouseholdEventCommandValidator()
    {
        RuleFor(x => x.EventId).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200).When(x => x.Title is not null);
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description is not null);
        RuleFor(x => x.Category).MaximumLength(50).When(x => x.Category is not null);
        RuleFor(x => x.Color).MaximumLength(20).When(x => x.Color is not null);
        RuleFor(x => x.AssignedToId).GreaterThan(0L).When(x => x.AssignedToId.HasValue);
        RuleFor(x => x)
            .Must(x => !x.EndDate.HasValue || !x.StartDate.HasValue || x.EndDate >= x.StartDate)
            .WithName("EndDate")
            .WithMessage("End date must be on or after start date.");
    }
}
