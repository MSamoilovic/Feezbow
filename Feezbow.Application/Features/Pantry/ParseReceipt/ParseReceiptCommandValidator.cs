using FluentValidation;

namespace Feezbow.Application.Features.Pantry.ParseReceipt;

public class ParseReceiptCommandValidator : AbstractValidator<ParseReceiptCommand>
{
    private static readonly string[] _allowedMimeTypes = ["image/jpeg", "image/png", "image/webp"];

    public ParseReceiptCommandValidator()
    {
        RuleFor(x => x.ProjectId).GreaterThan(0);

        RuleFor(x => x.ImageData)
            .NotEmpty()
            .Must(b => b.Length <= 5 * 1024 * 1024)
            .WithMessage("Image must not exceed 5 MB.");

        RuleFor(x => x.MediaType)
            .Must(m => _allowedMimeTypes.Contains(m))
            .WithMessage("Image must be JPEG, PNG, or WebP.");
    }
}
