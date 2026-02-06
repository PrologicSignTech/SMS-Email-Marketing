using FluentValidation;
using MarketingPlatform.Application.DTOs.ContactGroup;

namespace MarketingPlatform.Application.Validators
{
    public class CreateContactGroupValidator : AbstractValidator<CreateContactGroupDto>
    {
        public CreateContactGroupValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Group name is required")
                .MaximumLength(200).WithMessage("Group name cannot exceed 200 characters");
        }
    }
}
