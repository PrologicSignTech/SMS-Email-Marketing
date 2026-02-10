-- ============================================================================
-- MarketingPlatform - Complete Seed Data Script (SQL Server)
-- Generated from DbInitializer.cs
--
-- This script is IDEMPOTENT - safe to run multiple times.
-- It checks for existing data before inserting.
--
-- NOTE: AspNetUsers are SKIPPED because password hashing cannot be done
--       in pure SQL. Users must be seeded via the application or a .NET tool.
-- ============================================================================

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

BEGIN TRY

    PRINT '=== Starting MarketingPlatform Seed Data ==='
    PRINT ''

    -- ========================================================================
    -- 1. IDENTITY ROLES (AspNetRoles)
    -- ========================================================================
    PRINT '--- Seeding Identity Roles (AspNetRoles) ---'

    IF NOT EXISTS (SELECT 1 FROM [AspNetRoles] WHERE [Name] = N'Admin')
    BEGIN
        INSERT INTO [AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
        VALUES (NEWID(), N'Admin', N'ADMIN', NEWID());
        PRINT '  Created role: Admin'
    END

    IF NOT EXISTS (SELECT 1 FROM [AspNetRoles] WHERE [Name] = N'User')
    BEGIN
        INSERT INTO [AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
        VALUES (NEWID(), N'User', N'USER', NEWID());
        PRINT '  Created role: User'
    END

    IF NOT EXISTS (SELECT 1 FROM [AspNetRoles] WHERE [Name] = N'Manager')
    BEGIN
        INSERT INTO [AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
        VALUES (NEWID(), N'Manager', N'MANAGER', NEWID());
        PRINT '  Created role: Manager'
    END

    IF NOT EXISTS (SELECT 1 FROM [AspNetRoles] WHERE [Name] = N'SuperAdmin')
    BEGIN
        INSERT INTO [AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
        VALUES (NEWID(), N'SuperAdmin', N'SUPERADMIN', NEWID());
        PRINT '  Created role: SuperAdmin'
    END

    PRINT ''

    -- ========================================================================
    -- 2. CUSTOM ROLES (CustomRoles table)
    --    Permission flag values calculated from Permission enum:
    --      SuperAdmin (All)  = -1
    --      Admin             = 1072431103
    --      Manager           = 243316599
    --      User              = 4403
    --      Analyst           = 41971985
    --      Viewer            = 4369
    -- ========================================================================
    PRINT '--- Seeding Custom Roles ---'

    IF NOT EXISTS (SELECT 1 FROM [CustomRoles])
    BEGIN
        INSERT INTO [CustomRoles] ([Name], [Description], [Permissions], [IsSystemRole], [IsActive], [CreatedAt])
        VALUES
            (N'SuperAdmin', N'Full system access with all permissions', CAST(-1 AS BIGINT), 1, 1, GETUTCDATE()),
            (N'Admin', N'Administrator with most permissions including user/role management', 1072431103, 1, 1, GETUTCDATE()),
            (N'Manager', N'Campaign and contact management with analytics access', 243316599, 1, 1, GETUTCDATE()),
            (N'User', N'Standard user with basic campaign and contact management', 4403, 1, 1, GETUTCDATE()),
            (N'Analyst', N'Read access with detailed analytics capabilities', 41971985, 1, 1, GETUTCDATE()),
            (N'Viewer', N'Read-only access to campaigns and basic analytics', 4369, 1, 1, GETUTCDATE());

        PRINT '  Created 6 custom roles'
    END
    ELSE
    BEGIN
        PRINT '  Custom roles already exist - skipped'
    END

    PRINT ''

    -- ========================================================================
    -- 3. FEATURES (14 features)
    -- ========================================================================
    PRINT '--- Seeding Features ---'

    IF NOT EXISTS (SELECT 1 FROM [Features])
    BEGIN
        INSERT INTO [Features] ([Name], [Description], [IsActive], [DisplayOrder], [CreatedAt], [IsDeleted])
        VALUES
            (N'SMS Messages',              N'Send SMS text messages to your contacts',                1, 1,  GETUTCDATE(), 0),
            (N'MMS Messages',              N'Send multimedia messages with images and videos',        1, 2,  GETUTCDATE(), 0),
            (N'Email Campaigns',           N'Create and send email marketing campaigns',              1, 3,  GETUTCDATE(), 0),
            (N'Contact Management',        N'Organize and manage your contact database',              1, 4,  GETUTCDATE(), 0),
            (N'Basic Analytics',           N'View basic campaign performance metrics',                1, 5,  GETUTCDATE(), 0),
            (N'Advanced Analytics',        N'Detailed insights and custom reports',                   1, 6,  GETUTCDATE(), 0),
            (N'Automation Workflows',      N'Automate your marketing campaigns',                     1, 7,  GETUTCDATE(), 0),
            (N'Custom Templates',          N'Create reusable message templates',                     1, 8,  GETUTCDATE(), 0),
            (N'API Access',                N'Programmatic access to platform features',               1, 9,  GETUTCDATE(), 0),
            (N'Priority Support',          N'Fast-tracked customer support',                         1, 10, GETUTCDATE(), 0),
            (N'24/7 Support',              N'Round-the-clock customer support',                      1, 11, GETUTCDATE(), 0),
            (N'Dedicated Account Manager', N'Personal account management and guidance',               1, 12, GETUTCDATE(), 0),
            (N'White-label Options',       N'Customize the platform with your branding',              1, 13, GETUTCDATE(), 0),
            (N'Team Collaboration',        N'Multiple user accounts and permissions',                 1, 14, GETUTCDATE(), 0);

        PRINT '  Created 14 features'
    END
    ELSE
    BEGIN
        PRINT '  Features already exist - skipped'
    END

    PRINT ''

    -- ========================================================================
    -- 4. SUBSCRIPTION PLANS (3 plans)
    -- ========================================================================
    PRINT '--- Seeding Subscription Plans ---'

    IF NOT EXISTS (SELECT 1 FROM [SubscriptionPlans])
    BEGIN
        INSERT INTO [SubscriptionPlans]
            ([Name], [Description], [PlanCategory], [IsMostPopular],
             [PriceMonthly], [PriceYearly], [SMSLimit], [MMSLimit], [EmailLimit], [ContactLimit],
             [Features], [IsActive], [IsVisible], [ShowOnLanding], [CreatedAt], [IsDeleted])
        VALUES
            (N'Starter',
             N'Perfect for small businesses getting started with marketing automation',
             N'For small businesses', 0,
             29.99, 299.99, 1000, 100, 5000, 1000,
             N'["Basic campaign management", "Basic analytics", "Email support"]',
             1, 1, 1, GETUTCDATE(), 0),

            (N'Professional',
             N'Advanced features for growing teams and increased reach',
             N'For growing teams', 1,
             79.99, 799.99, 10000, 1000, 50000, 10000,
             N'["Advanced campaign management", "Workflows & automation", "Advanced analytics", "Priority support", "Custom templates"]',
             1, 1, 1, GETUTCDATE(), 0),

            (N'Enterprise',
             N'Complete solution with unlimited power and dedicated support',
             N'For large organizations', 0,
             249.99, 2499.99, 100000, 10000, 500000, 100000,
             N'["Unlimited campaigns", "Advanced workflows", "Premium analytics", "24/7 support", "Dedicated account manager", "API access", "White-label options"]',
             1, 1, 1, GETUTCDATE(), 0);

        PRINT '  Created 3 subscription plans'
    END
    ELSE
    BEGIN
        PRINT '  Subscription plans already exist - skipped'
    END

    PRINT ''

    -- ========================================================================
    -- 5. PLAN-FEATURE MAPPINGS
    --    Uses subqueries to reference the IDs of plans and features inserted above.
    -- ========================================================================
    PRINT '--- Seeding Plan-Feature Mappings ---'

    IF NOT EXISTS (SELECT 1 FROM [PlanFeatureMappings])
    BEGIN
        -- Starter Plan Features (6 mappings)
        INSERT INTO [PlanFeatureMappings] ([SubscriptionPlanId], [FeatureId], [IsIncluded], [FeatureValue], [DisplayOrder], [CreatedAt], [IsDeleted])
        VALUES
            ((SELECT [Id] FROM [SubscriptionPlans] WHERE [Name] = N'Starter'),
             (SELECT [Id] FROM [Features] WHERE [Name] = N'SMS Messages'),
             1, N'1,000 messages/month', 1, GETUTCDATE(), 0),

            ((SELECT [Id] FROM [SubscriptionPlans] WHERE [Name] = N'Starter'),
             (SELECT [Id] FROM [Features] WHERE [Name] = N'MMS Messages'),
             1, N'100 messages/month', 2, GETUTCDATE(), 0),

            ((SELECT [Id] FROM [SubscriptionPlans] WHERE [Name] = N'Starter'),
             (SELECT [Id] FROM [Features] WHERE [Name] = N'Email Campaigns'),
             1, N'5,000 emails/month', 3, GETUTCDATE(), 0),

            ((SELECT [Id] FROM [SubscriptionPlans] WHERE [Name] = N'Starter'),
             (SELECT [Id] FROM [Features] WHERE [Name] = N'Contact Management'),
             1, N'Up to 1,000 contacts', 4, GETUTCDATE(), 0),

            ((SELECT [Id] FROM [SubscriptionPlans] WHERE [Name] = N'Starter'),
             (SELECT [Id] FROM [Features] WHERE [Name] = N'Basic Analytics'),
             1, NULL, 5, GETUTCDATE(), 0),

            ((SELECT [Id] FROM [SubscriptionPlans] WHERE [Name] = N'Starter'),
             (SELECT [Id] FROM [Features] WHERE [Name] = N'Custom Templates'),
             1, NULL, 6, GETUTCDATE(), 0);

        -- Professional Plan Features (9 mappings)
        INSERT INTO [PlanFeatureMappings] ([SubscriptionPlanId], [FeatureId], [IsIncluded], [FeatureValue], [DisplayOrder], [CreatedAt], [IsDeleted])
        VALUES
            ((SELECT [Id] FROM [SubscriptionPlans] WHERE [Name] = N'Professional'),
             (SELECT [Id] FROM [Features] WHERE [Name] = N'SMS Messages'),
             1, N'10,000 messages/month', 1, GETUTCDATE(), 0),

            ((SELECT [Id] FROM [SubscriptionPlans] WHERE [Name] = N'Professional'),
             (SELECT [Id] FROM [Features] WHERE [Name] = N'MMS Messages'),
             1, N'1,000 messages/month', 2, GETUTCDATE(), 0),

            ((SELECT [Id] FROM [SubscriptionPlans] WHERE [Name] = N'Professional'),
             (SELECT [Id] FROM [Features] WHERE [Name] = N'Email Campaigns'),
             1, N'50,000 emails/month', 3, GETUTCDATE(), 0),

            ((SELECT [Id] FROM [SubscriptionPlans] WHERE [Name] = N'Professional'),
             (SELECT [Id] FROM [Features] WHERE [Name] = N'Contact Management'),
             1, N'Up to 10,000 contacts', 4, GETUTCDATE(), 0),

            ((SELECT [Id] FROM [SubscriptionPlans] WHERE [Name] = N'Professional'),
             (SELECT [Id] FROM [Features] WHERE [Name] = N'Advanced Analytics'),
             1, NULL, 5, GETUTCDATE(), 0),

            ((SELECT [Id] FROM [SubscriptionPlans] WHERE [Name] = N'Professional'),
             (SELECT [Id] FROM [Features] WHERE [Name] = N'Automation Workflows'),
             1, NULL, 6, GETUTCDATE(), 0),

            ((SELECT [Id] FROM [SubscriptionPlans] WHERE [Name] = N'Professional'),
             (SELECT [Id] FROM [Features] WHERE [Name] = N'Custom Templates'),
             1, N'Unlimited', 7, GETUTCDATE(), 0),

            ((SELECT [Id] FROM [SubscriptionPlans] WHERE [Name] = N'Professional'),
             (SELECT [Id] FROM [Features] WHERE [Name] = N'Priority Support'),
             1, NULL, 8, GETUTCDATE(), 0),

            ((SELECT [Id] FROM [SubscriptionPlans] WHERE [Name] = N'Professional'),
             (SELECT [Id] FROM [Features] WHERE [Name] = N'Team Collaboration'),
             1, N'Up to 5 users', 9, GETUTCDATE(), 0);

        -- Enterprise Plan Features (12 mappings)
        INSERT INTO [PlanFeatureMappings] ([SubscriptionPlanId], [FeatureId], [IsIncluded], [FeatureValue], [DisplayOrder], [CreatedAt], [IsDeleted])
        VALUES
            ((SELECT [Id] FROM [SubscriptionPlans] WHERE [Name] = N'Enterprise'),
             (SELECT [Id] FROM [Features] WHERE [Name] = N'SMS Messages'),
             1, N'100,000 messages/month', 1, GETUTCDATE(), 0),

            ((SELECT [Id] FROM [SubscriptionPlans] WHERE [Name] = N'Enterprise'),
             (SELECT [Id] FROM [Features] WHERE [Name] = N'MMS Messages'),
             1, N'10,000 messages/month', 2, GETUTCDATE(), 0),

            ((SELECT [Id] FROM [SubscriptionPlans] WHERE [Name] = N'Enterprise'),
             (SELECT [Id] FROM [Features] WHERE [Name] = N'Email Campaigns'),
             1, N'500,000 emails/month', 3, GETUTCDATE(), 0),

            ((SELECT [Id] FROM [SubscriptionPlans] WHERE [Name] = N'Enterprise'),
             (SELECT [Id] FROM [Features] WHERE [Name] = N'Contact Management'),
             1, N'Up to 100,000 contacts', 4, GETUTCDATE(), 0),

            ((SELECT [Id] FROM [SubscriptionPlans] WHERE [Name] = N'Enterprise'),
             (SELECT [Id] FROM [Features] WHERE [Name] = N'Advanced Analytics'),
             1, N'Custom reports', 5, GETUTCDATE(), 0),

            ((SELECT [Id] FROM [SubscriptionPlans] WHERE [Name] = N'Enterprise'),
             (SELECT [Id] FROM [Features] WHERE [Name] = N'Automation Workflows'),
             1, N'Advanced', 6, GETUTCDATE(), 0),

            ((SELECT [Id] FROM [SubscriptionPlans] WHERE [Name] = N'Enterprise'),
             (SELECT [Id] FROM [Features] WHERE [Name] = N'Custom Templates'),
             1, N'Unlimited', 7, GETUTCDATE(), 0),

            ((SELECT [Id] FROM [SubscriptionPlans] WHERE [Name] = N'Enterprise'),
             (SELECT [Id] FROM [Features] WHERE [Name] = N'API Access'),
             1, NULL, 8, GETUTCDATE(), 0),

            ((SELECT [Id] FROM [SubscriptionPlans] WHERE [Name] = N'Enterprise'),
             (SELECT [Id] FROM [Features] WHERE [Name] = N'24/7 Support'),
             1, NULL, 9, GETUTCDATE(), 0),

            ((SELECT [Id] FROM [SubscriptionPlans] WHERE [Name] = N'Enterprise'),
             (SELECT [Id] FROM [Features] WHERE [Name] = N'Dedicated Account Manager'),
             1, NULL, 10, GETUTCDATE(), 0),

            ((SELECT [Id] FROM [SubscriptionPlans] WHERE [Name] = N'Enterprise'),
             (SELECT [Id] FROM [Features] WHERE [Name] = N'White-label Options'),
             1, NULL, 11, GETUTCDATE(), 0),

            ((SELECT [Id] FROM [SubscriptionPlans] WHERE [Name] = N'Enterprise'),
             (SELECT [Id] FROM [Features] WHERE [Name] = N'Team Collaboration'),
             1, N'Unlimited users', 12, GETUTCDATE(), 0);

        PRINT '  Created 27 plan-feature mappings (6 Starter + 9 Professional + 12 Enterprise)'
    END
    ELSE
    BEGIN
        PRINT '  Plan-feature mappings already exist - skipped'
    END

    PRINT ''

    -- ========================================================================
    -- 6. MESSAGE PROVIDERS
    --    Enum values: ProviderType { SMS = 0, MMS = 1, Email = 2 }
    --    Enum values: HealthStatus { Healthy = 0, Degraded = 1, Unhealthy = 2, Unknown = 3 }
    -- ========================================================================
    PRINT '--- Seeding Message Providers ---'

    IF NOT EXISTS (SELECT 1 FROM [MessageProviders])
    BEGIN
        INSERT INTO [MessageProviders] ([Name], [Type], [IsActive], [IsPrimary], [HealthStatus], [CreatedAt], [IsDeleted])
        VALUES
            (N'Twilio SMS',    0, 1, 1, 3, GETUTCDATE(), 0),   -- Type=SMS(0), HealthStatus=Unknown(3)
            (N'SendGrid Email', 2, 1, 1, 3, GETUTCDATE(), 0);  -- Type=Email(2), HealthStatus=Unknown(3)

        PRINT '  Created 2 message providers'
    END
    ELSE
    BEGIN
        PRINT '  Message providers already exist - skipped'
    END

    PRINT ''

    -- ========================================================================
    -- 7. CHANNEL ROUTING CONFIGS
    --    Enum values: ChannelType { SMS = 0, MMS = 1, Email = 2 }
    --    Enum values: RoutingStrategy { Primary = 0, Fallback = 1, RoundRobin = 2, LeastCost = 3, HighestReliability = 4 }
    --    Enum values: RetryStrategy { None = 0, Linear = 1, Exponential = 2, Custom = 3 }
    -- ========================================================================
    PRINT '--- Seeding Channel Routing Configs ---'

    IF NOT EXISTS (SELECT 1 FROM [ChannelRoutingConfigs])
    BEGIN
        INSERT INTO [ChannelRoutingConfigs]
            ([Channel], [PrimaryProvider], [FallbackProvider], [RoutingStrategy],
             [EnableFallback], [MaxRetries], [RetryStrategy], [InitialRetryDelaySeconds], [MaxRetryDelaySeconds],
             [IsActive], [Priority], [CreatedAt], [IsDeleted])
        VALUES
            -- SMS routing
            (0, N'MockSMSProvider', N'BackupSMSProvider', 0,    -- Channel=SMS(0), RoutingStrategy=Primary(0)
             1, 3, 2, 60, 3600,                                 -- RetryStrategy=Exponential(2)
             1, 1, GETUTCDATE(), 0),

            -- MMS routing
            (1, N'MockMMSProvider', N'BackupMMSProvider', 0,    -- Channel=MMS(1), RoutingStrategy=Primary(0)
             1, 3, 2, 60, 3600,                                 -- RetryStrategy=Exponential(2)
             1, 1, GETUTCDATE(), 0),

            -- Email routing
            (2, N'MockEmailProvider', N'BackupEmailProvider', 0, -- Channel=Email(2), RoutingStrategy=Primary(0)
             1, 3, 2, 120, 7200,                                 -- RetryStrategy=Exponential(2)
             1, 1, GETUTCDATE(), 0);

        PRINT '  Created 3 channel routing configs'
    END
    ELSE
    BEGIN
        PRINT '  Channel routing configs already exist - skipped'
    END

    PRINT ''

    -- ========================================================================
    -- 8. PRICING MODELS
    --    Enum values: PricingModelType { Flat = 0, Tiered = 1, Volume = 2, PerUnit = 3 }
    --    Enum values: BillingPeriod { Monthly = 0, Quarterly = 1, Yearly = 2, OneTime = 3 }
    -- ========================================================================
    PRINT '--- Seeding Pricing Models ---'

    IF NOT EXISTS (SELECT 1 FROM [PricingModels])
    BEGIN
        INSERT INTO [PricingModels]
            ([Name], [Description], [Type], [BasePrice], [BillingPeriod],
             [IsActive], [Priority], [Configuration], [CreatedAt], [UpdatedAt], [IsDeleted])
        VALUES
            (N'Starter',
             N'Perfect for small businesses getting started',
             0, 29.00, 0,  -- Type=Flat(0), BillingPeriod=Monthly(0)
             1, 1,
             N'{"features":["1,000 SMS messages/month","500 emails/month","Basic analytics","Email support","1 user"]}',
             GETUTCDATE(), GETUTCDATE(), 0),

            (N'Professional',
             N'For growing businesses with larger audiences',
             0, 99.00, 0,  -- Type=Flat(0), BillingPeriod=Monthly(0)
             1, 2,
             N'{"features":["10,000 SMS messages/month","5,000 emails/month","Advanced analytics","Priority support","5 users","Custom templates","Automation workflows"],"isPopular":true}',
             GETUTCDATE(), GETUTCDATE(), 0),

            (N'Enterprise',
             N'For large organizations with custom needs',
             0, 299.00, 0,  -- Type=Flat(0), BillingPeriod=Monthly(0)
             1, 3,
             N'{"features":["Unlimited SMS messages","Unlimited emails","Advanced analytics & reporting","24/7 phone support","Unlimited users","Custom branding","API access","Dedicated account manager"]}',
             GETUTCDATE(), GETUTCDATE(), 0);

        PRINT '  Created 3 pricing models'
    END
    ELSE
    BEGIN
        PRINT '  Pricing models already exist - skipped'
    END

    PRINT ''

    -- ========================================================================
    -- 9. PLATFORM SETTINGS (Landing Page settings)
    --    Enum values: SettingDataType { String = 0, Integer = 1, Boolean = 2, Decimal = 3, Json = 4 }
    --    Enum values: SettingScope { Global = 0, Tenant = 1, User = 2 }
    -- ========================================================================
    PRINT '--- Seeding Platform Settings (Landing Page) ---'

    IF NOT EXISTS (SELECT 1 FROM [PlatformSettings] WHERE [Category] = N'LandingPage' AND [IsDeleted] = 0)
    BEGIN
        INSERT INTO [PlatformSettings] ([Key], [Value], [Category], [Description], [DataType], [Scope], [IsEncrypted], [IsReadOnly], [CreatedAt], [UpdatedAt], [IsDeleted])
        VALUES
            -- Hero Section Settings
            (N'LandingPage.Hero.Type',
             N'banner',
             N'LandingPage', N'Hero section type: ''banner'' or ''slider''',
             0, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),  -- DataType=String(0), Scope=Global(0)

            (N'LandingPage.Hero.Title',
             N'Transform Your Marketing with SMS, MMS & Email',
             N'LandingPage', N'Hero section main title',
             0, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),

            (N'LandingPage.Hero.Subtitle',
             N'A powerful, enterprise-grade marketing platform to reach your customers where they are. Send targeted campaigns, track performance, and grow your business.',
             N'LandingPage', N'Hero section subtitle/description',
             0, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),

            (N'LandingPage.Hero.BannerImage',
             N'/images/hero-banner.jpg',
             N'LandingPage', N'Hero banner image URL',
             0, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),

            (N'LandingPage.Hero.CTAText',
             N'Get Started Free',
             N'LandingPage', N'Primary call-to-action button text',
             0, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),

            (N'LandingPage.Hero.CTALink',
             N'/Auth/Register',
             N'LandingPage', N'Primary call-to-action button link',
             0, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),

            -- Slider Settings
            (N'LandingPage.Slider.Slides',
             N'[{"title":"Transform Your Marketing","subtitle":"Reach customers on SMS, MMS & Email","image":"/images/slide1.jpg","ctaText":"Get Started","ctaLink":"/Auth/Register"},{"title":"Advanced Analytics","subtitle":"Track and optimize your campaigns","image":"/images/slide2.jpg","ctaText":"Learn More","ctaLink":"#features"},{"title":"Automate Your Workflow","subtitle":"Save time with powerful automation","image":"/images/slide3.jpg","ctaText":"See How","ctaLink":"#features"}]',
             N'LandingPage', N'Slider slides configuration (JSON array)',
             4, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),  -- DataType=Json(4)

            (N'LandingPage.Slider.AutoPlay',
             N'true',
             N'LandingPage', N'Enable slider auto-play',
             2, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),  -- DataType=Boolean(2)

            (N'LandingPage.Slider.Interval',
             N'5000',
             N'LandingPage', N'Slider auto-play interval in milliseconds',
             1, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),  -- DataType=Integer(1)

            -- Menu/Navigation Settings
            (N'LandingPage.Menu.BackgroundColor',
             N'#ffffff',
             N'LandingPage', N'Navigation menu background color',
             0, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),

            (N'LandingPage.Menu.TextColor',
             N'#212529',
             N'LandingPage', N'Navigation menu text color',
             0, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),

            (N'LandingPage.Menu.HoverColor',
             N'#667eea',
             N'LandingPage', N'Navigation menu hover color',
             0, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),

            (N'LandingPage.Menu.FontSize',
             N'16',
             N'LandingPage', N'Navigation menu font size (in pixels)',
             1, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),  -- DataType=Integer(1)

            (N'LandingPage.Menu.Position',
             N'top',
             N'LandingPage', N'Navigation menu position: ''top'' or ''fixed''',
             0, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),

            (N'LandingPage.Menu.Items',
             N'[{"text":"Home","link":"#home","order":1},{"text":"Features","link":"#features","order":2},{"text":"Pricing","link":"#pricing","order":3},{"text":"Contact","link":"#contact","order":4},{"text":"Login","link":"/Auth/Login","order":5,"class":"btn-outline-primary"}]',
             N'LandingPage', N'Navigation menu items (JSON array)',
             4, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),  -- DataType=Json(4)

            -- Theme Colors
            (N'LandingPage.Theme.PrimaryColor',
             N'#667eea',
             N'LandingPage', N'Primary theme color',
             0, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),

            (N'LandingPage.Theme.SecondaryColor',
             N'#764ba2',
             N'LandingPage', N'Secondary theme color',
             0, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),

            (N'LandingPage.Theme.AccentColor',
             N'#f093fb',
             N'LandingPage', N'Accent theme color',
             0, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),

            -- Company Info
            (N'LandingPage.Company.Name',
             N'Marketing Platform',
             N'LandingPage', N'Company name displayed on landing page',
             0, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),

            (N'LandingPage.Company.Logo',
             N'/images/logo.png',
             N'LandingPage', N'Company logo URL',
             0, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),

            (N'LandingPage.Company.Tagline',
             N'SMS, MMS & Email Marketing Platform',
             N'LandingPage', N'Company tagline',
             0, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),

            -- Statistics Section
            (N'LandingPage.Stats.MessagesSent',
             N'10M+',
             N'LandingPage', N'Messages sent statistic',
             0, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),

            (N'LandingPage.Stats.DeliveryRate',
             N'98%',
             N'LandingPage', N'Delivery rate statistic',
             0, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),

            (N'LandingPage.Stats.ActiveUsers',
             N'5K+',
             N'LandingPage', N'Active users statistic',
             0, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),

            (N'LandingPage.Stats.Support',
             N'24/7',
             N'LandingPage', N'Support availability statistic',
             0, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),

            -- Footer Settings
            (N'LandingPage.Footer.CopyrightText',
             N'© 2024 Marketing Platform. All rights reserved.',
             N'LandingPage', N'Footer copyright text',
             0, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),

            (N'LandingPage.Footer.SocialLinks',
             N'[{"platform":"facebook","url":"https://facebook.com/marketingplatform","icon":"bi-facebook"},{"platform":"twitter","url":"https://twitter.com/marketingplatform","icon":"bi-twitter"},{"platform":"linkedin","url":"https://linkedin.com/company/marketingplatform","icon":"bi-linkedin"},{"platform":"instagram","url":"https://instagram.com/marketingplatform","icon":"bi-instagram"}]',
             N'LandingPage', N'Footer social media links (JSON array)',
             4, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),  -- DataType=Json(4)

            -- SEO Settings
            (N'LandingPage.SEO.Title',
             N'Marketing Platform - SMS, MMS & Email Marketing',
             N'LandingPage', N'Page title for SEO',
             0, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),

            (N'LandingPage.SEO.Description',
             N'Transform your marketing with our enterprise-grade SMS, MMS & Email platform. Powerful automation, advanced analytics, and seamless integration.',
             N'LandingPage', N'Meta description for SEO',
             0, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),

            (N'LandingPage.SEO.Keywords',
             N'SMS marketing, email marketing, MMS marketing, marketing automation, campaign management',
             N'LandingPage', N'Meta keywords for SEO',
             0, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),

            -- Features Section
            (N'LandingPage.Features.SectionTitle',
             N'Powerful Features for Modern Marketing',
             N'LandingPage', N'Features section title',
             0, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),

            (N'LandingPage.Features.SectionSubtitle',
             N'Everything you need to create, manage, and optimize your campaigns',
             N'LandingPage', N'Features section subtitle',
             0, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),

            (N'LandingPage.Features.List',
             N'[{"icon":"bi-broadcast","title":"Multi-Channel Campaigns","description":"Send SMS, MMS, and Email campaigns from one unified platform. Reach your audience on their preferred channels.","color":"primary"},{"icon":"bi-graph-up-arrow","title":"Advanced Analytics","description":"Track campaign performance in real-time with detailed analytics and reporting. Make data-driven decisions to optimize your results.","color":"success"},{"icon":"bi-clock-history","title":"Automation & Scheduling","description":"Schedule campaigns in advance and automate your marketing workflows. Save time and improve efficiency.","color":"info"},{"icon":"bi-people","title":"Contact Management","description":"Organize your contacts with dynamic groups and tags. Segment your audience for targeted messaging.","color":"warning"},{"icon":"bi-file-earmark-text","title":"Template Library","description":"Create reusable message templates with dynamic variables. Personalize content at scale.","color":"danger"},{"icon":"bi-shield-check","title":"Compliance & Security","description":"Stay compliant with GDPR, CAN-SPAM, and TCPA regulations. Enterprise-grade security for your data.","color":"secondary"}]',
             N'LandingPage', N'Features list (JSON array)',
             4, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),  -- DataType=Json(4)

            -- Pricing Section
            (N'LandingPage.Pricing.SectionTitle',
             N'Simple, Transparent Pricing',
             N'LandingPage', N'Pricing section title',
             0, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),

            (N'LandingPage.Pricing.SectionSubtitle',
             N'Choose the plan that fits your business needs',
             N'LandingPage', N'Pricing section subtitle',
             0, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),

            (N'LandingPage.Pricing.ShowYearlyToggle',
             N'true',
             N'LandingPage', N'Show monthly/yearly pricing toggle',
             2, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),  -- DataType=Boolean(2)

            -- CTA Section
            (N'LandingPage.CTA.Title',
             N'Ready to Transform Your Marketing?',
             N'LandingPage', N'Call-to-action section title',
             0, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),

            (N'LandingPage.CTA.Subtitle',
             N'Join thousands of businesses using our platform to grow their reach',
             N'LandingPage', N'Call-to-action section subtitle',
             0, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),

            (N'LandingPage.CTA.ButtonText',
             N'Start Free Trial',
             N'LandingPage', N'Call-to-action button text',
             0, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),

            (N'LandingPage.CTA.ButtonLink',
             N'/Auth/Register',
             N'LandingPage', N'Call-to-action button link',
             0, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),

            (N'LandingPage.CTA.BackgroundColor',
             N'#667eea',
             N'LandingPage', N'Call-to-action section background color',
             0, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),

            -- Contact Section
            (N'LandingPage.Contact.Email',
             N'support@marketingplatform.com',
             N'LandingPage', N'Contact email address',
             0, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),

            (N'LandingPage.Contact.Phone',
             N'+1 (555) 123-4567',
             N'LandingPage', N'Contact phone number',
             0, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),

            (N'LandingPage.Contact.Address',
             N'123 Marketing Street, San Francisco, CA 94102',
             N'LandingPage', N'Company address',
             0, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),

            -- Testimonials Section
            (N'LandingPage.Testimonials.ShowSection',
             N'true',
             N'LandingPage', N'Show testimonials section',
             2, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),  -- DataType=Boolean(2)

            (N'LandingPage.Testimonials.SectionTitle',
             N'What Our Customers Say',
             N'LandingPage', N'Testimonials section title',
             0, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0),

            (N'LandingPage.Testimonials.List',
             N'[{"name":"John Smith","company":"TechCorp Inc.","position":"Marketing Director","testimonial":"This platform has transformed how we communicate with our customers. The automation features alone have saved us countless hours.","rating":5,"image":"/images/testimonials/john.jpg"},{"name":"Sarah Johnson","company":"E-commerce Plus","position":"CEO","testimonial":"Outstanding service and support. Our email campaigns have never performed better. Highly recommended!","rating":5,"image":"/images/testimonials/sarah.jpg"},{"name":"Michael Chen","company":"Retail Solutions","position":"Operations Manager","testimonial":"The multi-channel approach is exactly what we needed. We can now reach our customers on their preferred platforms seamlessly.","rating":5,"image":"/images/testimonials/michael.jpg"}]',
             N'LandingPage', N'Testimonials list (JSON array)',
             4, 0, 0, 0, GETUTCDATE(), GETUTCDATE(), 0);  -- DataType=Json(4)

        PRINT '  Created 47 landing page settings'
    END
    ELSE
    BEGIN
        PRINT '  Landing page settings already exist - skipped'
    END

    PRINT ''

    -- ========================================================================
    -- 10. PAGE CONTENTS (Privacy Policy & Terms of Service)
    -- ========================================================================
    PRINT '--- Seeding Page Contents ---'

    -- Privacy Policy
    IF NOT EXISTS (SELECT 1 FROM [PageContents] WHERE [PageKey] = N'privacy-policy')
    BEGIN
        INSERT INTO [PageContents] ([PageKey], [Title], [MetaDescription], [Content], [IsPublished], [CreatedAt], [UpdatedAt], [IsDeleted])
        VALUES (
            N'privacy-policy',
            N'Privacy Policy',
            N'Learn how we collect, use, and protect your personal information.',
            N'
<h2>1. Information We Collect</h2>
<p>We collect information that you provide directly to us, including:</p>
<ul>
    <li>Name and contact information (email address, phone number)</li>
    <li>Account credentials</li>
    <li>Payment information</li>
    <li>Communication preferences</li>
    <li>Campaign and marketing data</li>
</ul>

<h2>2. How We Use Your Information</h2>
<p>We use the information we collect to:</p>
<ul>
    <li>Provide, maintain, and improve our services</li>
    <li>Process transactions and send related information</li>
    <li>Send technical notices, updates, security alerts, and support messages</li>
    <li>Respond to your comments, questions, and customer service requests</li>
    <li>Monitor and analyze trends, usage, and activities</li>
</ul>

<h2>3. Data Security</h2>
<p>We implement appropriate technical and organizational measures to protect your personal data against unauthorized or unlawful processing, accidental loss, destruction, or damage. This includes encryption of sensitive data, regular security assessments, and access controls.</p>

<h2>4. Data Retention</h2>
<p>We retain your personal data for as long as necessary to provide our services, comply with legal obligations, resolve disputes, and enforce our agreements.</p>

<h2>5. Your Rights</h2>
<p>You have the right to:</p>
<ul>
    <li>Access your personal data</li>
    <li>Correct inaccurate data</li>
    <li>Request deletion of your data</li>
    <li>Object to processing of your data</li>
    <li>Request data portability</li>
    <li>Withdraw consent at any time</li>
</ul>

<h2>6. Contact Us</h2>
<p>If you have any questions about this Privacy Policy or our data practices, please contact us at privacy@marketingplatform.com</p>

<p><em>Last updated: January 2024</em></p>
',
            1, GETUTCDATE(), GETUTCDATE(), 0
        );
        PRINT '  Created Privacy Policy page content'
    END
    ELSE
    BEGIN
        PRINT '  Privacy Policy already exists - skipped'
    END

    -- Terms of Service
    IF NOT EXISTS (SELECT 1 FROM [PageContents] WHERE [PageKey] = N'terms-of-service')
    BEGIN
        INSERT INTO [PageContents] ([PageKey], [Title], [MetaDescription], [Content], [IsPublished], [CreatedAt], [UpdatedAt], [IsDeleted])
        VALUES (
            N'terms-of-service',
            N'Terms of Service',
            N'Read our terms of service and user agreement.',
            N'
<h2>1. Acceptance of Terms</h2>
<p>By accessing and using Marketing Platform ("the Service"), you accept and agree to be bound by the terms and provisions of this agreement. If you do not agree to these terms, please do not use the Service.</p>

<h2>2. Use License</h2>
<p>Permission is granted to access and use the Service for legitimate business purposes. This license shall automatically terminate if you violate any of these restrictions.</p>

<h2>3. Account Terms</h2>
<p>When you create an account with us, you must provide accurate and complete information. You are responsible for:</p>
<ul>
    <li>Maintaining the security of your account and password</li>
    <li>All activities that occur under your account</li>
    <li>Immediately notifying us of any unauthorized use</li>
    <li>Ensuring your use complies with all applicable laws</li>
</ul>

<h2>4. Service Availability</h2>
<p>We strive to provide a reliable service, but we do not guarantee that:</p>
<ul>
    <li>The Service will be uninterrupted, timely, secure, or error-free</li>
    <li>Any errors or defects will be corrected</li>
    <li>The Service is free of viruses or other harmful components</li>
</ul>

<h2>5. Prohibited Uses</h2>
<p>You may not use our Service:</p>
<ul>
    <li>For any unlawful purpose or to violate any laws</li>
    <li>To send spam, unsolicited messages, or illegal content</li>
    <li>To transmit malware or other harmful code</li>
    <li>To interfere with or disrupt the Service or servers</li>
    <li>To impersonate any person or entity</li>
</ul>

<h2>6. Intellectual Property</h2>
<p>The Service and its original content, features, and functionality are owned by Marketing Platform and are protected by international copyright, trademark, and other intellectual property laws.</p>

<h2>7. Termination</h2>
<p>We may terminate or suspend your account and access to the Service immediately, without prior notice or liability, for any reason, including if you breach these Terms.</p>

<h2>8. Limitation of Liability</h2>
<p>In no event shall Marketing Platform be liable for any indirect, incidental, special, consequential, or punitive damages resulting from your use of or inability to use the Service.</p>

<h2>9. Changes to Terms</h2>
<p>We reserve the right to modify or replace these Terms at any time. If a revision is material, we will provide at least 30 days'' notice prior to any new terms taking effect.</p>

<h2>10. Contact Information</h2>
<p>If you have any questions about these Terms, please contact us at legal@marketingplatform.com</p>

<p><em>Last updated: January 2024</em></p>
',
            1, GETUTCDATE(), GETUTCDATE(), 0
        );
        PRINT '  Created Terms of Service page content'
    END
    ELSE
    BEGIN
        PRINT '  Terms of Service already exists - skipped'
    END

    PRINT ''
    PRINT '=== MarketingPlatform Seed Data Complete ==='
    PRINT ''
    PRINT 'NOTE: AspNetUsers were NOT seeded because password hashing'
    PRINT '      cannot be performed in raw SQL. Use the .NET application'
    PRINT '      or MarketingPlatform.Seeder project to seed users.'

    COMMIT TRANSACTION;
    PRINT ''
    PRINT 'Transaction COMMITTED successfully.'

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;

    PRINT ''
    PRINT '!!! ERROR - Transaction ROLLED BACK !!!'
    PRINT 'Error Number:  ' + CAST(ERROR_NUMBER() AS NVARCHAR(10));
    PRINT 'Error Message: ' + ERROR_MESSAGE();
    PRINT 'Error Line:    ' + CAST(ERROR_LINE() AS NVARCHAR(10));

    THROW;
END CATCH

SET NOCOUNT OFF;
