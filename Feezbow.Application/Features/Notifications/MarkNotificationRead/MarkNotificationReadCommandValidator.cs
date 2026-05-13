using FluentValidation;

namespace Feezbow.Application.Features.Notifications.MarkNotificationRead;

public class MarkNotificationReadCommandValidator : AbstractValidator<MarkNotificationReadCommand>
{
    public MarkNotificationReadCommandValidator()
    {
        RuleFor(x => x.NotificationId).GreaterThan(0);
    }
}
