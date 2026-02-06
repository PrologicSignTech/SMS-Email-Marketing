using FluentValidation;
using MarketingPlatform.Application.DTOs.Message;

namespace MarketingPlatform.Application.Validators
{
    public class CreateMessageValidator : AbstractValidator<CreateMessageDto>
    {
        public CreateMessageValidator()
        {
            RuleFor(x => x.CampaignId)
                .GreaterThan(0).WithMessage("Valid campaign ID required");

            RuleFor(x => x.ContactId)
                .GreaterThan(0).WithMessage("Valid contact ID required");

            RuleFor(x => x.Recipient)
                .NotEmpty().WithMessage("Recipient is required");

            RuleFor(x => x.MessageBody)
                .NotEmpty().WithMessage("Message body is required")
                .MaximumLength(1600).WithMessage("SMS message too long (max 1600 chars)");

            RuleFor(x => x.Channel)
                .IsInEnum().WithMessage("Invalid channel type");

            When(x => x.Channel == Core.Enums.ChannelType.Email, () =>
            {
                RuleFor(x => x.Subject)
                    .NotEmpty().WithMessage("Email subject is required");

                RuleFor(x => x.Recipient)
                    .EmailAddress().WithMessage("Invalid email address");
            });

            When(x => x.Channel == Core.Enums.ChannelType.SMS || 
                      x.Channel == Core.Enums.ChannelType.MMS, () =>
            {
                RuleFor(x => x.Recipient)
                    .Matches(@"^\+?[1-9]\d{1,14}$")
                    .WithMessage("Invalid phone number format (E.164)");
            });
        }
    }
}
