using FluentValidation;
using MarketingPlatform.Application.DTOs.SuppressionList;

namespace MarketingPlatform.Application.Validators
{
    public class CreateSuppressionListValidator : AbstractValidator<CreateSuppressionListDto>
    {
        public CreateSuppressionListValidator()
        {
            RuleFor(x => x.PhoneOrEmail)
                .NotEmpty()
                .WithMessage("Phone number or email is required");

            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage("Invalid suppression type");
        }
    }
}
