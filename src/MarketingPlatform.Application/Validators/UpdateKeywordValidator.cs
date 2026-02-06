using FluentValidation;
using MarketingPlatform.Application.DTOs.Keyword;

namespace MarketingPlatform.Application.Validators
{
    public class UpdateKeywordValidator : AbstractValidator<UpdateKeywordDto>
    {
        public UpdateKeywordValidator()
        {
            RuleFor(x => x.KeywordText)
                .NotEmpty()
                .WithMessage("Keyword text is required")
                .MaximumLength(50)
                .WithMessage("Keyword text must not exceed 50 characters")
                .Matches("^[A-Za-z0-9]+$")
                .WithMessage("Keyword text must contain only alphanumeric characters (no spaces or special characters)");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Description must not exceed 500 characters");

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Invalid keyword status");

            RuleFor(x => x.ResponseMessage)
                .MaximumLength(1000)
                .WithMessage("Response message must not exceed 1000 characters");
        }
    }
}
