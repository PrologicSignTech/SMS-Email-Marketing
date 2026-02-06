-- Add new columns to SubscriptionPlans table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SubscriptionPlans]') AND name = 'IsMostPopular')
BEGIN
    ALTER TABLE [SubscriptionPlans] ADD [IsMostPopular] bit NOT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SubscriptionPlans]') AND name = 'PlanCategory')
BEGIN
    ALTER TABLE [SubscriptionPlans] ADD [PlanCategory] nvarchar(50) NOT NULL DEFAULT 'Standard';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SubscriptionPlans]') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE [SubscriptionPlans] ADD [UpdatedAt] datetime2 NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SubscriptionPlans]') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE [SubscriptionPlans] ADD [IsDeleted] bit NOT NULL DEFAULT 0;
END
GO

-- Create Features table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Features')
BEGIN
    CREATE TABLE [Features] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(500) NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        [DisplayOrder] int NOT NULL DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        CONSTRAINT [PK_Features] PRIMARY KEY ([Id])
    );
    CREATE UNIQUE INDEX [IX_Features_Name] ON [Features] ([Name]);
END
GO

-- Create PlanFeatureMappings table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PlanFeatureMappings')
BEGIN
    CREATE TABLE [PlanFeatureMappings] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [SubscriptionPlanId] int NOT NULL,
        [FeatureId] int NOT NULL,
        [FeatureValue] nvarchar(100) NULL,
        [IsIncluded] bit NOT NULL DEFAULT 1,
        [DisplayOrder] int NOT NULL DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_PlanFeatureMappings] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PlanFeatureMappings_SubscriptionPlans] FOREIGN KEY ([SubscriptionPlanId]) REFERENCES [SubscriptionPlans] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PlanFeatureMappings_Features] FOREIGN KEY ([FeatureId]) REFERENCES [Features] ([Id]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_PlanFeatureMappings_SubscriptionPlanId] ON [PlanFeatureMappings] ([SubscriptionPlanId]);
    CREATE INDEX [IX_PlanFeatureMappings_FeatureId] ON [PlanFeatureMappings] ([FeatureId]);
    CREATE UNIQUE INDEX [IX_PlanFeatureMappings_PlanId_FeatureId] ON [PlanFeatureMappings] ([SubscriptionPlanId], [FeatureId]);
END
GO

-- Insert migration record if not exists
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260120052557_AddFeatureManagementAndPlanImprovements')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20260120052557_AddFeatureManagementAndPlanImprovements', '8.0.0');
END
GO

PRINT 'Migration completed successfully!';
