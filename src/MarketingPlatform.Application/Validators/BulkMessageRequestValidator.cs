using FluentValidation;
using MarketingPlatform.Application.DTOs.Message;

namespace MarketingPlatform.Application.Validators
{
    public class BulkMessageRequestValidator : AbstractValidator<BulkMessageRequestDto>
    {
        public BulkMessageRequestValidator()
        {
            RuleFor(x => x.CampaignId)
                .GreaterThan(0).WithMessage("Valid campaign ID required");

            RuleFor(x => x.ContactIds)
                .NotEmpty().WithMessage("At least one contact required")
                .Must(x => x.Count <= 10000).WithMessage("Maximum 10,000 contacts per batch");

            RuleFor(x => x.MessageBody)
                .NotEmpty().WithMessage("Message body is required");

            RuleFor(x => x.Channel)
                .IsInEnum().WithMessage("Invalid channel type");
        }
    }
}
