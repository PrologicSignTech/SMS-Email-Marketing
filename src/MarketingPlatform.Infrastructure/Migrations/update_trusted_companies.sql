-- Update/Insert Trusted Companies with actual logo paths
-- Run this script to populate the TrustedCompanies table with company logos

-- Check if TrustedCompanies table exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TrustedCompanies]') AND type in (N'U'))
BEGIN
    -- Clear existing data or update with new logo paths
    -- First, check if we have any companies
    IF NOT EXISTS (SELECT 1 FROM [dbo].[TrustedCompanies] WHERE IsActive = 1 AND NOT IsDeleted)
    BEGIN
        -- Insert new companies with logo paths
        INSERT INTO [dbo].[TrustedCompanies] 
            ([CompanyName], [LogoUrl], [WebsiteUrl], [DisplayOrder], [IsActive], [IsDeleted], [CreatedAt], [UpdatedAt])
        VALUES
            ('Slack', '/images/companies/slack.svg', 'https://slack.com', 1, 1, 0, GETUTCDATE(), GETUTCDATE()),
            ('Shopify', '/images/companies/shopify.svg', 'https://shopify.com', 2, 1, 0, GETUTCDATE(), GETUTCDATE()),
            ('Stripe', '/images/companies/stripe.svg', 'https://stripe.com', 3, 1, 0, GETUTCDATE(), GETUTCDATE()),
            ('Zoom', '/images/companies/zoom.svg', 'https://zoom.us', 4, 1, 0, GETUTCDATE(), GETUTCDATE()),
            ('Salesforce', '/images/companies/salesforce.svg', 'https://salesforce.com', 5, 1, 0, GETUTCDATE(), GETUTCDATE()),
            ('HubSpot', '/images/companies/hubspot.svg', 'https://hubspot.com', 6, 1, 0, GETUTCDATE(), GETUTCDATE()),
            ('Microsoft', '/images/companies/microsoft.svg', 'https://microsoft.com', 7, 1, 0, GETUTCDATE(), GETUTCDATE()),
            ('Google', '/images/companies/google.svg', 'https://google.com', 8, 1, 0, GETUTCDATE(), GETUTCDATE());
        
        PRINT '8 trusted companies inserted successfully.';
    END
    ELSE
    BEGIN
        -- Update existing companies with new logo paths if needed
        UPDATE [dbo].[TrustedCompanies] SET [LogoUrl] = '/images/companies/slack.svg', [UpdatedAt] = GETUTCDATE() WHERE [CompanyName] = 'Slack';
        UPDATE [dbo].[TrustedCompanies] SET [LogoUrl] = '/images/companies/shopify.svg', [UpdatedAt] = GETUTCDATE() WHERE [CompanyName] = 'Shopify';
        UPDATE [dbo].[TrustedCompanies] SET [LogoUrl] = '/images/companies/stripe.svg', [UpdatedAt] = GETUTCDATE() WHERE [CompanyName] = 'Stripe';
        UPDATE [dbo].[TrustedCompanies] SET [LogoUrl] = '/images/companies/zoom.svg', [UpdatedAt] = GETUTCDATE() WHERE [CompanyName] = 'Zoom';
        UPDATE [dbo].[TrustedCompanies] SET [LogoUrl] = '/images/companies/salesforce.svg', [UpdatedAt] = GETUTCDATE() WHERE [CompanyName] = 'Salesforce';
        UPDATE [dbo].[TrustedCompanies] SET [LogoUrl] = '/images/companies/hubspot.svg', [UpdatedAt] = GETUTCDATE() WHERE [CompanyName] = 'HubSpot';
        UPDATE [dbo].[TrustedCompanies] SET [LogoUrl] = '/images/companies/microsoft.svg', [UpdatedAt] = GETUTCDATE() WHERE [CompanyName] = 'Microsoft';
        UPDATE [dbo].[TrustedCompanies] SET [LogoUrl] = '/images/companies/google.svg', [UpdatedAt] = GETUTCDATE() WHERE [CompanyName] = 'Google';
        
        PRINT 'Trusted companies logo paths updated successfully.';
    END
END
ELSE
BEGIN
    PRINT 'TrustedCompanies table does not exist. Please run migrations first.';
END
GO
