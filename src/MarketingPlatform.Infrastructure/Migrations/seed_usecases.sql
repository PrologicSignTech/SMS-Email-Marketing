CREATE TABLE  [dbo].[UseCases]
(
 Id INT IDENTITY(1,1) PRIMARY KEY,

    Title NVARCHAR(MAX) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,

    IconClass NVARCHAR(MAX) NOT NULL 
        DEFAULT 'bi-lightbulb',

    Industry NVARCHAR(MAX) NULL,

    ImageUrl NVARCHAR(MAX) NULL,

    ResultsText NVARCHAR(MAX) NULL,

    ColorClass NVARCHAR(MAX) NOT NULL 
        DEFAULT 'primary',

    DisplayOrder INT NOT NULL,

    IsActive BIT NOT NULL 
        DEFAULT 1,

    IsDeleted BIT NOT NULL,

    CreatedAt DATETIME2 NOT NULL 
        DEFAULT SYSUTCDATETIME(),

    UpdatedAt DATETIME2 NULL 
        DEFAULT SYSUTCDATETIME()
);

-- Check if UseCases table has data, if empty then seed
IF NOT EXISTS (SELECT 1 FROM [dbo].[UseCases] WHERE IsActive = 1)
BEGIN
    INSERT INTO [dbo].[UseCases] 
        ([Title], [Description], [IconClass], [Industry], [ImageUrl], [ResultsText], [ColorClass], [DisplayOrder], [IsActive], [IsDeleted], [CreatedAt], [UpdatedAt])
    VALUES
        -- E-commerce
        (
            'Boost Online Sales with Targeted Campaigns',
            'Cart Abandonment Recovery: Send automated SMS reminders to recover lost sales
Order Updates: Keep customers informed with delivery notifications
Flash Sale Alerts: Drive urgency with time-sensitive promotions
Product Recommendations: Personalized offers based on purchase history',
            'bi-cart',
            'E-Commerce',
            '/images/use-cases/ecommerce.svg',
            'E-commerce store increased revenue by 45% using cart abandonment campaigns',
            'primary',
            1,
            1,
            0,
            GETUTCDATE(),
            GETUTCDATE()
        ),
        -- Healthcare
        (
            'Improve Patient Engagement & Care',
            'Appointment Reminders: Reduce no-shows with automated SMS reminders
Prescription Notifications: Alert patients when prescriptions are ready
Health Tips: Send personalized wellness information
Test Results: Securely notify patients of available results',
            'bi-hospital',
            'Healthcare',
            '/images/use-cases/healthcare.svg',
            'Medical practice reduced no-shows by 60% with appointment reminders',
            'success',
            2,
            1,
            0,
            GETUTCDATE(),
            GETUTCDATE()
        ),
        -- Real Estate
        (
            'Close More Deals Faster',
            'New Listing Alerts: Instantly notify buyers of properties matching their criteria
Open House Reminders: Maximize attendance with timely notifications
Price Drop Alerts: Re-engage interested buyers when prices change
Market Updates: Keep clients informed with market insights',
            'bi-house',
            'Real Estate',
            '/images/use-cases/realestate.svg',
            'Real estate agency increased showings by 35% with listing alerts',
            'info',
            3,
            1,
            0,
            GETUTCDATE(),
            GETUTCDATE()
        ),
        -- Retail
        (
            'Drive Foot Traffic & Repeat Business',
            'Grand Opening Announcements: Generate buzz for new store openings
Exclusive Offers: Reward loyal customers with VIP promotions
Inventory Updates: Alert customers when popular items are back in stock
Event Invitations: Fill your store with in-store event announcements',
            'bi-shop',
            'Retail',
            '/images/use-cases/retail.svg',
            'Retail chain increased foot traffic by 50% with targeted promotions',
            'warning',
            4,
            1,
            0,
            GETUTCDATE(),
            GETUTCDATE()
        );
    
    PRINT '4 use cases seeded successfully.';
END
ELSE
BEGIN
    PRINT 'UseCases table already contains data. Skipping seed.';
END
GO
