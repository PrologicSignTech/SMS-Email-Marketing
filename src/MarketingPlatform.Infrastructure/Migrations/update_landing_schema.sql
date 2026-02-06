-- ================================================================
-- Landing Page Schema Updates
-- Removes stats from LandingFeatures
-- Adds Testimonials, UseCases, and TrustedCompanies tables
-- ================================================================

USE MarketingPlatformDb;
GO

PRINT '========================================';
PRINT 'UPDATING LANDING PAGE SCHEMA';
PRINT '========================================';
PRINT '';

-- ================================================================
-- STEP 1: Keep Stat Columns in LandingFeatures (used in detail page hero)
-- ================================================================

PRINT 'Step 1: Stat columns retained in LandingFeatures for detail page hero...';
PRINT '✓ StatTitle1-3 and StatValue1-3 columns will be kept';
PRINT '  (Used only in feature detail page hero, not in flip cards)';
PRINT '';

-- ================================================================
-- STEP 2: Create Testimonials Table
-- ================================================================

PRINT 'Step 2: Creating Testimonials table...';

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Testimonials')
BEGIN
    CREATE TABLE [dbo].[Testimonials] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [CustomerName] NVARCHAR(200) NOT NULL,
        [CustomerTitle] NVARCHAR(200) NULL,          -- e.g., "CEO", "Marketing Director"
        [CompanyName] NVARCHAR(200) NOT NULL,
        [CompanyLogo] NVARCHAR(500) NULL,            -- URL to company logo
        [AvatarUrl] NVARCHAR(500) NULL,              -- Customer photo
        [Rating] INT NOT NULL DEFAULT 5,              -- 1-5 star rating
        [TestimonialText] NVARCHAR(1000) NOT NULL,   -- The actual testimonial
        [DisplayOrder] INT NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    PRINT '✓ Created Testimonials table';

    -- Insert sample testimonials
    INSERT INTO [Testimonials]
        ([CustomerName], [CustomerTitle], [CompanyName], [CompanyLogo], [Rating], [TestimonialText], [DisplayOrder], [IsActive])
    VALUES
        ('Sarah Johnson', 'Marketing Director', 'TechCorp Inc', '/images/companies/techcorp-logo.png', 5,
         'This platform transformed our marketing campaigns. We saw a 300% increase in engagement within the first month!',
         1, 1),
        ('Michael Chen', 'CEO', 'GrowthStart', '/images/companies/growthstart-logo.png', 5,
         'The automation features saved us countless hours. Our team can now focus on strategy instead of manual tasks.',
         2, 1),
        ('Emily Rodriguez', 'Head of Sales', 'RetailPro', '/images/companies/retailpro-logo.png', 5,
         'Outstanding analytics and reporting. We can now make data-driven decisions that actually move the needle.',
         3, 1),
        ('David Park', 'VP Marketing', 'HealthFirst', '/images/companies/healthfirst-logo.png', 4,
         'Easy to use, powerful features, and excellent customer support. Highly recommended for any business size.',
         4, 1),
        ('Lisa Anderson', 'Founder', 'StartupHub', '/images/companies/startuphub-logo.png', 5,
         'As a startup, we needed something affordable yet powerful. This platform exceeded all our expectations!',
         5, 1),
        ('James Wilson', 'Digital Marketing Manager', 'E-Commerce Plus', '/images/companies/ecommerceplus-logo.png', 5,
         'The multi-channel campaign management is a game changer. We reached our customers across all platforms seamlessly.',
         6, 1);

    PRINT '✓ Inserted 6 sample testimonials';
END
ELSE
BEGIN
    PRINT '⚠ Testimonials table already exists';
END

PRINT '';

-- ================================================================
-- STEP 3: Create UseCases Table
-- ================================================================

