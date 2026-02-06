using FluentValidation;
using MarketingPlatform.Application.DTOs.Contact;

namespace MarketingPlatform.Application.Validators
{
    public class CreateContactValidator : AbstractValidator<CreateContactDto>
    {
        public CreateContactValidator()
        {
            RuleFor(x => x.Email)
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
                .WithMessage("Invalid email format");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().When(x => string.IsNullOrEmpty(x.Email))
                .WithMessage("Either phone number or email is required");

            RuleFor(x => x.Email)
                .NotEmpty().When(x => string.IsNullOrEmpty(x.PhoneNumber))
                .WithMessage("Either phone number or email is required");
        }
    }
}
