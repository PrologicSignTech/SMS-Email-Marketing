using FluentValidation;
using MarketingPlatform.Application.DTOs.Campaign;
using MarketingPlatform.Core.Enums;

namespace MarketingPlatform.Application.Validators
{
    public class CreateCampaignValidator : AbstractValidator<CreateCampaignDto>
    {
        public CreateCampaignValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Campaign name is required")
                .MaximumLength(200).WithMessage("Campaign name cannot exceed 200 characters");

            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Invalid campaign type");

            RuleFor(x => x.Content.MessageBody)
                .NotEmpty().WithMessage("Message content is required")
                .MaximumLength(5000).WithMessage("Message body too long");

            RuleFor(x => x.Content.Channel)
                .IsInEnum().WithMessage("Invalid channel type");

            RuleFor(x => x.Audience.TargetType)
                .IsInEnum().WithMessage("Invalid target type");

            RuleFor(x => x.Audience.GroupIds)
                .NotEmpty()
                .When(x => x.Audience.TargetType == TargetType.Groups)
                .WithMessage("Group IDs required when targeting groups");
        }
    }
}
