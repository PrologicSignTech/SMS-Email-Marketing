using FluentValidation;
using MarketingPlatform.Application.DTOs.Campaign;

namespace MarketingPlatform.Application.Validators
{
    public class UpdateCampaignValidator : AbstractValidator<UpdateCampaignDto>
    {
        public UpdateCampaignValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Campaign name is required")
                .MaximumLength(200).WithMessage("Campaign name cannot exceed 200 characters");

            When(x => x.Content != null, () =>
            {
                RuleFor(x => x.Content!.MessageBody)
                    .NotEmpty().WithMessage("Message content is required")
                    .MaximumLength(5000).WithMessage("Message body too long");
            });
        }
    }
}
