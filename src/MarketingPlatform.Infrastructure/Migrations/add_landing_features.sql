-- Create LandingFeatures table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LandingFeatures]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[LandingFeatures](
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Title] NVARCHAR(200) NOT NULL,
        [ShortDescription] NVARCHAR(500) NOT NULL,
        [DetailedDescription] NVARCHAR(2000) NOT NULL,
        [IconClass] NVARCHAR(100) NOT NULL,
        [ColorClass] NVARCHAR(50) NOT NULL,
        [DisplayOrder] INT NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [ShowOnLanding] BIT NOT NULL DEFAULT 1,
        [StatTitle1] NVARCHAR(100) NULL,
        [StatValue1] NVARCHAR(100) NULL,
        [StatTitle2] NVARCHAR(100) NULL,
        [StatValue2] NVARCHAR(100) NULL,
        [StatTitle3] NVARCHAR(100) NULL,
        [StatValue3] NVARCHAR(100) NULL,
        [CallToActionText] NVARCHAR(100) NULL,
        [CallToActionUrl] NVARCHAR(500) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0
    );
    PRINT 'LandingFeatures table created successfully!';
END
ELSE
BEGIN
    PRINT 'LandingFeatures table already exists.';
END
GO

-- Seed landing features
IF NOT EXISTS (SELECT * FROM LandingFeatures WHERE Title = 'Multi-Channel Campaigns')
BEGIN
    INSERT INTO LandingFeatures
    (Title, ShortDescription, DetailedDescription, IconClass, ColorClass, DisplayOrder,
     StatTitle1, StatValue1, StatTitle2, StatValue2, StatTitle3, StatValue3,
     CallToActionText, CallToActionUrl, IsActive, ShowOnLanding, CreatedAt)
    VALUES
    (
        'Multi-Channel Campaigns',
        'Send SMS, MMS, and Email campaigns from one unified platform. Reach your audience on their preferred channels with seamless integration.',
        'Our multi-channel campaign system allows you to orchestrate sophisticated marketing campaigns across SMS, MMS, and Email simultaneously. With intelligent delivery optimization, your messages reach customers at the perfect time on their preferred channel.',
        'bi-broadcast',
        'primary',
        1,
        'Channels Supported',
        '3+',
        'Delivery Rate',
        '99.9%',
        'Integration Time',
        '< 5 min',
        'Start Multi-Channel Campaign',
        '/Campaigns/Create',
        1,
        1,
        GETUTCDATE()
    ),
    (
        'Advanced Analytics',
        'Track campaign performance in real-time with detailed analytics and reporting. Make data-driven decisions to optimize your results and ROI.',
        'Get comprehensive insights into every aspect of your campaigns with real-time dashboards, conversion tracking, and detailed reporting. Our analytics platform helps you understand customer behavior and optimize for maximum ROI.',
        'bi-graph-up-arrow',
        'success',
        2,
        'Metrics Tracked',
        '50+',
        'Real-Time Updates',
        'Instant',
        'Export Formats',
        '5+',
        'View Analytics Demo',
        '/Analytics/Index',
        1,
        1,
        GETUTCDATE()
    ),
    (
        'Automation & Scheduling',
        'Schedule campaigns in advance and automate your marketing workflows. Save time, improve efficiency, and never miss an opportunity.',
        'Powerful automation tools that let you create sophisticated workflows, schedule campaigns weeks in advance, and trigger messages based on customer actions. Set it and forget it while our system handles the rest.',
        'bi-clock-history',
        'info',
        3,
        'Time Saved',
        '80%',
        'Workflows Available',
        'Unlimited',
        'Scheduling Precision',
        '1 min',
        'Explore Automation',
        '/Workflows/Index',
        1,
        1,
        GETUTCDATE()
    ),
    (
        'Smart Contact Management',
        'Organize your contacts with dynamic groups and tags. Segment your audience intelligently for highly targeted messaging.',
        'Advanced contact management with smart segmentation, dynamic groups, custom fields, and powerful filtering. Import from anywhere, segment by behavior, and create hyper-targeted campaigns that convert.',
        'bi-people',
        'warning',
        4,
        'Contacts Supported',
        'Unlimited',
        'Segmentation Rules',
        'Custom',
        'Import Speed',
        'Instant',
        'Manage Contacts',
        '/Contacts/Index',
        1,
        1,
        GETUTCDATE()
    ),
    (
        'Template Library',
        'Access professionally designed templates for SMS, MMS, and Email. Customize to match your brand and messaging goals.',
        'Choose from hundreds of proven, conversion-optimized templates or create your own. Our drag-and-drop editor makes customization easy, and dynamic personalization ensures every message feels personal.',
        'bi-file-earmark-text',
        'danger',
        5,
        'Templates Available',
        '500+',
        'Customization',
        'Full',
        'Preview Devices',
        'All',
        'Browse Templates',
        '/Templates/Index',
        1,
        1,
        GETUTCDATE()
    ),
    (
        'API & Integrations',
        'Powerful REST API and pre-built integrations with popular platforms. Connect seamlessly with your existing tools.',
        'Enterprise-grade REST API with comprehensive documentation, webhooks for real-time events, and pre-built integrations with Zapier, Salesforce, HubSpot, and more. Build custom integrations with ease.',
        'bi-plugin',
        'secondary',
        6,
        'API Endpoints',
        '100+',
        'Integrations',
        '50+',
        'Uptime SLA',
        '99.95%',
        'View API Docs',
        '/Settings/Integrations',
        1,
        1,
        GETUTCDATE()
    );

    PRINT 'Landing features seeded successfully!';
END
ELSE
BEGIN
    PRINT 'Landing features already exist.';
END
GO

PRINT 'LandingFeatures table setup completed!';
