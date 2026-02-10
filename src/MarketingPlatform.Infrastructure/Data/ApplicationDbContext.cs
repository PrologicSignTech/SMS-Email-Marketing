using MarketingPlatform.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MarketingPlatform.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<UserActivityLog> UserActivityLogs => Set<UserActivityLog>();
        public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
        public DbSet<UserSubscription> UserSubscriptions => Set<UserSubscription>();
        public DbSet<UsageTracking> UsageTrackings => Set<UsageTracking>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<BillingHistory> BillingHistories => Set<BillingHistory>();
        public DbSet<Contact> Contacts => Set<Contact>();
        public DbSet<ContactGroup> ContactGroups => Set<ContactGroup>();
        public DbSet<ContactGroupMember> ContactGroupMembers => Set<ContactGroupMember>();
        public DbSet<ContactTag> ContactTags => Set<ContactTag>();
        public DbSet<ContactTagAssignment> ContactTagAssignments => Set<ContactTagAssignment>();
        public DbSet<SuppressionList> SuppressionLists => Set<SuppressionList>();
        public DbSet<ConsentHistory> ConsentHistories => Set<ConsentHistory>();
        public DbSet<Campaign> Campaigns => Set<Campaign>();
        public DbSet<CampaignContent> CampaignContents => Set<CampaignContent>();
        public DbSet<CampaignAudience> CampaignAudiences => Set<CampaignAudience>();
        public DbSet<CampaignSchedule> CampaignSchedules => Set<CampaignSchedule>();
        public DbSet<CampaignMessage> CampaignMessages => Set<CampaignMessage>();
        public DbSet<MessageTemplate> MessageTemplates => Set<MessageTemplate>();
        public DbSet<Keyword> Keywords => Set<Keyword>();
        public DbSet<KeywordActivity> KeywordActivities => Set<KeywordActivity>();
        public DbSet<KeywordReservation> KeywordReservations => Set<KeywordReservation>();
        public DbSet<KeywordAssignment> KeywordAssignments => Set<KeywordAssignment>();
        public DbSet<KeywordConflict> KeywordConflicts => Set<KeywordConflict>();
        public DbSet<Workflow> Workflows => Set<Workflow>();
        public DbSet<WorkflowStep> WorkflowSteps => Set<WorkflowStep>();
        public DbSet<WorkflowExecution> WorkflowExecutions => Set<WorkflowExecution>();
        public DbSet<URLShortener> URLShorteners => Set<URLShortener>();
        public DbSet<URLClick> URLClicks => Set<URLClick>();
        public DbSet<CampaignAnalytics> CampaignAnalytics => Set<CampaignAnalytics>();
        public DbSet<CampaignVariant> CampaignVariants => Set<CampaignVariant>();
        public DbSet<CampaignVariantAnalytics> CampaignVariantAnalytics => Set<CampaignVariantAnalytics>();
        public DbSet<ContactEngagement> ContactEngagements => Set<ContactEngagement>();
        public DbSet<MessageProvider> MessageProviders => Set<MessageProvider>();
        public DbSet<ProviderLog> ProviderLogs => Set<ProviderLog>();
        public DbSet<ComplianceSettings> ComplianceSettings => Set<ComplianceSettings>();
        public DbSet<ComplianceAuditLog> ComplianceAuditLogs => Set<ComplianceAuditLog>();
        public DbSet<ContactConsent> ContactConsents => Set<ContactConsent>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<FrequencyControl> FrequencyControls => Set<FrequencyControl>();
        public DbSet<MessageDeliveryAttempt> MessageDeliveryAttempts => Set<MessageDeliveryAttempt>();
        public DbSet<ChannelRoutingConfig> ChannelRoutingConfigs => Set<ChannelRoutingConfig>();
        public DbSet<Role> CustomRoles => Set<Role>();
        public DbSet<Core.Entities.UserRole> CustomUserRoles => Set<Core.Entities.UserRole>();
        public DbSet<ApiRateLimit> ApiRateLimits => Set<ApiRateLimit>();
        public DbSet<RateLimitLog> RateLimitLogs => Set<RateLimitLog>();
        public DbSet<ProviderRateLimit> ProviderRateLimits => Set<ProviderRateLimit>();
        public DbSet<ComplianceRule> ComplianceRules => Set<ComplianceRule>();
        public DbSet<ComplianceRuleAudit> ComplianceRuleAudits => Set<ComplianceRuleAudit>();
        public DbSet<EncryptionAuditLog> EncryptionAuditLogs => Set<EncryptionAuditLog>();
        public DbSet<ExternalAuthProvider> ExternalAuthProviders => Set<ExternalAuthProvider>();
        public DbSet<FeatureToggle> FeatureToggles => Set<FeatureToggle>();
        public DbSet<FileStorageSettings> FileStorageSettings => Set<FileStorageSettings>();
        public DbSet<SuperAdminRole> SuperAdminRoles => Set<SuperAdminRole>();
        public DbSet<PrivilegedActionLog> PrivilegedActionLogs => Set<PrivilegedActionLog>();
        public DbSet<PlatformConfiguration> PlatformConfigurations => Set<PlatformConfiguration>();
        public DbSet<PlatformSetting> PlatformSettings => Set<PlatformSetting>();
        public DbSet<UserExternalLogin> UserExternalLogins => Set<UserExternalLogin>();
        public DbSet<PricingModel> PricingModels => Set<PricingModel>();
        public DbSet<ChannelPricing> ChannelPricings => Set<ChannelPricing>();
        public DbSet<RegionPricing> RegionPricings => Set<RegionPricing>();
        public DbSet<UsagePricing> UsagePricings => Set<UsagePricing>();
        public DbSet<TaxConfiguration> TaxConfigurations => Set<TaxConfiguration>();
        public DbSet<PageContent> PageContents => Set<PageContent>();
        public DbSet<ChatRoom> ChatRooms => Set<ChatRoom>();
        public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
        public DbSet<Feature> Features => Set<Feature>();
        public DbSet<PlanFeatureMapping> PlanFeatureMappings => Set<PlanFeatureMapping>();
        public DbSet<LandingFeature> LandingFeatures => Set<LandingFeature>();
        public DbSet<LandingFaq> LandingFaqs => Set<LandingFaq>();
        public DbSet<Testimonial> Testimonials => Set<Testimonial>();
        public DbSet<UseCase> UseCases => Set<UseCase>();
        public DbSet<TrustedCompany> TrustedCompanies => Set<TrustedCompany>();
        public DbSet<SecurityBadge> SecurityBadges => Set<SecurityBadge>();
        public DbSet<LandingStat> LandingStats => Set<LandingStat>();
        public DbSet<FooterSettings> FooterSettings => Set<FooterSettings>();
        public DbSet<PhoneNumber> PhoneNumbers => Set<PhoneNumber>();
        public DbSet<SuppressionRule> SuppressionRules => Set<SuppressionRule>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all entity configurations from assembly
            // This includes KeywordConfiguration which defines the Keyword -> Campaign foreign key relationship
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // Prevent cascade cycle errors on SQL Server:
            // Set all FK relationships to NoAction (Restrict) by default
            foreach (var relationship in modelBuilder.Model.GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }

            // Global query filters for soft delete
            modelBuilder.Entity<Contact>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<Campaign>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<CampaignVariant>().HasQueryFilter(v => !v.IsDeleted);
            modelBuilder.Entity<ContactGroup>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<MessageTemplate>().HasQueryFilter(m => !m.IsDeleted);
            modelBuilder.Entity<Workflow>().HasQueryFilter(w => !w.IsDeleted);
            modelBuilder.Entity<PlatformSetting>().HasQueryFilter(s => !s.IsDeleted);
            modelBuilder.Entity<FeatureToggle>().HasQueryFilter(f => !f.IsDeleted);
            modelBuilder.Entity<ComplianceRule>().HasQueryFilter(r => !r.IsDeleted);
            modelBuilder.Entity<PageContent>().HasQueryFilter(p => !p.IsDeleted);
            // Note: ComplianceRuleAudit does not have soft delete - audit records are permanent
        }
    }
}
