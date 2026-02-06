-- Add Media and Contact Fields to LandingFeatures Table
-- Run this after add_landing_features.sql

USE MarketingPlatformDb;
GO

PRINT 'Adding media and contact fields to LandingFeatures table...';

-- Check if columns don't exist before adding
 
    ALTER TABLE [LandingFeatures]
    ADD [HeaderImageUrl] NVARCHAR(500) NULL;
    PRINT '✓ Added HeaderImageUrl column';
 

 
    ALTER TABLE [LandingFeatures]
    ADD [VideoUrl] NVARCHAR(500) NULL;
    PRINT '✓ Added VideoUrl column';
 
    ALTER TABLE [LandingFeatures]
    ADD [GalleryImages] NVARCHAR(MAX) NULL;
    PRINT '✓ Added GalleryImages column';
 
    ALTER TABLE [LandingFeatures]
    ADD [ContactName] NVARCHAR(200) NULL;
    PRINT '✓ Added ContactName column';
 
    ALTER TABLE [LandingFeatures]
    ADD [ContactEmail] NVARCHAR(200) NULL;
    PRINT '✓ Added ContactEmail column';
 
    ALTER TABLE [LandingFeatures]
    ADD [ContactPhone] NVARCHAR(50) NULL;
    PRINT '✓ Added ContactPhone column';
 
    ALTER TABLE [LandingFeatures]
    ADD [ContactMessage] NVARCHAR(1000) NULL;
    PRINT '✓ Added ContactMessage column';
 
PRINT '';
PRINT 'Updating existing features with sample media and contact data...';

-- Update Multi-Channel Campaigns with media
UPDATE [LandingFeatures]
SET
    [HeaderImageUrl] = '/images/features/multi-channel-header.jpg',
    [VideoUrl] = 'https://www.youtube.com/embed/dQw4w9WgXcQ',
    [GalleryImages] = '[""/images/features/multi-channel-1.jpg"", ""/images/features/multi-channel-2.jpg"", ""/images/features/multi-channel-3.jpg""]',
    [ContactName] = 'Sales Team',
    [ContactEmail] = 'sales@marketingplatform.com',
    [ContactPhone] = '+1 (555) 123-4567',
    [ContactMessage] = 'Interested in learning more about multi-channel campaigns? Our team is here to help!'
WHERE [Title] = 'Multi-Channel Campaigns';

-- Update Advanced Analytics with media
UPDATE [LandingFeatures]
SET
    [HeaderImageUrl] = '/images/features/analytics-header.jpg',
    [VideoUrl] = 'https://www.youtube.com/embed/dQw4w9WgXcQ',
    [GalleryImages] = '[""/images/features/analytics-1.jpg"", ""/images/features/analytics-2.jpg""]',
    [ContactName] = 'Analytics Team',
    [ContactEmail] = 'analytics@marketingplatform.com',
    [ContactPhone] = '+1 (555) 234-5678',
    [ContactMessage] = 'Want to see how our analytics can transform your data? Get in touch!'
WHERE [Title] = 'Advanced Analytics';

-- Update Automation & Scheduling with media
UPDATE [LandingFeatures]
SET
    [HeaderImageUrl] = '/images/features/automation-header.jpg',
    [VideoUrl] = 'https://www.youtube.com/embed/dQw4w9WgXcQ',
    [GalleryImages] = '[""/images/features/automation-1.jpg"", ""/images/features/automation-2.jpg"", ""/images/features/automation-3.jpg"", ""/images/features/automation-4.jpg""]',
    [ContactName] = 'Automation Experts',
    [ContactEmail] = 'automation@marketingplatform.com',
    [ContactPhone] = '+1 (555) 345-6789',
    [ContactMessage] = 'Ready to automate your marketing? Let us show you how!'
WHERE [Title] = 'Automation & Scheduling';

-- Update Smart Contact Management with media
UPDATE [LandingFeatures]
SET
    [HeaderImageUrl] = '/images/features/contacts-header.jpg',
    [GalleryImages] = '[""/images/features/contacts-1.jpg"", ""/images/features/contacts-2.jpg""]',
    [ContactName] = 'Support Team',
    [ContactEmail] = 'support@marketingplatform.com',
    [ContactPhone] = '+1 (555) 456-7890',
    [ContactMessage] = 'Need help managing your contacts effectively? We are here for you!'
WHERE [Title] = 'Smart Contact Management';

-- Update Template Library with media
UPDATE [LandingFeatures]
SET
    [HeaderImageUrl] = '/images/features/templates-header.jpg',
    [GalleryImages] = '[""/images/features/template-1.jpg"", ""/images/features/template-2.jpg"", ""/images/features/template-3.jpg""]',
    [ContactName] = 'Design Team',
    [ContactEmail] = 'design@marketingplatform.com',
    [ContactPhone] = '+1 (555) 567-8901',
    [ContactMessage] = 'Looking for custom templates? Our design team can help!'
WHERE [Title] = 'Template Library';

-- Update API & Integrations with media
UPDATE [LandingFeatures]
SET
    [HeaderImageUrl] = '/images/features/api-header.jpg',
    [VideoUrl] = 'https://www.youtube.com/embed/dQw4w9WgXcQ',
    [GalleryImages] = '[""/images/features/api-1.jpg"", ""/images/features/api-2.jpg""]',
    [ContactName] = 'Developer Relations',
    [ContactEmail] = 'developers@marketingplatform.com',
    [ContactPhone] = '+1 (555) 678-9012',
    [ContactMessage] = 'Need API documentation or integration support? Contact our dev team!'
WHERE [Title] = 'API & Integrations';

PRINT '✓ Updated all features with media and contact information';
PRINT '';
PRINT '========================================';
PRINT 'Media fields added successfully!';
PRINT '========================================';
PRINT '';
PRINT 'Next steps:';
PRINT '1. Run: dotnet ef migrations add AddLandingFeatureMedia --project src/MarketingPlatform.Infrastructure';
PRINT '2. Restart the API and Web applications';
PRINT '3. Test feature detail pages';
PRINT '';
