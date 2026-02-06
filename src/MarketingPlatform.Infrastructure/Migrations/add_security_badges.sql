-- Create SecurityBadges table for landing page security and compliance badges
-- Run this script to add the SecurityBadges table and seed initial data

-- Check if table exists, create if not
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SecurityBadges]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SecurityBadges] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Title] NVARCHAR(200) NOT NULL,
        [Subtitle] NVARCHAR(200) NOT NULL,
        [IconUrl] NVARCHAR(500) NOT NULL,
        [Description] NVARCHAR(2000) NOT NULL,
        [DisplayOrder] INT NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [ShowOnLanding] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL
    );
    
    PRINT 'SecurityBadges table created successfully.';
END
ELSE
BEGIN
    PRINT 'SecurityBadges table already exists.';
END
GO

-- Seed initial security badges (only if table is empty)
IF NOT EXISTS (SELECT 1 FROM [dbo].[SecurityBadges])
BEGIN
    INSERT INTO [dbo].[SecurityBadges] 
        ([Title], [Subtitle], [IconUrl], [Description], [DisplayOrder], [IsActive], [ShowOnLanding], [CreatedAt])
    VALUES
        -- GDPR
        (
            'GDPR',
            'Compliant',
            '/images/badges/gdpr.svg',
            'Fully compliant with the General Data Protection Regulation (GDPR), ensuring the highest standards of data privacy and protection for all European users.',
            1,
            1,
            1,
            GETUTCDATE()
        ),
        -- ISO 27001
        (
            'ISO 27001',
            'Certified',
            '/images/badges/iso-27001.svg',
            'ISO 27001 certified for Information Security Management Systems (ISMS), demonstrating our commitment to protecting your data with international best practices.',
            2,
            1,
            1,
            GETUTCDATE()
        ),
        -- SOC 2 Type II
        (
            'SOC 2',
            'Type II',
            '/images/badges/soc2.svg',
            'SOC 2 Type II compliant, providing independent verification of our security controls, availability, processing integrity, confidentiality, and privacy.',
            3,
            1,
            1,
            GETUTCDATE()
        ),
        -- HIPAA
        (
            'HIPAA',
            'Compliant',
            '/images/badges/hipaa.svg',
            'HIPAA compliant for healthcare data, ensuring Protected Health Information (PHI) is handled with the utmost security and confidentiality.',
            4,
            1,
            1,
            GETUTCDATE()
        ),
        -- 256-bit SSL Encryption
        (
            '256-bit SSL',
            'Encrypted',
            '/images/badges/ssl.svg',
            'All data transmitted through our platform is protected with 256-bit SSL encryption, the industry standard for secure communications.',
            5,
            1,
            1,
            GETUTCDATE()
        );
    
    PRINT '5 security badges seeded successfully.';
END
ELSE
BEGIN
    PRINT 'SecurityBadges table already contains data. Skipping seed.';
END
GO
