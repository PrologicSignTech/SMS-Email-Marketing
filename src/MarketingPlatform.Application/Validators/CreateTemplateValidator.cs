using FluentValidation;
using MarketingPlatform.Application.DTOs.Template;

namespace MarketingPlatform.Application.Validators
{
    public class CreateTemplateValidator : AbstractValidator<CreateTemplateDto>
    {
        public CreateTemplateValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Template name is required")
                .MaximumLength(200).WithMessage("Template name cannot exceed 200 characters");

            RuleFor(x => x.Channel)
                .IsInEnum().WithMessage("Invalid channel type");

            RuleFor(x => x.Category)
                .IsInEnum().WithMessage("Invalid category");

            RuleFor(x => x.MessageBody)
                .NotEmpty().WithMessage("Message body is required")
                .MaximumLength(5000).WithMessage("Message body too long");

            When(x => x.Channel == Core.Enums.ChannelType.Email, () =>
            {
                RuleFor(x => x.Subject)
                    .NotEmpty().WithMessage("Email subject is required")
                    .MaximumLength(200).WithMessage("Subject too long");
            });

            When(x => x.Channel == Core.Enums.ChannelType.SMS, () =>
            {
                RuleFor(x => x.MessageBody)
                    .MaximumLength(1600).WithMessage("SMS message too long (max 1600 chars)");
            });

            RuleForEach(x => x.Variables)
                .SetValidator(new TemplateVariableValidator());
        }
    }

    public class TemplateVariableValidator : AbstractValidator<TemplateVariableDto>
    {
        public TemplateVariableValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Variable name is required")
                .Matches(@"^[a-zA-Z_][a-zA-Z0-9_]*$")
                .WithMessage("Variable name must be alphanumeric (no spaces, start with letter)");
        }
    }
}
