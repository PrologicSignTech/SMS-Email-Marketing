-- =====================================================
-- Landing Stats Table and Seed Data
-- Run this script to add the LandingStats table
-- =====================================================

-- Create LandingStats table if not exists
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LandingStats')
BEGIN
    CREATE TABLE [LandingStats] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Value] NVARCHAR(100) NOT NULL,
        [Label] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [IconClass] NVARCHAR(100) NOT NULL DEFAULT 'bi-graph-up',
        [ColorClass] NVARCHAR(50) NOT NULL DEFAULT 'primary',
        [CounterTarget] BIGINT NULL,
        [CounterSuffix] NVARCHAR(20) NULL,
        [CounterPrefix] NVARCHAR(20) NULL,
        [DisplayOrder] INT NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [ShowOnLanding] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0
    );

    PRINT 'LandingStats table created successfully.';
END
ELSE
BEGIN
    PRINT 'LandingStats table already exists.';
END
GO

-- Seed initial stats data
IF NOT EXISTS (SELECT TOP 1 1 FROM [LandingStats])
BEGIN
    INSERT INTO [LandingStats] ([Value], [Label], [Description], [IconClass], [ColorClass], [CounterTarget], [CounterSuffix], [CounterPrefix], [DisplayOrder], [IsActive], [ShowOnLanding])
    VALUES
    (
        '10M+',
        'Messages Delivered',
        'Total messages successfully delivered through our platform',
        'bi-envelope-paper',
        'primary',
        10000000,
        '+',
        NULL,
        1,
        1,
        1
    ),
    (
        '98%',
        'Delivery Success Rate',
        'Industry-leading delivery success rate across all channels',
        'bi-check-circle',
        'success',
        98,
        '%',
        NULL,
        2,
        1,
        1
    ),
    (
        '5,000+',
        'Happy Customers',
        'Businesses trust our platform for their marketing needs',
        'bi-people',
        'info',
        5000,
        '+',
        NULL,
        3,
        1,
        1
    ),
    (
        '24/7',
        'Customer Support',
        'Round-the-clock support to help you succeed',
        'bi-headset',
        'warning',
        24,
        '/7',
        NULL,
        4,
        1,
        1
    );

    PRINT 'Seed data inserted into LandingStats table.';
END
ELSE
BEGIN
    PRINT 'LandingStats table already has data.';
END
GO

PRINT 'Landing Stats setup complete!';
GO
