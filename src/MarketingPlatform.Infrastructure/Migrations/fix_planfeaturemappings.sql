-- Add BaseEntity columns to PlanFeatureMappings table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[PlanFeatureMappings]') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE [PlanFeatureMappings] ADD [UpdatedAt] datetime2 NULL;
    PRINT 'Added UpdatedAt column to PlanFeatureMappings';
END
ELSE
BEGIN
    PRINT 'UpdatedAt column already exists in PlanFeatureMappings';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[PlanFeatureMappings]') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE [PlanFeatureMappings] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
    PRINT 'Added IsDeleted column to PlanFeatureMappings';
END
ELSE
BEGIN
    PRINT 'IsDeleted column already exists in PlanFeatureMappings';
END
GO

-- Also add to Features table if missing
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Features]') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE [Features] ADD [UpdatedAt] datetime2 NULL;
    PRINT 'Added UpdatedAt column to Features';
END
ELSE
BEGIN
    PRINT 'UpdatedAt column already exists in Features';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Features]') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE [Features] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
    PRINT 'Added IsDeleted column to Features';
END
ELSE
BEGIN
    PRINT 'IsDeleted column already exists in Features';
END
GO

PRINT 'All BaseEntity columns added successfully!';
