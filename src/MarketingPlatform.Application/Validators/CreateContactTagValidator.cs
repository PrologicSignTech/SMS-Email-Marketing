using FluentValidation;
using MarketingPlatform.Application.DTOs.ContactTag;

namespace MarketingPlatform.Application.Validators
{
    public class CreateContactTagValidator : AbstractValidator<CreateContactTagDto>
    {
        public CreateContactTagValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Tag name is required")
                .MaximumLength(50)
                .WithMessage("Tag name cannot exceed 50 characters");

            RuleFor(x => x.Color)
                .MaximumLength(20)
                .When(x => !string.IsNullOrEmpty(x.Color))
                .WithMessage("Color cannot exceed 20 characters");
        }
    }
}
