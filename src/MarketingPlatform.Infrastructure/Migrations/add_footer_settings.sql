-- =============================================
-- Script: add_footer_settings.sql
-- Description: Creates FooterSettings table and seeds default data
-- Run this after applying migrations
-- =============================================

-- Check if table exists
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'FooterSettings')
BEGIN
    CREATE TABLE [FooterSettings] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [CompanyName] NVARCHAR(200) NOT NULL DEFAULT 'Marketing Platform',
        [CompanyDescription] NVARCHAR(1000) NOT NULL DEFAULT '',
        [AddressLine1] NVARCHAR(200) NOT NULL DEFAULT '',
        [AddressLine2] NVARCHAR(200) NOT NULL DEFAULT '',
        [Phone] NVARCHAR(50) NOT NULL DEFAULT '',
        [Email] NVARCHAR(100) NOT NULL DEFAULT '',
        [BusinessHours] NVARCHAR(200) NOT NULL DEFAULT '',
        [MapEmbedUrl] NVARCHAR(1000) NULL,
        [FacebookUrl] NVARCHAR(500) NULL,
        [TwitterUrl] NVARCHAR(500) NULL,
        [LinkedInUrl] NVARCHAR(500) NULL,
        [InstagramUrl] NVARCHAR(500) NULL,
        [YouTubeUrl] NVARCHAR(500) NULL,
        [CopyrightText] NVARCHAR(500) NOT NULL DEFAULT '',
        [ShowNewsletter] BIT NOT NULL DEFAULT 1,
        [NewsletterTitle] NVARCHAR(200) NOT NULL DEFAULT 'Subscribe to Our Newsletter',
        [NewsletterDescription] NVARCHAR(500) NOT NULL DEFAULT '',
        [ShowMap] BIT NOT NULL DEFAULT 1,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0
    );
    PRINT 'FooterSettings table created successfully.';
END
ELSE
BEGIN
    PRINT 'FooterSettings table already exists.';
END
GO

-- Seed default footer settings if table is empty
IF NOT EXISTS (SELECT 1 FROM FooterSettings WHERE IsDeleted = 0)
BEGIN
    INSERT INTO [FooterSettings] (
        [CompanyName],
        [CompanyDescription],
        [AddressLine1],
        [AddressLine2],
        [Phone],
        [Email],
        [BusinessHours],
        [MapEmbedUrl],
        [FacebookUrl],
        [TwitterUrl],
        [LinkedInUrl],
        [InstagramUrl],
        [YouTubeUrl],
        [CopyrightText],
        [ShowNewsletter],
        [NewsletterTitle],
        [NewsletterDescription],
        [ShowMap],
        [IsActive],
        [CreatedAt]
    )
    VALUES (
        'Marketing Platform',
        'Enterprise-grade SMS, MMS & Email marketing platform. Reach your customers where they are and grow your business exponentially.',
        '123 Business Avenue, Suite 100',
        'New York, NY 10001, USA',
        '+1 (234) 567-890',
        'support@marketingplatform.com',
        'Mon - Fri: 9:00 AM - 6:00 PM EST',
        'https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d3022.9663095343008!2d-74.00425878428698!3d40.74076794379132!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x89c259bf5c1654f3%3A0xc80f9cfce5383d5d!2sGoogle!5e0!3m2!1sen!2sus!4v1234567890',
        'https://facebook.com/marketingplatform',
        'https://twitter.com/marketingplatform',
        'https://linkedin.com/company/marketingplatform',
        'https://instagram.com/marketingplatform',
        'https://youtube.com/c/marketingplatform',
        '&copy; 2024 Marketing Platform. All rights reserved.',
        1,
        'Subscribe to Our Newsletter',
        'Get the latest updates, tips and exclusive offers delivered to your inbox.',
        1,
        1,
        GETUTCDATE()
    );
    PRINT 'Default footer settings seeded successfully.';
END
ELSE
BEGIN
    PRINT 'Footer settings already exist.';
END
GO

-- Verify data
SELECT * FROM FooterSettings WHERE IsDeleted = 0;
GO