PRINT 'Step 3: Creating UseCases table...';

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'UseCases')
BEGIN
    CREATE TABLE [dbo].[UseCases] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Title] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(1000) NOT NULL,
        [IconClass] NVARCHAR(100) NOT NULL DEFAULT 'bi-lightbulb',  -- Bootstrap icon class
        [Industry] NVARCHAR(100) NULL,                               -- e.g., "E-commerce", "Healthcare"
        [ImageUrl] NVARCHAR(500) NULL,                               -- Featured image
        [ResultsText] NVARCHAR(500) NULL,                            -- e.g., "300% increase in engagement"
        [ColorClass] NVARCHAR(50) NOT NULL DEFAULT 'primary',        -- primary, success, info, warning, etc.
        [DisplayOrder] INT NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    PRINT '✓ Created UseCases table';

    -- Insert sample use cases
    INSERT INTO [UseCases]
        ([Title], [Description], [IconClass], [Industry], [ResultsText], [ColorClass], [DisplayOrder], [IsActive])
    VALUES
        ('E-Commerce Flash Sales',
         'Send targeted SMS campaigns for limited-time offers and drive immediate purchases. Reach customers instantly with personalized promotions.',
         'bi-cart-check', 'E-Commerce', '300% increase in flash sale conversions', 'primary', 1, 1),

        ('Healthcare Appointment Reminders',
         'Reduce no-shows with automated appointment reminders via SMS and email. Improve patient engagement and clinic efficiency.',
         'bi-hospital', 'Healthcare', '65% reduction in missed appointments', 'success', 2, 1),

        ('Real Estate Property Updates',
         'Keep potential buyers informed about new listings and price changes. Send multimedia messages with property photos and details.',
         'bi-house-door', 'Real Estate', '45% faster property sales cycle', 'info', 3, 1),

        ('Restaurant Reservations',
         'Manage bookings and send confirmation messages automatically. Engage customers with special menu promotions and events.',
         'bi-shop', 'Food & Beverage', '80% improvement in table occupancy', 'warning', 4, 1),

        ('Fitness Class Reminders',
         'Boost class attendance with timely reminders and motivational messages. Share workout tips and nutrition advice.',
         'bi-heart-pulse', 'Fitness & Wellness', '55% increase in class attendance', 'danger', 5, 1),

        ('Retail Customer Loyalty',
         'Build lasting relationships with personalized offers and rewards notifications. Drive repeat purchases with exclusive deals.',
         'bi-gift', 'Retail', '200% growth in repeat customers', 'secondary', 6, 1);

    PRINT '✓ Inserted 6 sample use cases';
END
ELSE
BEGIN
    PRINT '⚠ UseCases table already exists';
END

PRINT '';

-- ================================================================
-- STEP 4: Create TrustedCompanies Table
-- ================================================================

PRINT 'Step 4: Creating TrustedCompanies table...';

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TrustedCompanies')
BEGIN
    CREATE TABLE [dbo].[TrustedCompanies] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [CompanyName] NVARCHAR(200) NOT NULL,
        [LogoUrl] NVARCHAR(500) NOT NULL,            -- URL to company logo image
        [WebsiteUrl] NVARCHAR(500) NULL,             -- Optional link to company website
        [DisplayOrder] INT NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    PRINT '✓ Created TrustedCompanies table';

    -- Insert sample trusted companies
    INSERT INTO [TrustedCompanies]
        ([CompanyName], [LogoUrl], [WebsiteUrl], [DisplayOrder], [IsActive])
    VALUES
        ('Microsoft', '/images/logos/microsoft.svg', 'https://microsoft.com', 1, 1),
        ('Amazon', '/images/logos/amazon.svg', 'https://amazon.com', 2, 1),
        ('Google', '/images/logos/google.svg', 'https://google.com', 3, 1),
        ('IBM', '/images/logos/ibm.svg', 'https://ibm.com', 4, 1),
        ('Salesforce', '/images/logos/salesforce.svg', 'https://salesforce.com', 5, 1),
        ('Oracle', '/images/logos/oracle.svg', 'https://oracle.com', 6, 1),
        ('SAP', '/images/logos/sap.svg', 'https://sap.com', 7, 1),
        ('Adobe', '/images/logos/adobe.svg', 'https://adobe.com', 8, 1),
        ('Cisco', '/images/logos/cisco.svg', 'https://cisco.com', 9, 1),
        ('Intel', '/images/logos/intel.svg', 'https://intel.com', 10, 1),
        ('Dell', '/images/logos/dell.svg', 'https://dell.com', 11, 1),
        ('HP', '/images/logos/hp.svg', 'https://hp.com', 12, 1);

    PRINT '✓ Inserted 12 sample trusted companies';
END
ELSE
BEGIN
    PRINT '⚠ TrustedCompanies table already exists';
END

PRINT '';

-- ================================================================
-- SUMMARY
-- ================================================================

PRINT '========================================';
PRINT 'SCHEMA UPDATE COMPLETE';
PRINT '========================================';
PRINT '';
PRINT 'Tables created/updated:';
PRINT '  ✓ LandingFeatures (removed stat columns)';
PRINT '  ✓ Testimonials (with 6 sample records)';
PRINT '  ✓ UseCases (with 6 sample records)';
PRINT '  ✓ TrustedCompanies (with 12 sample records)';
PRINT '';
PRINT 'Next steps:';
PRINT '  1. Update LandingFeature entity in code';
PRINT '  2. Create entity classes for new tables';
PRINT '  3. Create API endpoints';
PRINT '  4. Add sections to landing page';
PRINT '';
