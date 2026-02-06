using FluentValidation;
using MarketingPlatform.Application.DTOs.Template;

namespace MarketingPlatform.Application.Validators
{
    public class UpdateTemplateValidator : AbstractValidator<UpdateTemplateDto>
    {
        public UpdateTemplateValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Template name is required")
                .MaximumLength(200).WithMessage("Template name cannot exceed 200 characters");

            RuleFor(x => x.MessageBody)
                .NotEmpty().WithMessage("Message body is required")
                .MaximumLength(5000).WithMessage("Message body too long");

            When(x => x.Variables != null, () =>
            {
                RuleForEach(x => x.Variables)
                    .SetValidator(new TemplateVariableValidator());
            });
        }
    }
}
