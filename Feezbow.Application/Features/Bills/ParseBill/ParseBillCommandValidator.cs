using FluentValidation;

namespace Feezbow.Application.Features.Bills.ParseBill;

public class ParseBillCommandValidator : AbstractValidator<ParseBillCommand>
{
    private static readonly string[] AllowedMediaTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp",
        "application/pdf",
    ];

    public ParseBillCommandValidator()
    {
        RuleFor(x => x.ProjectId).GreaterThan(0);

        RuleFor(x => x.FileData)
            .NotEmpty()
            .Must(d => d.Length <= 10 * 1024 * 1024)
            .WithMessage("File must not exceed 10 MB.");

        RuleFor(x => x.MediaType)
            .Must(m => AllowedMediaTypes.Contains(m))
            .WithMessage("Unsupported file type. Allowed: JPEG, PNG, WebP, PDF.");
    }
}
