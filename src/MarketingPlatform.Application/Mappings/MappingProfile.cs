using AutoMapper;
using MarketingPlatform.Application.DTOs.User;
using MarketingPlatform.Application.DTOs.Contact;
using MarketingPlatform.Application.DTOs.ContactGroup;
using MarketingPlatform.Application.DTOs.Campaign;
using MarketingPlatform.Application.DTOs.Template;
using MarketingPlatform.Application.DTOs.Message;
using MarketingPlatform.Application.DTOs.SuppressionList;
using MarketingPlatform.Application.DTOs.ContactTag;
using MarketingPlatform.Application.DTOs.Keyword;
using MarketingPlatform.Application.DTOs.URL;
using MarketingPlatform.Application.DTOs.Compliance;
using MarketingPlatform.Application.DTOs.Role;
using MarketingPlatform.Application.DTOs.SuperAdmin;
using MarketingPlatform.Application.DTOs.Configuration;
using MarketingPlatform.Application.DTOs.Subscription;
using MarketingPlatform.Application.DTOs.Billing;
using MarketingPlatform.Application.DTOs.Pricing;
using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Models;

namespace MarketingPlatform.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User mappings
            CreateMap<ApplicationUser, UserDto>();
            CreateMap<UpdateUserDto, ApplicationUser>();

            // Contact mappings
            CreateMap<Contact, ContactDto>();
            CreateMap<CreateContactDto, Contact>();
            CreateMap<UpdateContactDto, Contact>();

            // ContactGroup mappings
            CreateMap<ContactGroup, ContactGroupDto>()
                .ForMember(dest => dest.RuleCriteria, opt => opt.Ignore()); // Handled manually in service
            CreateMap<CreateContactGroupDto, ContactGroup>()
                .ForMember(dest => dest.RuleCriteria, opt => opt.Ignore()); // Handled manually in service

            // Dynamic group rule mappings
            CreateMap<GroupRuleCriteria, GroupRuleCriteriaDto>();
            CreateMap<GroupRuleCriteriaDto, GroupRuleCriteria>();
            CreateMap<GroupRule, GroupRuleDto>();
            CreateMap<GroupRuleDto, GroupRule>();

            // Campaign mappings
            CreateMap<Campaign, CampaignDto>();
            CreateMap<CreateCampaignDto, Campaign>();
            CreateMap<UpdateCampaignDto, Campaign>();
            CreateMap<CampaignContent, CampaignContentDto>();
            CreateMap<CampaignAudience, CampaignAudienceDto>();
            CreateMap<CampaignSchedule, CampaignScheduleDto>();
            
            // Campaign Variant mappings
            CreateMap<CampaignVariant, CampaignVariantDto>();
            CreateMap<CreateCampaignVariantDto, CampaignVariant>();
            CreateMap<UpdateCampaignVariantDto, CampaignVariant>();
            CreateMap<CampaignVariantAnalytics, CampaignVariantAnalyticsDto>();

            // Template mappings
            CreateMap<MessageTemplate, TemplateDto>();
            CreateMap<CreateTemplateDto, MessageTemplate>();
            CreateMap<UpdateTemplateDto, MessageTemplate>();
            // Message mappings
            CreateMap<CampaignMessage, MessageDto>()
                .ForMember(dest => dest.CampaignName, opt => opt.MapFrom(src => src.Campaign.Name))
                .ForMember(dest => dest.ContactName, opt => opt.MapFrom(src => 
                    src.Contact.FirstName + " " + src.Contact.LastName))
                .ForMember(dest => dest.MediaUrls, opt => opt.Ignore()); // Handled manually in service
            CreateMap<CreateMessageDto, CampaignMessage>()
                .ForMember(dest => dest.MediaUrls, opt => opt.Ignore()); // Handled manually in service

            // SuppressionList mappings
            CreateMap<SuppressionList, SuppressionListDto>();
            CreateMap<CreateSuppressionListDto, SuppressionList>();

            // ContactTag mappings
            CreateMap<ContactTag, ContactTagDto>()
                .ForMember(dest => dest.ContactCount, opt => opt.Ignore()); // Handled manually in service
            CreateMap<CreateContactTagDto, ContactTag>();

            // Keyword mappings
            CreateMap<Keyword, KeywordDto>()
                .ForMember(dest => dest.LinkedCampaignName, opt => opt.Ignore()) // Handled manually in service
                .ForMember(dest => dest.OptInGroupName, opt => opt.Ignore()) // Handled manually in service
                .ForMember(dest => dest.ActivityCount, opt => opt.Ignore()); // Handled manually in service
            CreateMap<CreateKeywordDto, Keyword>();
            CreateMap<UpdateKeywordDto, Keyword>();
            CreateMap<KeywordActivity, KeywordActivityDto>()
                .ForMember(dest => dest.KeywordText, opt => opt.Ignore()); // Handled manually in service
            
            // Keyword Reservation mappings
            CreateMap<KeywordReservation, KeywordReservationDto>()
                .ForMember(dest => dest.RequestedByUserName, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedByUserName, opt => opt.Ignore());
            CreateMap<CreateKeywordReservationDto, KeywordReservation>();
            
            // Keyword Assignment mappings
            CreateMap<KeywordAssignment, KeywordAssignmentDto>()
                .ForMember(dest => dest.KeywordText, opt => opt.Ignore())
                .ForMember(dest => dest.CampaignName, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedByUserName, opt => opt.Ignore());
            CreateMap<CreateKeywordAssignmentDto, KeywordAssignment>();
            
            // Keyword Conflict mappings
            CreateMap<KeywordConflict, KeywordConflictDto>()
                .ForMember(dest => dest.RequestingUserName, opt => opt.Ignore())
                .ForMember(dest => dest.ExistingUserName, opt => opt.Ignore())
                .ForMember(dest => dest.ResolvedByUserName, opt => opt.Ignore());
            
            // URL Shortener mappings
            CreateMap<URLShortener, UrlShortenerDto>();
            CreateMap<CreateShortenedUrlDto, URLShortener>();
            CreateMap<URLClick, UrlClickDto>();

            // Compliance mappings
            CreateMap<ComplianceSettings, ComplianceSettingsDto>();
            CreateMap<UpdateComplianceSettingsDto, ComplianceSettings>();
            CreateMap<ContactConsent, ContactConsentDto>();
            CreateMap<ConsentHistory, ConsentHistoryDto>();
            CreateMap<ComplianceAuditLog, ComplianceAuditLogDto>()
                .ForMember(dest => dest.ContactName, opt => opt.Ignore()) // Handled manually in service
                .ForMember(dest => dest.CampaignName, opt => opt.Ignore()); // Handled manually in service

            // Role mappings
            CreateMap<Role, RoleDto>()
                .ForMember(dest => dest.PermissionNames, opt => opt.Ignore()) // Handled manually in service
                .ForMember(dest => dest.UserCount, opt => opt.Ignore()); // Handled manually in service
            CreateMap<CreateRoleDto, Role>()
                .ForMember(dest => dest.Permissions, opt => opt.Ignore()); // Handled manually in service

            // Super Admin mappings
            CreateMap<SuperAdminRole, SuperAdminRoleDto>()
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => 
                    (src.User.FirstName + " " + src.User.LastName).Trim()))
                .ForMember(dest => dest.AssignedByEmail, opt => opt.MapFrom(src => src.AssignedByUser != null ? src.AssignedByUser.Email : null))
                .ForMember(dest => dest.RevokedByEmail, opt => opt.MapFrom(src => src.RevokedByUser != null ? src.RevokedByUser.Email : null));

            CreateMap<PrivilegedActionLog, PrivilegedActionLogDto>()
                .ForMember(dest => dest.ActionTypeName, opt => opt.MapFrom(src => src.ActionType.ToString()))
                .ForMember(dest => dest.SeverityName, opt => opt.MapFrom(src => src.Severity.ToString()))
                .ForMember(dest => dest.PerformedByEmail, opt => opt.MapFrom(src => src.PerformedByUser.Email));

            CreateMap<PlatformConfiguration, PlatformConfigurationDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.ToString()))
                .ForMember(dest => dest.LastModifiedByEmail, opt => opt.MapFrom(src => src.LastModifiedByUser != null ? src.LastModifiedByUser.Email : null));
            // Configuration mappings
            CreateMap<PlatformSetting, PlatformSettingDto>();
            CreateMap<CreatePlatformSettingDto, PlatformSetting>();
            CreateMap<UpdatePlatformSettingDto, PlatformSetting>();

            CreateMap<FeatureToggle, FeatureToggleDto>();
            CreateMap<CreateFeatureToggleDto, FeatureToggle>();
            CreateMap<UpdateFeatureToggleDto, FeatureToggle>();

            CreateMap<ComplianceRule, ComplianceRuleDto>();
            CreateMap<CreateComplianceRuleDto, ComplianceRule>();
            CreateMap<UpdateComplianceRuleDto, ComplianceRule>();

            CreateMap<ComplianceRuleAudit, ComplianceRuleAuditDto>();

            // SubscriptionPlan mappings
            CreateMap<SubscriptionPlan, SubscriptionPlanDto>()
                .ForMember(dest => dest.Features, opt => opt.Ignore()); // Handled manually in service
            CreateMap<CreateSubscriptionPlanDto, SubscriptionPlan>()
                .ForMember(dest => dest.Features, opt => opt.Ignore()); // Handled manually in service

            // Billing mappings
            CreateMap<UserSubscription, UserSubscriptionDto>()
                .ForMember(dest => dest.PlanName, opt => opt.MapFrom(src => src.SubscriptionPlan.Name));
            CreateMap<Invoice, InvoiceDto>();
            CreateMap<BillingHistory, BillingHistoryDto>();

            // Pricing Model mappings
            CreateMap<PricingModel, PricingModelDto>();
            CreateMap<CreatePricingModelDto, PricingModel>();
            CreateMap<UpdatePricingModelDto, PricingModel>();

            // Channel Pricing mappings
            CreateMap<ChannelPricing, ChannelPricingDto>()
                .ForMember(dest => dest.PricingModelName, opt => opt.Ignore());
            CreateMap<CreateChannelPricingDto, ChannelPricing>();
            CreateMap<UpdateChannelPricingDto, ChannelPricing>();

            // Region Pricing mappings
            CreateMap<RegionPricing, RegionPricingDto>()
                .ForMember(dest => dest.PricingModelName, opt => opt.Ignore());
            CreateMap<CreateRegionPricingDto, RegionPricing>();
            CreateMap<UpdateRegionPricingDto, RegionPricing>();

            // Usage Pricing mappings
            CreateMap<UsagePricing, UsagePricingDto>()
                .ForMember(dest => dest.PricingModelName, opt => opt.Ignore());
            CreateMap<CreateUsagePricingDto, UsagePricing>();
            CreateMap<UpdateUsagePricingDto, UsagePricing>();

            // Tax Configuration mappings
            CreateMap<TaxConfiguration, TaxConfigurationDto>();
            CreateMap<CreateTaxConfigurationDto, TaxConfiguration>();
            CreateMap<UpdateTaxConfigurationDto, TaxConfiguration>();
        }
    }
}
