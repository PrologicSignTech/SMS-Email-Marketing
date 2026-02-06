IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [FirstName] nvarchar(max) NULL,
    [LastName] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [LastLoginAt] datetime2 NULL,
    [IsActive] bit NOT NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [MessageProviders] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [Type] int NOT NULL,
    [ApiKey] nvarchar(max) NULL,
    [ApiSecret] nvarchar(max) NULL,
    [Configuration] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [IsPrimary] bit NOT NULL,
    [HealthStatus] int NOT NULL,
    [LastHealthCheck] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_MessageProviders] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [SubscriptionPlans] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [PriceMonthly] decimal(18,2) NOT NULL,
    [PriceYearly] decimal(18,2) NOT NULL,
    [SMSLimit] int NOT NULL,
    [MMSLimit] int NOT NULL,
    [EmailLimit] int NOT NULL,
    [ContactLimit] int NOT NULL,
    [Features] nvarchar(max) NULL,
    [StripeProductId] nvarchar(100) NULL,
    [StripePriceIdMonthly] nvarchar(100) NULL,
    [StripePriceIdYearly] nvarchar(100) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_SubscriptionPlans] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Campaigns] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Description] nvarchar(1000) NULL,
    [Type] int NOT NULL,
    [Status] int NOT NULL,
    [ScheduledAt] datetime2 NULL,
    [StartedAt] datetime2 NULL,
    [CompletedAt] datetime2 NULL,
    [TotalRecipients] int NOT NULL,
    [SuccessCount] int NOT NULL,
    [FailureCount] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_Campaigns] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Campaigns_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ComplianceSettings] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [RequireDoubleOptIn] bit NOT NULL,
    [EnableQuietHours] bit NOT NULL,
    [QuietHoursStart] time NULL,
    [QuietHoursEnd] time NULL,
    [CompanyName] nvarchar(max) NULL,
    [CompanyAddress] nvarchar(max) NULL,
    [PrivacyPolicyUrl] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_ComplianceSettings] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ComplianceSettings_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ContactGroups] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Description] nvarchar(1000) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_ContactGroups] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ContactGroups_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Contacts] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [PhoneNumber] nvarchar(20) NOT NULL,
    [Email] nvarchar(256) NULL,
    [FirstName] nvarchar(100) NULL,
    [LastName] nvarchar(100) NULL,
    [Country] nvarchar(100) NULL,
    [City] nvarchar(100) NULL,
    [PostalCode] nvarchar(20) NULL,
    [CustomAttributes] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_Contacts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Contacts_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ContactTags] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [Color] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_ContactTags] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ContactTags_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [MessageTemplates] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Channel] int NOT NULL,
    [Category] nvarchar(100) NULL,
    [Subject] nvarchar(200) NULL,
    [MessageBody] nvarchar(max) NOT NULL,
    [MediaUrls] nvarchar(max) NULL,
    [IsDefault] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_MessageTemplates] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MessageTemplates_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [SuppressionLists] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [PhoneOrEmail] nvarchar(max) NOT NULL,
    [Type] int NOT NULL,
    [Reason] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_SuppressionLists] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SuppressionLists_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [UsageTrackings] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [Year] int NOT NULL,
    [Month] int NOT NULL,
    [SMSUsed] int NOT NULL,
    [MMSUsed] int NOT NULL,
    [EmailUsed] int NOT NULL,
    [ContactsUsed] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_UsageTrackings] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UsageTrackings_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [UserActivityLogs] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [Action] nvarchar(max) NOT NULL,
    [Details] nvarchar(max) NULL,
    [IpAddress] nvarchar(max) NULL,
    [UserAgent] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_UserActivityLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserActivityLogs_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Workflows] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Description] nvarchar(1000) NULL,
    [TriggerType] int NOT NULL,
    [TriggerCriteria] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_Workflows] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Workflows_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ProviderLogs] (
    [Id] int NOT NULL IDENTITY,
    [MessageProviderId] int NOT NULL,
    [RequestPayload] nvarchar(max) NOT NULL,
    [ResponsePayload] nvarchar(max) NULL,
    [StatusCode] int NULL,
    [IsSuccess] bit NOT NULL,
    [ErrorMessage] nvarchar(max) NULL,
    [RequestedAt] datetime2 NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_ProviderLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ProviderLogs_MessageProviders_MessageProviderId] FOREIGN KEY ([MessageProviderId]) REFERENCES [MessageProviders] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [UserSubscriptions] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [SubscriptionPlanId] int NOT NULL,
    [Status] int NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NULL,
    [TrialEndDate] datetime2 NULL,
    [IsYearly] bit NOT NULL,
    [StripeSubscriptionId] nvarchar(100) NULL,
    [StripeCustomerId] nvarchar(100) NULL,
    [CanceledAt] datetime2 NULL,
    [CancellationReason] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_UserSubscriptions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserSubscriptions_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserSubscriptions_SubscriptionPlans_SubscriptionPlanId] FOREIGN KEY ([SubscriptionPlanId]) REFERENCES [SubscriptionPlans] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [CampaignAnalytics] (
    [Id] int NOT NULL IDENTITY,
    [CampaignId] int NOT NULL,
    [TotalSent] int NOT NULL,
    [TotalDelivered] int NOT NULL,
    [TotalFailed] int NOT NULL,
    [TotalClicks] int NOT NULL,
    [TotalOptOuts] int NOT NULL,
    [DeliveryRate] decimal(18,2) NOT NULL,
    [ClickRate] decimal(18,2) NOT NULL,
    [OptOutRate] decimal(18,2) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_CampaignAnalytics] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CampaignAnalytics_Campaigns_CampaignId] FOREIGN KEY ([CampaignId]) REFERENCES [Campaigns] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [CampaignAudiences] (
    [Id] int NOT NULL IDENTITY,
    [CampaignId] int NOT NULL,
    [TargetType] int NOT NULL,
    [GroupIds] nvarchar(max) NULL,
    [SegmentCriteria] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_CampaignAudiences] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CampaignAudiences_Campaigns_CampaignId] FOREIGN KEY ([CampaignId]) REFERENCES [Campaigns] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [CampaignSchedules] (
    [Id] int NOT NULL IDENTITY,
    [CampaignId] int NOT NULL,
    [ScheduleType] int NOT NULL,
    [ScheduledDateTime] datetime2 NULL,
    [RecurrencePattern] nvarchar(max) NULL,
    [TimeZone] nvarchar(100) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_CampaignSchedules] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CampaignSchedules_Campaigns_CampaignId] FOREIGN KEY ([CampaignId]) REFERENCES [Campaigns] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [URLShorteners] (
    [Id] int NOT NULL IDENTITY,
    [CampaignId] int NOT NULL,
    [OriginalUrl] nvarchar(2000) NOT NULL,
    [ShortCode] nvarchar(20) NOT NULL,
    [ShortUrl] nvarchar(200) NOT NULL,
    [ClickCount] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_URLShorteners] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_URLShorteners_Campaigns_CampaignId] FOREIGN KEY ([CampaignId]) REFERENCES [Campaigns] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Keywords] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [KeywordText] nvarchar(50) NOT NULL,
    [Description] nvarchar(500) NULL,
    [IsGloballyReserved] bit NOT NULL,
    [Status] int NOT NULL,
    [ResponseMessage] nvarchar(max) NULL,
    [LinkedCampaignId] int NULL,
    [OptInGroupId] int NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_Keywords] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Keywords_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Keywords_Campaigns_LinkedCampaignId] FOREIGN KEY ([LinkedCampaignId]) REFERENCES [Campaigns] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_Keywords_ContactGroups_OptInGroupId] FOREIGN KEY ([OptInGroupId]) REFERENCES [ContactGroups] ([Id]) ON DELETE SET NULL
);
GO

CREATE TABLE [CampaignMessages] (
    [Id] int NOT NULL IDENTITY,
    [CampaignId] int NOT NULL,
    [ContactId] int NOT NULL,
    [Status] int NOT NULL,
    [MessageBody] nvarchar(max) NULL,
    [MediaUrls] nvarchar(max) NULL,
    [SentAt] datetime2 NULL,
    [DeliveredAt] datetime2 NULL,
    [ProviderMessageId] nvarchar(100) NULL,
    [ErrorMessage] nvarchar(1000) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_CampaignMessages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CampaignMessages_Campaigns_CampaignId] FOREIGN KEY ([CampaignId]) REFERENCES [Campaigns] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_CampaignMessages_Contacts_ContactId] FOREIGN KEY ([ContactId]) REFERENCES [Contacts] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [ConsentHistories] (
    [Id] int NOT NULL IDENTITY,
    [ContactId] int NOT NULL,
    [ConsentGiven] bit NOT NULL,
    [ConsentType] nvarchar(max) NULL,
    [IpAddress] nvarchar(max) NULL,
    [ConsentDate] datetime2 NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_ConsentHistories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ConsentHistories_Contacts_ContactId] FOREIGN KEY ([ContactId]) REFERENCES [Contacts] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ContactEngagements] (
    [Id] int NOT NULL IDENTITY,
    [ContactId] int NOT NULL,
    [TotalMessagesSent] int NOT NULL,
    [TotalMessagesDelivered] int NOT NULL,
    [TotalClicks] int NOT NULL,
    [LastEngagementDate] datetime2 NULL,
    [EngagementScore] decimal(18,2) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_ContactEngagements] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ContactEngagements_Contacts_ContactId] FOREIGN KEY ([ContactId]) REFERENCES [Contacts] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ContactGroupMembers] (
    [Id] int NOT NULL IDENTITY,
    [ContactId] int NOT NULL,
    [ContactGroupId] int NOT NULL,
    [JoinedAt] datetime2 NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_ContactGroupMembers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ContactGroupMembers_ContactGroups_ContactGroupId] FOREIGN KEY ([ContactGroupId]) REFERENCES [ContactGroups] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ContactGroupMembers_Contacts_ContactId] FOREIGN KEY ([ContactId]) REFERENCES [Contacts] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ContactTagAssignments] (
    [Id] int NOT NULL IDENTITY,
    [ContactId] int NOT NULL,
    [ContactTagId] int NOT NULL,
    [AssignedAt] datetime2 NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_ContactTagAssignments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ContactTagAssignments_ContactTags_ContactTagId] FOREIGN KEY ([ContactTagId]) REFERENCES [ContactTags] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ContactTagAssignments_Contacts_ContactId] FOREIGN KEY ([ContactId]) REFERENCES [Contacts] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [CampaignContents] (
    [Id] int NOT NULL IDENTITY,
    [CampaignId] int NOT NULL,
    [Subject] nvarchar(200) NULL,
    [MessageBody] nvarchar(max) NULL,
    [MediaUrls] nvarchar(max) NULL,
    [MessageTemplateId] int NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_CampaignContents] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CampaignContents_Campaigns_CampaignId] FOREIGN KEY ([CampaignId]) REFERENCES [Campaigns] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_CampaignContents_MessageTemplates_MessageTemplateId] FOREIGN KEY ([MessageTemplateId]) REFERENCES [MessageTemplates] ([Id]) ON DELETE SET NULL
);
GO

CREATE TABLE [WorkflowExecutions] (
    [Id] int NOT NULL IDENTITY,
    [WorkflowId] int NOT NULL,
    [ContactId] int NOT NULL,
    [Status] int NOT NULL,
    [CurrentStepOrder] int NOT NULL,
    [StartedAt] datetime2 NULL,
    [CompletedAt] datetime2 NULL,
    [ErrorMessage] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_WorkflowExecutions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_WorkflowExecutions_Workflows_WorkflowId] FOREIGN KEY ([WorkflowId]) REFERENCES [Workflows] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [WorkflowSteps] (
    [Id] int NOT NULL IDENTITY,
    [WorkflowId] int NOT NULL,
    [StepOrder] int NOT NULL,
    [ActionType] int NOT NULL,
    [ActionConfiguration] nvarchar(max) NULL,
    [DelayMinutes] int NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_WorkflowSteps] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_WorkflowSteps_Workflows_WorkflowId] FOREIGN KEY ([WorkflowId]) REFERENCES [Workflows] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Invoices] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [UserSubscriptionId] int NULL,
    [InvoiceNumber] nvarchar(50) NOT NULL,
    [Status] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Tax] decimal(18,2) NOT NULL,
    [Total] decimal(18,2) NOT NULL,
    [InvoiceDate] datetime2 NOT NULL,
    [DueDate] datetime2 NULL,
    [PaidDate] datetime2 NULL,
    [StripeInvoiceId] nvarchar(100) NULL,
    [Description] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_Invoices] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Invoices_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Invoices_UserSubscriptions_UserSubscriptionId] FOREIGN KEY ([UserSubscriptionId]) REFERENCES [UserSubscriptions] ([Id]) ON DELETE SET NULL
);
GO

CREATE TABLE [URLClicks] (
    [Id] int NOT NULL IDENTITY,
    [URLShortenerId] int NOT NULL,
    [IpAddress] nvarchar(max) NULL,
    [UserAgent] nvarchar(max) NULL,
    [Referrer] nvarchar(max) NULL,
    [ClickedAt] datetime2 NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_URLClicks] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_URLClicks_URLShorteners_URLShortenerId] FOREIGN KEY ([URLShortenerId]) REFERENCES [URLShorteners] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [KeywordActivities] (
    [Id] int NOT NULL IDENTITY,
    [KeywordId] int NOT NULL,
    [PhoneNumber] nvarchar(max) NOT NULL,
    [IncomingMessage] nvarchar(max) NOT NULL,
    [ResponseSent] nvarchar(max) NULL,
    [ReceivedAt] datetime2 NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_KeywordActivities] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_KeywordActivities_Keywords_KeywordId] FOREIGN KEY ([KeywordId]) REFERENCES [Keywords] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [BillingHistories] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [InvoiceId] int NULL,
    [Type] int NOT NULL,
    [Status] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Description] nvarchar(max) NULL,
    [StripeChargeId] nvarchar(max) NULL,
    [TransactionDate] datetime2 NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_BillingHistories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BillingHistories_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_BillingHistories_Invoices_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES [Invoices] ([Id])
);
GO

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO

CREATE INDEX [IX_BillingHistories_InvoiceId] ON [BillingHistories] ([InvoiceId]);
GO

CREATE INDEX [IX_BillingHistories_UserId] ON [BillingHistories] ([UserId]);
GO

CREATE UNIQUE INDEX [IX_CampaignAnalytics_CampaignId] ON [CampaignAnalytics] ([CampaignId]);
GO

CREATE UNIQUE INDEX [IX_CampaignAudiences_CampaignId] ON [CampaignAudiences] ([CampaignId]);
GO

CREATE UNIQUE INDEX [IX_CampaignContents_CampaignId] ON [CampaignContents] ([CampaignId]);
GO

CREATE INDEX [IX_CampaignContents_MessageTemplateId] ON [CampaignContents] ([MessageTemplateId]);
GO

CREATE INDEX [IX_CampaignMessages_CampaignId] ON [CampaignMessages] ([CampaignId]);
GO

CREATE INDEX [IX_CampaignMessages_ContactId] ON [CampaignMessages] ([ContactId]);
GO

CREATE INDEX [IX_CampaignMessages_Status] ON [CampaignMessages] ([Status]);
GO

CREATE INDEX [IX_Campaigns_CreatedAt] ON [Campaigns] ([CreatedAt]);
GO

CREATE INDEX [IX_Campaigns_ScheduledAt] ON [Campaigns] ([ScheduledAt]);
GO

CREATE INDEX [IX_Campaigns_Status] ON [Campaigns] ([Status]);
GO

CREATE INDEX [IX_Campaigns_UserId] ON [Campaigns] ([UserId]);
GO

CREATE UNIQUE INDEX [IX_CampaignSchedules_CampaignId] ON [CampaignSchedules] ([CampaignId]);
GO

CREATE INDEX [IX_ComplianceSettings_UserId] ON [ComplianceSettings] ([UserId]);
GO

CREATE INDEX [IX_ConsentHistories_ContactId] ON [ConsentHistories] ([ContactId]);
GO

CREATE UNIQUE INDEX [IX_ContactEngagements_ContactId] ON [ContactEngagements] ([ContactId]);
GO

CREATE INDEX [IX_ContactGroupMembers_ContactGroupId] ON [ContactGroupMembers] ([ContactGroupId]);
GO

CREATE INDEX [IX_ContactGroupMembers_ContactId] ON [ContactGroupMembers] ([ContactId]);
GO

CREATE UNIQUE INDEX [IX_ContactGroupMembers_ContactId_ContactGroupId] ON [ContactGroupMembers] ([ContactId], [ContactGroupId]);
GO

CREATE INDEX [IX_ContactGroups_Name] ON [ContactGroups] ([Name]);
GO

CREATE INDEX [IX_ContactGroups_UserId] ON [ContactGroups] ([UserId]);
GO

CREATE INDEX [IX_Contacts_CreatedAt] ON [Contacts] ([CreatedAt]);
GO

CREATE INDEX [IX_Contacts_Email] ON [Contacts] ([Email]);
GO

CREATE INDEX [IX_Contacts_PhoneNumber] ON [Contacts] ([PhoneNumber]);
GO

CREATE INDEX [IX_Contacts_UserId] ON [Contacts] ([UserId]);
GO

CREATE INDEX [IX_ContactTagAssignments_ContactId] ON [ContactTagAssignments] ([ContactId]);
GO

CREATE UNIQUE INDEX [IX_ContactTagAssignments_ContactId_ContactTagId] ON [ContactTagAssignments] ([ContactId], [ContactTagId]);
GO

CREATE INDEX [IX_ContactTagAssignments_ContactTagId] ON [ContactTagAssignments] ([ContactTagId]);
GO

CREATE INDEX [IX_ContactTags_UserId] ON [ContactTags] ([UserId]);
GO

CREATE UNIQUE INDEX [IX_Invoices_InvoiceNumber] ON [Invoices] ([InvoiceNumber]);
GO

CREATE INDEX [IX_Invoices_Status] ON [Invoices] ([Status]);
GO

CREATE INDEX [IX_Invoices_UserId] ON [Invoices] ([UserId]);
GO

CREATE INDEX [IX_Invoices_UserSubscriptionId] ON [Invoices] ([UserSubscriptionId]);
GO

CREATE INDEX [IX_KeywordActivities_KeywordId] ON [KeywordActivities] ([KeywordId]);
GO

CREATE INDEX [IX_Keywords_KeywordText] ON [Keywords] ([KeywordText]);
GO

CREATE INDEX [IX_Keywords_LinkedCampaignId] ON [Keywords] ([LinkedCampaignId]);
GO

CREATE INDEX [IX_Keywords_OptInGroupId] ON [Keywords] ([OptInGroupId]);
GO

CREATE INDEX [IX_Keywords_UserId] ON [Keywords] ([UserId]);
GO

CREATE INDEX [IX_MessageTemplates_Category] ON [MessageTemplates] ([Category]);
GO

CREATE INDEX [IX_MessageTemplates_Channel] ON [MessageTemplates] ([Channel]);
GO

CREATE INDEX [IX_MessageTemplates_UserId] ON [MessageTemplates] ([UserId]);
GO

CREATE INDEX [IX_ProviderLogs_MessageProviderId] ON [ProviderLogs] ([MessageProviderId]);
GO

CREATE INDEX [IX_SubscriptionPlans_Name] ON [SubscriptionPlans] ([Name]);
GO

CREATE INDEX [IX_SuppressionLists_UserId] ON [SuppressionLists] ([UserId]);
GO

CREATE INDEX [IX_URLClicks_URLShortenerId] ON [URLClicks] ([URLShortenerId]);
GO

CREATE INDEX [IX_URLShorteners_CampaignId] ON [URLShorteners] ([CampaignId]);
GO

CREATE UNIQUE INDEX [IX_URLShorteners_ShortCode] ON [URLShorteners] ([ShortCode]);
GO

CREATE INDEX [IX_UsageTrackings_UserId] ON [UsageTrackings] ([UserId]);
GO

CREATE UNIQUE INDEX [IX_UsageTrackings_UserId_Year_Month] ON [UsageTrackings] ([UserId], [Year], [Month]);
GO

CREATE INDEX [IX_UserActivityLogs_UserId] ON [UserActivityLogs] ([UserId]);
GO

CREATE INDEX [IX_UserSubscriptions_Status] ON [UserSubscriptions] ([Status]);
GO

CREATE INDEX [IX_UserSubscriptions_SubscriptionPlanId] ON [UserSubscriptions] ([SubscriptionPlanId]);
GO

CREATE INDEX [IX_UserSubscriptions_UserId] ON [UserSubscriptions] ([UserId]);
GO

CREATE INDEX [IX_WorkflowExecutions_WorkflowId] ON [WorkflowExecutions] ([WorkflowId]);
GO

CREATE INDEX [IX_Workflows_UserId] ON [Workflows] ([UserId]);
GO

CREATE INDEX [IX_WorkflowSteps_WorkflowId] ON [WorkflowSteps] ([WorkflowId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260117211155_InitialCreate', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [RefreshTokens] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [Token] nvarchar(max) NOT NULL,
    [ExpiresAt] datetime2 NOT NULL,
    [IsRevoked] bit NOT NULL,
    [ReplacedByToken] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RefreshTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_RefreshTokens_UserId] ON [RefreshTokens] ([UserId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260117213055_AddRefreshToken', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [ContactGroups] ADD [ContactCount] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [ContactGroups] ADD [IsDynamic] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [ContactGroups] ADD [IsStatic] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [ContactGroups] ADD [RuleCriteria] nvarchar(max) NULL;
GO

ALTER TABLE [CampaignSchedules] ADD [TimeZoneAware] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [CampaignMessages] ADD [Channel] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [CampaignMessages] ADD [CostAmount] decimal(18,2) NOT NULL DEFAULT 0.0;
GO

ALTER TABLE [CampaignMessages] ADD [ExternalMessageId] nvarchar(max) NULL;
GO

ALTER TABLE [CampaignMessages] ADD [FailedAt] datetime2 NULL;
GO

ALTER TABLE [CampaignMessages] ADD [HTMLContent] nvarchar(max) NULL;
GO

ALTER TABLE [CampaignMessages] ADD [MaxRetries] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [CampaignMessages] ADD [Recipient] nvarchar(max) NOT NULL DEFAULT N'';
GO

ALTER TABLE [CampaignMessages] ADD [RetryCount] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [CampaignMessages] ADD [ScheduledAt] datetime2 NULL;
GO

ALTER TABLE [CampaignMessages] ADD [Subject] nvarchar(max) NULL;
GO

ALTER TABLE [CampaignContents] ADD [Channel] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [CampaignContents] ADD [HTMLContent] nvarchar(max) NULL;
GO

ALTER TABLE [CampaignContents] ADD [PersonalizationTokens] nvarchar(max) NULL;
GO

ALTER TABLE [CampaignAudiences] ADD [ExclusionListIds] nvarchar(max) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260118063054_UpdateCampaignMessageEntity', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

EXEC sp_rename N'[MessageTemplates].[MediaUrls]', N'TemplateVariables', N'COLUMN';
GO

DROP INDEX [IX_MessageTemplates_Category] ON [MessageTemplates];
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MessageTemplates]') AND [c].[name] = N'Category');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [MessageTemplates] DROP CONSTRAINT [' + @var0 + '];');
UPDATE [MessageTemplates] SET [Category] = 0 WHERE [Category] IS NULL;
ALTER TABLE [MessageTemplates] ALTER COLUMN [Category] int NOT NULL;
ALTER TABLE [MessageTemplates] ADD DEFAULT 0 FOR [Category];
CREATE INDEX [IX_MessageTemplates_Category] ON [MessageTemplates] ([Category]);
GO

ALTER TABLE [MessageTemplates] ADD [DefaultMediaUrls] nvarchar(max) NULL;
GO

ALTER TABLE [MessageTemplates] ADD [Description] nvarchar(max) NULL;
GO

ALTER TABLE [MessageTemplates] ADD [HTMLContent] nvarchar(max) NULL;
GO

ALTER TABLE [MessageTemplates] ADD [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit);
GO

ALTER TABLE [MessageTemplates] ADD [LastUsedAt] datetime2 NULL;
GO

ALTER TABLE [MessageTemplates] ADD [UsageCount] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [ContactGroups] ADD [ContactCount] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [ContactGroups] ADD [IsDynamic] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [ContactGroups] ADD [IsStatic] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [ContactGroups] ADD [RuleCriteria] nvarchar(max) NULL;
GO

ALTER TABLE [CampaignSchedules] ADD [TimeZoneAware] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [CampaignMessages] ADD [CostAmount] decimal(18,2) NOT NULL DEFAULT 0.0;
GO

ALTER TABLE [CampaignContents] ADD [Channel] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [CampaignContents] ADD [HTMLContent] nvarchar(max) NULL;
GO

ALTER TABLE [CampaignContents] ADD [PersonalizationTokens] nvarchar(max) NULL;
GO

ALTER TABLE [CampaignAudiences] ADD [ExclusionListIds] nvarchar(max) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260118064723_AddTemplateManagementFields', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [FrequencyControls] (
    [Id] int NOT NULL IDENTITY,
    [ContactId] int NOT NULL,
    [UserId] nvarchar(450) NOT NULL,
    [MaxMessagesPerDay] int NOT NULL DEFAULT 5,
    [MaxMessagesPerWeek] int NOT NULL DEFAULT 20,
    [MaxMessagesPerMonth] int NOT NULL DEFAULT 50,
    [LastMessageSentAt] datetime2 NOT NULL,
    [MessagesSentToday] int NOT NULL DEFAULT 0,
    [MessagesSentThisWeek] int NOT NULL DEFAULT 0,
    [MessagesSentThisMonth] int NOT NULL DEFAULT 0,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_FrequencyControls] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_FrequencyControls_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]),
    CONSTRAINT [FK_FrequencyControls_Contacts_ContactId] FOREIGN KEY ([ContactId]) REFERENCES [Contacts] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_FrequencyControls_ContactId] ON [FrequencyControls] ([ContactId]);
GO

CREATE UNIQUE INDEX [IX_FrequencyControls_ContactId_UserId] ON [FrequencyControls] ([ContactId], [UserId]);
GO

CREATE INDEX [IX_FrequencyControls_UserId] ON [FrequencyControls] ([UserId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260118170818_AddSchedulingAndAutomation', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[FrequencyControls]') AND [c].[name] = N'LastMessageSentAt');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [FrequencyControls] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [FrequencyControls] ALTER COLUMN [LastMessageSentAt] datetime2 NULL;
GO

CREATE TABLE [ChannelRoutingConfigs] (
    [Id] int NOT NULL IDENTITY,
    [Channel] int NOT NULL,
    [PrimaryProvider] nvarchar(100) NOT NULL,
    [FallbackProvider] nvarchar(100) NULL,
    [RoutingStrategy] int NOT NULL,
    [EnableFallback] bit NOT NULL,
    [MaxRetries] int NOT NULL,
    [RetryStrategy] int NOT NULL,
    [InitialRetryDelaySeconds] int NOT NULL,
    [MaxRetryDelaySeconds] int NOT NULL,
    [CostThreshold] decimal(18,6) NULL,
    [IsActive] bit NOT NULL,
    [Priority] int NOT NULL,
    [AdditionalSettings] nvarchar(4000) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_ChannelRoutingConfigs] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [MessageDeliveryAttempts] (
    [Id] int NOT NULL IDENTITY,
    [CampaignMessageId] int NOT NULL,
    [AttemptNumber] int NOT NULL,
    [Channel] int NOT NULL,
    [ProviderName] nvarchar(100) NULL,
    [AttemptedAt] datetime2 NOT NULL,
    [Success] bit NOT NULL,
    [ExternalMessageId] nvarchar(200) NULL,
    [ErrorMessage] nvarchar(2000) NULL,
    [ErrorCode] nvarchar(100) NULL,
    [CostAmount] decimal(18,6) NULL,
    [ResponseTimeMs] int NOT NULL,
    [FallbackReason] int NULL,
    [AdditionalMetadata] nvarchar(4000) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_MessageDeliveryAttempts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MessageDeliveryAttempts_CampaignMessages_CampaignMessageId] FOREIGN KEY ([CampaignMessageId]) REFERENCES [CampaignMessages] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_ChannelRoutingConfigs_Channel] ON [ChannelRoutingConfigs] ([Channel]);
GO

CREATE INDEX [IX_ChannelRoutingConfigs_Channel_IsActive_Priority] ON [ChannelRoutingConfigs] ([Channel], [IsActive], [Priority]);
GO

CREATE INDEX [IX_MessageDeliveryAttempts_AttemptedAt] ON [MessageDeliveryAttempts] ([AttemptedAt]);
GO

CREATE INDEX [IX_MessageDeliveryAttempts_CampaignMessageId] ON [MessageDeliveryAttempts] ([CampaignMessageId]);
GO

CREATE INDEX [IX_MessageDeliveryAttempts_CampaignMessageId_AttemptNumber] ON [MessageDeliveryAttempts] ([CampaignMessageId], [AttemptNumber]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260118172455_AddMessageRoutingAndDeliveryTracking', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[FrequencyControls]') AND [c].[name] = N'LastMessageSentAt');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [FrequencyControls] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [FrequencyControls] ALTER COLUMN [LastMessageSentAt] datetime2 NULL;
GO

ALTER TABLE [Contacts] ADD [EmailOptIn] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Contacts] ADD [EmailOptInDate] datetime2 NULL;
GO

ALTER TABLE [Contacts] ADD [MmsOptIn] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Contacts] ADD [MmsOptInDate] datetime2 NULL;
GO

ALTER TABLE [Contacts] ADD [SmsOptIn] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Contacts] ADD [SmsOptInDate] datetime2 NULL;
GO

ALTER TABLE [ConsentHistories] ADD [Channel] int NULL;
GO

ALTER TABLE [ConsentHistories] ADD [Source] int NULL;
GO

ALTER TABLE [ConsentHistories] ADD [UserAgent] nvarchar(max) NULL;
GO

ALTER TABLE [ComplianceSettings] ADD [ConsentRetentionDays] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [ComplianceSettings] ADD [EnableAuditLogging] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [ComplianceSettings] ADD [EnableConsentTracking] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [ComplianceSettings] ADD [EnforceSuppressionList] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [ComplianceSettings] ADD [OptInConfirmationMessage] nvarchar(max) NULL;
GO

ALTER TABLE [ComplianceSettings] ADD [OptInKeywords] nvarchar(max) NULL;
GO

ALTER TABLE [ComplianceSettings] ADD [OptOutConfirmationMessage] nvarchar(max) NULL;
GO

ALTER TABLE [ComplianceSettings] ADD [OptOutKeywords] nvarchar(max) NULL;
GO

ALTER TABLE [ComplianceSettings] ADD [QuietHoursTimeZone] nvarchar(max) NULL;
GO

ALTER TABLE [ComplianceSettings] ADD [RequireDoubleOptInEmail] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [ComplianceSettings] ADD [RequireDoubleOptInSms] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [ComplianceSettings] ADD [TermsOfServiceUrl] nvarchar(max) NULL;
GO

CREATE TABLE [ComplianceAuditLogs] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ContactId] int NULL,
    [CampaignId] int NULL,
    [ActionType] int NOT NULL,
    [Channel] int NOT NULL,
    [Description] nvarchar(max) NULL,
    [IpAddress] nvarchar(max) NULL,
    [UserAgent] nvarchar(max) NULL,
    [Metadata] nvarchar(max) NULL,
    [ActionDate] datetime2 NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_ComplianceAuditLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ComplianceAuditLogs_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ComplianceAuditLogs_Campaigns_CampaignId] FOREIGN KEY ([CampaignId]) REFERENCES [Campaigns] ([Id]),
    CONSTRAINT [FK_ComplianceAuditLogs_Contacts_ContactId] FOREIGN KEY ([ContactId]) REFERENCES [Contacts] ([Id])
);
GO

CREATE TABLE [ContactConsents] (
    [Id] int NOT NULL IDENTITY,
    [ContactId] int NOT NULL,
    [Channel] int NOT NULL,
    [Status] int NOT NULL,
    [Source] int NOT NULL,
    [ConsentDate] datetime2 NOT NULL,
    [RevokedDate] datetime2 NULL,
    [IpAddress] nvarchar(max) NULL,
    [UserAgent] nvarchar(max) NULL,
    [Notes] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_ContactConsents] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ContactConsents_Contacts_ContactId] FOREIGN KEY ([ContactId]) REFERENCES [Contacts] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_ComplianceAuditLogs_CampaignId] ON [ComplianceAuditLogs] ([CampaignId]);
GO

CREATE INDEX [IX_ComplianceAuditLogs_ContactId] ON [ComplianceAuditLogs] ([ContactId]);
GO

CREATE INDEX [IX_ComplianceAuditLogs_UserId] ON [ComplianceAuditLogs] ([UserId]);
GO

CREATE INDEX [IX_ContactConsents_ContactId] ON [ContactConsents] ([ContactId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260118172828_AddComplianceAndConsentManagement', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [ComplianceAuditLogs] ADD [Channel] int NOT NULL DEFAULT 0;
GO

CREATE TABLE [CustomRoles] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(50) NOT NULL,
    [Description] nvarchar(500) NOT NULL,
    [Permissions] bigint NOT NULL,
    [IsSystemRole] bit NOT NULL DEFAULT CAST(0 AS bit),
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    CONSTRAINT [PK_CustomRoles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [CustomUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] int NOT NULL,
    [AssignedAt] datetime2 NOT NULL,
    [AssignedBy] nvarchar(450) NULL,
    CONSTRAINT [PK_CustomUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_CustomUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_CustomUserRoles_CustomRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [CustomRoles] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_CustomRoles_IsActive] ON [CustomRoles] ([IsActive]);
GO

CREATE UNIQUE INDEX [IX_CustomRoles_Name] ON [CustomRoles] ([Name]);
GO

CREATE INDEX [IX_CustomUserRoles_RoleId] ON [CustomUserRoles] ([RoleId]);
GO

CREATE INDEX [IX_CustomUserRoles_UserId] ON [CustomUserRoles] ([UserId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260118175341_AddRBACSystem', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [FirstName] nvarchar(max) NULL,
    [LastName] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [LastLoginAt] datetime2 NULL,
    [IsActive] bit NOT NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [ChannelRoutingConfigs] (
    [Id] int NOT NULL IDENTITY,
    [Channel] int NOT NULL,
    [PrimaryProvider] nvarchar(100) NOT NULL,
    [FallbackProvider] nvarchar(100) NULL,
    [RoutingStrategy] int NOT NULL,
    [EnableFallback] bit NOT NULL,
    [MaxRetries] int NOT NULL,
    [RetryStrategy] int NOT NULL,
    [InitialRetryDelaySeconds] int NOT NULL,
    [MaxRetryDelaySeconds] int NOT NULL,
    [CostThreshold] decimal(18,6) NULL,
    [IsActive] bit NOT NULL,
    [Priority] int NOT NULL,
    [AdditionalSettings] nvarchar(4000) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_ChannelRoutingConfigs] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [CustomRoles] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(50) NOT NULL,
    [Description] nvarchar(500) NOT NULL,
    [Permissions] bigint NOT NULL,
    [IsSystemRole] bit NOT NULL DEFAULT CAST(0 AS bit),
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    CONSTRAINT [PK_CustomRoles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [MessageProviders] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [Type] int NOT NULL,
    [ApiKey] nvarchar(max) NULL,
    [ApiSecret] nvarchar(max) NULL,
    [Configuration] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [IsPrimary] bit NOT NULL,
    [HealthStatus] int NOT NULL,
    [LastHealthCheck] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_MessageProviders] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [ProviderRateLimits] (
    [Id] int NOT NULL IDENTITY,
    [ProviderName] nvarchar(100) NOT NULL,
    [ProviderType] nvarchar(50) NOT NULL,
    [MaxRequests] int NOT NULL DEFAULT 1000,
    [TimeWindowSeconds] int NOT NULL DEFAULT 60,
    [CurrentRequestCount] int NOT NULL DEFAULT 0,
    [WindowStartTime] datetime2 NOT NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [UserId] nvarchar(450) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_ProviderRateLimits] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [SubscriptionPlans] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [PriceMonthly] decimal(18,2) NOT NULL,
    [PriceYearly] decimal(18,2) NOT NULL,
    [SMSLimit] int NOT NULL,
    [MMSLimit] int NOT NULL,
    [EmailLimit] int NOT NULL,
    [ContactLimit] int NOT NULL,
    [Features] nvarchar(max) NULL,
    [StripeProductId] nvarchar(100) NULL,
    [StripePriceIdMonthly] nvarchar(100) NULL,
    [StripePriceIdYearly] nvarchar(100) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_SubscriptionPlans] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ApiRateLimits] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NULL,
    [TenantId] nvarchar(450) NULL,
    [EndpointPattern] nvarchar(500) NOT NULL,
    [MaxRequests] int NOT NULL DEFAULT 100,
    [TimeWindowSeconds] int NOT NULL DEFAULT 60,
    [CurrentRequestCount] int NOT NULL DEFAULT 0,
    [WindowStartTime] datetime2 NOT NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [Priority] int NOT NULL DEFAULT 0,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_ApiRateLimits] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ApiRateLimits_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Campaigns] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Description] nvarchar(1000) NULL,
    [Type] int NOT NULL,
    [Status] int NOT NULL,
    [ScheduledAt] datetime2 NULL,
    [StartedAt] datetime2 NULL,
    [CompletedAt] datetime2 NULL,
    [TotalRecipients] int NOT NULL,
    [SuccessCount] int NOT NULL,
    [FailureCount] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_Campaigns] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Campaigns_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ComplianceSettings] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [RequireDoubleOptIn] bit NOT NULL,
    [RequireDoubleOptInSms] bit NOT NULL,
    [RequireDoubleOptInEmail] bit NOT NULL,
    [EnableQuietHours] bit NOT NULL,
    [QuietHoursStart] time NULL,
    [QuietHoursEnd] time NULL,
    [QuietHoursTimeZone] nvarchar(max) NULL,
    [CompanyName] nvarchar(max) NULL,
    [CompanyAddress] nvarchar(max) NULL,
    [PrivacyPolicyUrl] nvarchar(max) NULL,
    [TermsOfServiceUrl] nvarchar(max) NULL,
    [OptOutKeywords] nvarchar(max) NULL,
    [OptInKeywords] nvarchar(max) NULL,
    [OptOutConfirmationMessage] nvarchar(max) NULL,
    [OptInConfirmationMessage] nvarchar(max) NULL,
    [EnforceSuppressionList] bit NOT NULL,
    [EnableConsentTracking] bit NOT NULL,
    [EnableAuditLogging] bit NOT NULL,
    [ConsentRetentionDays] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_ComplianceSettings] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ComplianceSettings_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ContactGroups] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Description] nvarchar(1000) NULL,
    [IsStatic] bit NOT NULL,
    [IsDynamic] bit NOT NULL,
    [RuleCriteria] nvarchar(max) NULL,
    [ContactCount] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_ContactGroups] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ContactGroups_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Contacts] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [PhoneNumber] nvarchar(20) NOT NULL,
    [Email] nvarchar(256) NULL,
    [FirstName] nvarchar(100) NULL,
    [LastName] nvarchar(100) NULL,
    [Country] nvarchar(100) NULL,
    [City] nvarchar(100) NULL,
    [PostalCode] nvarchar(20) NULL,
    [CustomAttributes] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [SmsOptIn] bit NOT NULL,
    [MmsOptIn] bit NOT NULL,
    [EmailOptIn] bit NOT NULL,
    [SmsOptInDate] datetime2 NULL,
    [MmsOptInDate] datetime2 NULL,
    [EmailOptInDate] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_Contacts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Contacts_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ContactTags] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [Color] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_ContactTags] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ContactTags_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [MessageTemplates] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Description] nvarchar(max) NULL,
    [Channel] int NOT NULL,
    [Category] int NOT NULL,
    [Subject] nvarchar(200) NULL,
    [MessageBody] nvarchar(max) NOT NULL,
    [HTMLContent] nvarchar(max) NULL,
    [DefaultMediaUrls] nvarchar(max) NULL,
    [TemplateVariables] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [IsDefault] bit NOT NULL,
    [UsageCount] int NOT NULL,
    [LastUsedAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_MessageTemplates] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MessageTemplates_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [RateLimitLogs] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [TenantId] nvarchar(450) NULL,
    [Endpoint] nvarchar(500) NOT NULL,
    [HttpMethod] nvarchar(10) NOT NULL,
    [IpAddress] nvarchar(45) NOT NULL,
    [RateLimitRule] nvarchar(500) NOT NULL,
    [RequestCount] int NOT NULL,
    [MaxRequests] int NOT NULL,
    [TimeWindowSeconds] int NOT NULL,
    [TriggeredAt] datetime2 NOT NULL,
    [RetryAfterSeconds] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_RateLimitLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RateLimitLogs_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [RefreshTokens] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [Token] nvarchar(max) NOT NULL,
    [ExpiresAt] datetime2 NOT NULL,
    [IsRevoked] bit NOT NULL,
    [ReplacedByToken] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RefreshTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [SuppressionLists] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [PhoneOrEmail] nvarchar(max) NOT NULL,
    [Type] int NOT NULL,
    [Reason] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_SuppressionLists] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SuppressionLists_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [UsageTrackings] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [Year] int NOT NULL,
    [Month] int NOT NULL,
    [SMSUsed] int NOT NULL,
    [MMSUsed] int NOT NULL,
    [EmailUsed] int NOT NULL,
    [ContactsUsed] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_UsageTrackings] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UsageTrackings_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [UserActivityLogs] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [Action] nvarchar(max) NOT NULL,
    [Details] nvarchar(max) NULL,
    [IpAddress] nvarchar(max) NULL,
    [UserAgent] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_UserActivityLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserActivityLogs_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Workflows] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Description] nvarchar(1000) NULL,
    [TriggerType] int NOT NULL,
    [TriggerCriteria] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_Workflows] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Workflows_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [CustomUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] int NOT NULL,
    [AssignedAt] datetime2 NOT NULL,
    [AssignedBy] nvarchar(450) NULL,
    CONSTRAINT [PK_CustomUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_CustomUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_CustomUserRoles_CustomRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [CustomRoles] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ProviderLogs] (
    [Id] int NOT NULL IDENTITY,
    [MessageProviderId] int NOT NULL,
    [RequestPayload] nvarchar(max) NOT NULL,
    [ResponsePayload] nvarchar(max) NULL,
    [StatusCode] int NULL,
    [IsSuccess] bit NOT NULL,
    [ErrorMessage] nvarchar(max) NULL,
    [RequestedAt] datetime2 NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_ProviderLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ProviderLogs_MessageProviders_MessageProviderId] FOREIGN KEY ([MessageProviderId]) REFERENCES [MessageProviders] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [UserSubscriptions] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [SubscriptionPlanId] int NOT NULL,
    [Status] int NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NULL,
    [TrialEndDate] datetime2 NULL,
    [IsYearly] bit NOT NULL,
    [StripeSubscriptionId] nvarchar(100) NULL,
    [StripeCustomerId] nvarchar(100) NULL,
    [CanceledAt] datetime2 NULL,
    [CancellationReason] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_UserSubscriptions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserSubscriptions_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserSubscriptions_SubscriptionPlans_SubscriptionPlanId] FOREIGN KEY ([SubscriptionPlanId]) REFERENCES [SubscriptionPlans] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [CampaignAnalytics] (
    [Id] int NOT NULL IDENTITY,
    [CampaignId] int NOT NULL,
    [TotalSent] int NOT NULL,
    [TotalDelivered] int NOT NULL,
    [TotalFailed] int NOT NULL,
    [TotalClicks] int NOT NULL,
    [TotalOptOuts] int NOT NULL,
    [DeliveryRate] decimal(18,2) NOT NULL,
    [ClickRate] decimal(18,2) NOT NULL,
    [OptOutRate] decimal(18,2) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_CampaignAnalytics] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CampaignAnalytics_Campaigns_CampaignId] FOREIGN KEY ([CampaignId]) REFERENCES [Campaigns] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [CampaignAudiences] (
    [Id] int NOT NULL IDENTITY,
    [CampaignId] int NOT NULL,
    [TargetType] int NOT NULL,
    [GroupIds] nvarchar(max) NULL,
    [SegmentCriteria] nvarchar(max) NULL,
    [ExclusionListIds] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_CampaignAudiences] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CampaignAudiences_Campaigns_CampaignId] FOREIGN KEY ([CampaignId]) REFERENCES [Campaigns] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [CampaignSchedules] (
    [Id] int NOT NULL IDENTITY,
    [CampaignId] int NOT NULL,
    [ScheduleType] int NOT NULL,
    [ScheduledDateTime] datetime2 NULL,
    [RecurrencePattern] nvarchar(max) NULL,
    [TimeZoneAware] bit NOT NULL,
    [TimeZone] nvarchar(100) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_CampaignSchedules] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CampaignSchedules_Campaigns_CampaignId] FOREIGN KEY ([CampaignId]) REFERENCES [Campaigns] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [URLShorteners] (
    [Id] int NOT NULL IDENTITY,
    [CampaignId] int NOT NULL,
    [OriginalUrl] nvarchar(2000) NOT NULL,
    [ShortCode] nvarchar(20) NOT NULL,
    [ShortUrl] nvarchar(200) NOT NULL,
    [ClickCount] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_URLShorteners] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_URLShorteners_Campaigns_CampaignId] FOREIGN KEY ([CampaignId]) REFERENCES [Campaigns] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Keywords] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [KeywordText] nvarchar(50) NOT NULL,
    [Description] nvarchar(500) NULL,
    [IsGloballyReserved] bit NOT NULL,
    [Status] int NOT NULL,
    [ResponseMessage] nvarchar(max) NULL,
    [LinkedCampaignId] int NULL,
    [OptInGroupId] int NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_Keywords] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Keywords_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Keywords_Campaigns_LinkedCampaignId] FOREIGN KEY ([LinkedCampaignId]) REFERENCES [Campaigns] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_Keywords_ContactGroups_OptInGroupId] FOREIGN KEY ([OptInGroupId]) REFERENCES [ContactGroups] ([Id]) ON DELETE SET NULL
);
GO

CREATE TABLE [CampaignMessages] (
    [Id] int NOT NULL IDENTITY,
    [CampaignId] int NOT NULL,
    [ContactId] int NOT NULL,
    [Recipient] nvarchar(max) NOT NULL,
    [Channel] int NOT NULL,
    [Status] int NOT NULL,
    [Subject] nvarchar(max) NULL,
    [MessageBody] nvarchar(max) NULL,
    [HTMLContent] nvarchar(max) NULL,
    [MediaUrls] nvarchar(max) NULL,
    [ScheduledAt] datetime2 NULL,
    [SentAt] datetime2 NULL,
    [DeliveredAt] datetime2 NULL,
    [FailedAt] datetime2 NULL,
    [ExternalMessageId] nvarchar(max) NULL,
    [ProviderMessageId] nvarchar(100) NULL,
    [ErrorMessage] nvarchar(1000) NULL,
    [CostAmount] decimal(18,2) NOT NULL,
    [RetryCount] int NOT NULL,
    [MaxRetries] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_CampaignMessages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CampaignMessages_Campaigns_CampaignId] FOREIGN KEY ([CampaignId]) REFERENCES [Campaigns] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_CampaignMessages_Contacts_ContactId] FOREIGN KEY ([ContactId]) REFERENCES [Contacts] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [ComplianceAuditLogs] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ContactId] int NULL,
    [CampaignId] int NULL,
    [ActionType] int NOT NULL,
    [Channel] int NOT NULL,
    [Description] nvarchar(max) NULL,
    [IpAddress] nvarchar(max) NULL,
    [UserAgent] nvarchar(max) NULL,
    [Metadata] nvarchar(max) NULL,
    [ActionDate] datetime2 NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_ComplianceAuditLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ComplianceAuditLogs_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ComplianceAuditLogs_Campaigns_CampaignId] FOREIGN KEY ([CampaignId]) REFERENCES [Campaigns] ([Id]),
    CONSTRAINT [FK_ComplianceAuditLogs_Contacts_ContactId] FOREIGN KEY ([ContactId]) REFERENCES [Contacts] ([Id])
);
GO

CREATE TABLE [ConsentHistories] (
    [Id] int NOT NULL IDENTITY,
    [ContactId] int NOT NULL,
    [ConsentGiven] bit NOT NULL,
    [ConsentType] nvarchar(max) NULL,
    [Channel] int NULL,
    [Source] int NULL,
    [IpAddress] nvarchar(max) NULL,
    [UserAgent] nvarchar(max) NULL,
    [ConsentDate] datetime2 NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_ConsentHistories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ConsentHistories_Contacts_ContactId] FOREIGN KEY ([ContactId]) REFERENCES [Contacts] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ContactConsents] (
    [Id] int NOT NULL IDENTITY,
    [ContactId] int NOT NULL,
    [Channel] int NOT NULL,
    [Status] int NOT NULL,
    [Source] int NOT NULL,
    [ConsentDate] datetime2 NOT NULL,
    [RevokedDate] datetime2 NULL,
    [IpAddress] nvarchar(max) NULL,
    [UserAgent] nvarchar(max) NULL,
    [Notes] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_ContactConsents] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ContactConsents_Contacts_ContactId] FOREIGN KEY ([ContactId]) REFERENCES [Contacts] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ContactEngagements] (
    [Id] int NOT NULL IDENTITY,
    [ContactId] int NOT NULL,
    [TotalMessagesSent] int NOT NULL,
    [TotalMessagesDelivered] int NOT NULL,
    [TotalClicks] int NOT NULL,
    [LastEngagementDate] datetime2 NULL,
    [EngagementScore] decimal(18,2) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_ContactEngagements] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ContactEngagements_Contacts_ContactId] FOREIGN KEY ([ContactId]) REFERENCES [Contacts] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ContactGroupMembers] (
    [Id] int NOT NULL IDENTITY,
    [ContactId] int NOT NULL,
    [ContactGroupId] int NOT NULL,
    [JoinedAt] datetime2 NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_ContactGroupMembers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ContactGroupMembers_ContactGroups_ContactGroupId] FOREIGN KEY ([ContactGroupId]) REFERENCES [ContactGroups] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ContactGroupMembers_Contacts_ContactId] FOREIGN KEY ([ContactId]) REFERENCES [Contacts] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [FrequencyControls] (
    [Id] int NOT NULL IDENTITY,
    [ContactId] int NOT NULL,
    [UserId] nvarchar(450) NOT NULL,
    [MaxMessagesPerDay] int NOT NULL DEFAULT 5,
    [MaxMessagesPerWeek] int NOT NULL DEFAULT 20,
    [MaxMessagesPerMonth] int NOT NULL DEFAULT 50,
    [LastMessageSentAt] datetime2 NULL,
    [MessagesSentToday] int NOT NULL DEFAULT 0,
    [MessagesSentThisWeek] int NOT NULL DEFAULT 0,
    [MessagesSentThisMonth] int NOT NULL DEFAULT 0,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_FrequencyControls] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_FrequencyControls_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]),
    CONSTRAINT [FK_FrequencyControls_Contacts_ContactId] FOREIGN KEY ([ContactId]) REFERENCES [Contacts] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ContactTagAssignments] (
    [Id] int NOT NULL IDENTITY,
    [ContactId] int NOT NULL,
    [ContactTagId] int NOT NULL,
    [AssignedAt] datetime2 NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_ContactTagAssignments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ContactTagAssignments_ContactTags_ContactTagId] FOREIGN KEY ([ContactTagId]) REFERENCES [ContactTags] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ContactTagAssignments_Contacts_ContactId] FOREIGN KEY ([ContactId]) REFERENCES [Contacts] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [CampaignContents] (
    [Id] int NOT NULL IDENTITY,
    [CampaignId] int NOT NULL,
    [Channel] int NOT NULL,
    [Subject] nvarchar(200) NULL,
    [MessageBody] nvarchar(max) NULL,
    [HTMLContent] nvarchar(max) NULL,
    [MediaUrls] nvarchar(max) NULL,
    [MessageTemplateId] int NULL,
    [PersonalizationTokens] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_CampaignContents] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CampaignContents_Campaigns_CampaignId] FOREIGN KEY ([CampaignId]) REFERENCES [Campaigns] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_CampaignContents_MessageTemplates_MessageTemplateId] FOREIGN KEY ([MessageTemplateId]) REFERENCES [MessageTemplates] ([Id]) ON DELETE SET NULL
);
GO

CREATE TABLE [WorkflowExecutions] (
    [Id] int NOT NULL IDENTITY,
    [WorkflowId] int NOT NULL,
    [ContactId] int NOT NULL,
    [Status] int NOT NULL,
    [CurrentStepOrder] int NOT NULL,
    [StartedAt] datetime2 NULL,
    [CompletedAt] datetime2 NULL,
    [ErrorMessage] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_WorkflowExecutions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_WorkflowExecutions_Workflows_WorkflowId] FOREIGN KEY ([WorkflowId]) REFERENCES [Workflows] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [WorkflowSteps] (
    [Id] int NOT NULL IDENTITY,
    [WorkflowId] int NOT NULL,
    [StepOrder] int NOT NULL,
    [ActionType] int NOT NULL,
    [ActionConfiguration] nvarchar(max) NULL,
    [DelayMinutes] int NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_WorkflowSteps] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_WorkflowSteps_Workflows_WorkflowId] FOREIGN KEY ([WorkflowId]) REFERENCES [Workflows] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Invoices] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [UserSubscriptionId] int NULL,
    [InvoiceNumber] nvarchar(50) NOT NULL,
    [Status] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Tax] decimal(18,2) NOT NULL,
    [Total] decimal(18,2) NOT NULL,
    [InvoiceDate] datetime2 NOT NULL,
    [DueDate] datetime2 NULL,
    [PaidDate] datetime2 NULL,
    [StripeInvoiceId] nvarchar(100) NULL,
    [Description] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_Invoices] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Invoices_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Invoices_UserSubscriptions_UserSubscriptionId] FOREIGN KEY ([UserSubscriptionId]) REFERENCES [UserSubscriptions] ([Id]) ON DELETE SET NULL
);
GO

CREATE TABLE [URLClicks] (
    [Id] int NOT NULL IDENTITY,
    [URLShortenerId] int NOT NULL,
    [IpAddress] nvarchar(max) NULL,
    [UserAgent] nvarchar(max) NULL,
    [Referrer] nvarchar(max) NULL,
    [ClickedAt] datetime2 NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_URLClicks] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_URLClicks_URLShorteners_URLShortenerId] FOREIGN KEY ([URLShortenerId]) REFERENCES [URLShorteners] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [KeywordActivities] (
    [Id] int NOT NULL IDENTITY,
    [KeywordId] int NOT NULL,
    [PhoneNumber] nvarchar(max) NOT NULL,
    [IncomingMessage] nvarchar(max) NOT NULL,
    [ResponseSent] nvarchar(max) NULL,
    [ReceivedAt] datetime2 NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_KeywordActivities] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_KeywordActivities_Keywords_KeywordId] FOREIGN KEY ([KeywordId]) REFERENCES [Keywords] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [MessageDeliveryAttempts] (
    [Id] int NOT NULL IDENTITY,
    [CampaignMessageId] int NOT NULL,
    [AttemptNumber] int NOT NULL,
    [Channel] int NOT NULL,
    [ProviderName] nvarchar(100) NULL,
    [AttemptedAt] datetime2 NOT NULL,
    [Success] bit NOT NULL,
    [ExternalMessageId] nvarchar(200) NULL,
    [ErrorMessage] nvarchar(2000) NULL,
    [ErrorCode] nvarchar(100) NULL,
    [CostAmount] decimal(18,6) NULL,
    [ResponseTimeMs] int NOT NULL,
    [FallbackReason] int NULL,
    [AdditionalMetadata] nvarchar(4000) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_MessageDeliveryAttempts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MessageDeliveryAttempts_CampaignMessages_CampaignMessageId] FOREIGN KEY ([CampaignMessageId]) REFERENCES [CampaignMessages] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [BillingHistories] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [InvoiceId] int NULL,
    [Type] int NOT NULL,
    [Status] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Description] nvarchar(max) NULL,
    [StripeChargeId] nvarchar(max) NULL,
    [TransactionDate] datetime2 NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_BillingHistories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BillingHistories_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_BillingHistories_Invoices_InvoiceId] FOREIGN KEY ([InvoiceId]) REFERENCES [Invoices] ([Id])
);
GO

CREATE INDEX [IX_ApiRateLimits_EndpointPattern] ON [ApiRateLimits] ([EndpointPattern]);
GO

CREATE INDEX [IX_ApiRateLimits_TenantId] ON [ApiRateLimits] ([TenantId]);
GO

CREATE INDEX [IX_ApiRateLimits_TenantId_EndpointPattern] ON [ApiRateLimits] ([TenantId], [EndpointPattern]);
GO

CREATE INDEX [IX_ApiRateLimits_UserId] ON [ApiRateLimits] ([UserId]);
GO

CREATE INDEX [IX_ApiRateLimits_UserId_EndpointPattern] ON [ApiRateLimits] ([UserId], [EndpointPattern]);
GO

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO

CREATE INDEX [IX_BillingHistories_InvoiceId] ON [BillingHistories] ([InvoiceId]);
GO

CREATE INDEX [IX_BillingHistories_UserId] ON [BillingHistories] ([UserId]);
GO

CREATE UNIQUE INDEX [IX_CampaignAnalytics_CampaignId] ON [CampaignAnalytics] ([CampaignId]);
GO

CREATE UNIQUE INDEX [IX_CampaignAudiences_CampaignId] ON [CampaignAudiences] ([CampaignId]);
GO

CREATE UNIQUE INDEX [IX_CampaignContents_CampaignId] ON [CampaignContents] ([CampaignId]);
GO

CREATE INDEX [IX_CampaignContents_MessageTemplateId] ON [CampaignContents] ([MessageTemplateId]);
GO

CREATE INDEX [IX_CampaignMessages_CampaignId] ON [CampaignMessages] ([CampaignId]);
GO

CREATE INDEX [IX_CampaignMessages_ContactId] ON [CampaignMessages] ([ContactId]);
GO

CREATE INDEX [IX_CampaignMessages_Status] ON [CampaignMessages] ([Status]);
GO

CREATE INDEX [IX_Campaigns_CreatedAt] ON [Campaigns] ([CreatedAt]);
GO

CREATE INDEX [IX_Campaigns_ScheduledAt] ON [Campaigns] ([ScheduledAt]);
GO

CREATE INDEX [IX_Campaigns_Status] ON [Campaigns] ([Status]);
GO

CREATE INDEX [IX_Campaigns_UserId] ON [Campaigns] ([UserId]);
GO

CREATE UNIQUE INDEX [IX_CampaignSchedules_CampaignId] ON [CampaignSchedules] ([CampaignId]);
GO

CREATE INDEX [IX_ChannelRoutingConfigs_Channel] ON [ChannelRoutingConfigs] ([Channel]);
GO

CREATE INDEX [IX_ChannelRoutingConfigs_Channel_IsActive_Priority] ON [ChannelRoutingConfigs] ([Channel], [IsActive], [Priority]);
GO

CREATE INDEX [IX_ComplianceAuditLogs_CampaignId] ON [ComplianceAuditLogs] ([CampaignId]);
GO

CREATE INDEX [IX_ComplianceAuditLogs_ContactId] ON [ComplianceAuditLogs] ([ContactId]);
GO

CREATE INDEX [IX_ComplianceAuditLogs_UserId] ON [ComplianceAuditLogs] ([UserId]);
GO

CREATE INDEX [IX_ComplianceSettings_UserId] ON [ComplianceSettings] ([UserId]);
GO

CREATE INDEX [IX_ConsentHistories_ContactId] ON [ConsentHistories] ([ContactId]);
GO

CREATE INDEX [IX_ContactConsents_ContactId] ON [ContactConsents] ([ContactId]);
GO

CREATE UNIQUE INDEX [IX_ContactEngagements_ContactId] ON [ContactEngagements] ([ContactId]);
GO

CREATE INDEX [IX_ContactGroupMembers_ContactGroupId] ON [ContactGroupMembers] ([ContactGroupId]);
GO

CREATE INDEX [IX_ContactGroupMembers_ContactId] ON [ContactGroupMembers] ([ContactId]);
GO

CREATE UNIQUE INDEX [IX_ContactGroupMembers_ContactId_ContactGroupId] ON [ContactGroupMembers] ([ContactId], [ContactGroupId]);
GO

CREATE INDEX [IX_ContactGroups_Name] ON [ContactGroups] ([Name]);
GO

CREATE INDEX [IX_ContactGroups_UserId] ON [ContactGroups] ([UserId]);
GO

CREATE INDEX [IX_Contacts_CreatedAt] ON [Contacts] ([CreatedAt]);
GO

CREATE INDEX [IX_Contacts_Email] ON [Contacts] ([Email]);
GO

CREATE INDEX [IX_Contacts_PhoneNumber] ON [Contacts] ([PhoneNumber]);
GO

CREATE INDEX [IX_Contacts_UserId] ON [Contacts] ([UserId]);
GO

CREATE INDEX [IX_ContactTagAssignments_ContactId] ON [ContactTagAssignments] ([ContactId]);
GO

CREATE UNIQUE INDEX [IX_ContactTagAssignments_ContactId_ContactTagId] ON [ContactTagAssignments] ([ContactId], [ContactTagId]);
GO

CREATE INDEX [IX_ContactTagAssignments_ContactTagId] ON [ContactTagAssignments] ([ContactTagId]);
GO

CREATE INDEX [IX_ContactTags_UserId] ON [ContactTags] ([UserId]);
GO

CREATE INDEX [IX_CustomRoles_IsActive] ON [CustomRoles] ([IsActive]);
GO

CREATE UNIQUE INDEX [IX_CustomRoles_Name] ON [CustomRoles] ([Name]);
GO

CREATE INDEX [IX_CustomUserRoles_RoleId] ON [CustomUserRoles] ([RoleId]);
GO

CREATE INDEX [IX_CustomUserRoles_UserId] ON [CustomUserRoles] ([UserId]);
GO

CREATE INDEX [IX_FrequencyControls_ContactId] ON [FrequencyControls] ([ContactId]);
GO

CREATE UNIQUE INDEX [IX_FrequencyControls_ContactId_UserId] ON [FrequencyControls] ([ContactId], [UserId]);
GO

CREATE INDEX [IX_FrequencyControls_UserId] ON [FrequencyControls] ([UserId]);
GO

CREATE UNIQUE INDEX [IX_Invoices_InvoiceNumber] ON [Invoices] ([InvoiceNumber]);
GO

CREATE INDEX [IX_Invoices_Status] ON [Invoices] ([Status]);
GO

CREATE INDEX [IX_Invoices_UserId] ON [Invoices] ([UserId]);
GO

CREATE INDEX [IX_Invoices_UserSubscriptionId] ON [Invoices] ([UserSubscriptionId]);
GO

CREATE INDEX [IX_KeywordActivities_KeywordId] ON [KeywordActivities] ([KeywordId]);
GO

CREATE INDEX [IX_Keywords_KeywordText] ON [Keywords] ([KeywordText]);
GO

CREATE INDEX [IX_Keywords_LinkedCampaignId] ON [Keywords] ([LinkedCampaignId]);
GO

CREATE INDEX [IX_Keywords_OptInGroupId] ON [Keywords] ([OptInGroupId]);
GO

CREATE INDEX [IX_Keywords_UserId] ON [Keywords] ([UserId]);
GO

CREATE INDEX [IX_MessageDeliveryAttempts_AttemptedAt] ON [MessageDeliveryAttempts] ([AttemptedAt]);
GO

CREATE INDEX [IX_MessageDeliveryAttempts_CampaignMessageId] ON [MessageDeliveryAttempts] ([CampaignMessageId]);
GO

CREATE INDEX [IX_MessageDeliveryAttempts_CampaignMessageId_AttemptNumber] ON [MessageDeliveryAttempts] ([CampaignMessageId], [AttemptNumber]);
GO

CREATE INDEX [IX_MessageTemplates_Category] ON [MessageTemplates] ([Category]);
GO

CREATE INDEX [IX_MessageTemplates_Channel] ON [MessageTemplates] ([Channel]);
GO

CREATE INDEX [IX_MessageTemplates_UserId] ON [MessageTemplates] ([UserId]);
GO

CREATE INDEX [IX_ProviderLogs_MessageProviderId] ON [ProviderLogs] ([MessageProviderId]);
GO

CREATE INDEX [IX_ProviderRateLimits_ProviderName] ON [ProviderRateLimits] ([ProviderName]);
GO

CREATE INDEX [IX_ProviderRateLimits_ProviderName_ProviderType] ON [ProviderRateLimits] ([ProviderName], [ProviderType]);
GO

CREATE INDEX [IX_ProviderRateLimits_ProviderType] ON [ProviderRateLimits] ([ProviderType]);
GO

CREATE INDEX [IX_ProviderRateLimits_UserId_ProviderName] ON [ProviderRateLimits] ([UserId], [ProviderName]);
GO

CREATE INDEX [IX_RateLimitLogs_TenantId] ON [RateLimitLogs] ([TenantId]);
GO

CREATE INDEX [IX_RateLimitLogs_TriggeredAt] ON [RateLimitLogs] ([TriggeredAt]);
GO

CREATE INDEX [IX_RateLimitLogs_UserId] ON [RateLimitLogs] ([UserId]);
GO

CREATE INDEX [IX_RateLimitLogs_UserId_TriggeredAt] ON [RateLimitLogs] ([UserId], [TriggeredAt]);
GO

CREATE INDEX [IX_RefreshTokens_UserId] ON [RefreshTokens] ([UserId]);
GO

CREATE INDEX [IX_SubscriptionPlans_Name] ON [SubscriptionPlans] ([Name]);
GO

CREATE INDEX [IX_SuppressionLists_UserId] ON [SuppressionLists] ([UserId]);
GO

CREATE INDEX [IX_URLClicks_URLShortenerId] ON [URLClicks] ([URLShortenerId]);
GO

CREATE INDEX [IX_URLShorteners_CampaignId] ON [URLShorteners] ([CampaignId]);
GO

CREATE UNIQUE INDEX [IX_URLShorteners_ShortCode] ON [URLShorteners] ([ShortCode]);
GO

CREATE INDEX [IX_UsageTrackings_UserId] ON [UsageTrackings] ([UserId]);
GO

CREATE UNIQUE INDEX [IX_UsageTrackings_UserId_Year_Month] ON [UsageTrackings] ([UserId], [Year], [Month]);
GO

CREATE INDEX [IX_UserActivityLogs_UserId] ON [UserActivityLogs] ([UserId]);
GO

CREATE INDEX [IX_UserSubscriptions_Status] ON [UserSubscriptions] ([Status]);
GO

CREATE INDEX [IX_UserSubscriptions_SubscriptionPlanId] ON [UserSubscriptions] ([SubscriptionPlanId]);
GO

CREATE INDEX [IX_UserSubscriptions_UserId] ON [UserSubscriptions] ([UserId]);
GO

CREATE INDEX [IX_WorkflowExecutions_WorkflowId] ON [WorkflowExecutions] ([WorkflowId]);
GO

CREATE INDEX [IX_Workflows_UserId] ON [Workflows] ([UserId]);
GO

CREATE INDEX [IX_WorkflowSteps_WorkflowId] ON [WorkflowSteps] ([WorkflowId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260118181947_AddApiRateLimiting', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ComplianceAuditLogs]') AND [c].[name] = N'AdditionalSettings');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [ComplianceAuditLogs] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [ComplianceAuditLogs] DROP COLUMN [AdditionalSettings];
GO

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ComplianceAuditLogs]') AND [c].[name] = N'CostThreshold');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [ComplianceAuditLogs] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [ComplianceAuditLogs] DROP COLUMN [CostThreshold];
GO

ALTER TABLE [Campaigns] ADD [ABTestEndDate] datetime2 NULL;
GO

ALTER TABLE [Campaigns] ADD [IsABTest] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Campaigns] ADD [WinningVariantId] int NULL;
GO

ALTER TABLE [CampaignMessages] ADD [VariantId] int NULL;
GO

CREATE TABLE [CampaignVariants] (
    [Id] int NOT NULL IDENTITY,
    [CampaignId] int NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Description] nvarchar(1000) NULL,
    [TrafficPercentage] decimal(5,2) NOT NULL,
    [IsControl] bit NOT NULL,
    [IsActive] bit NOT NULL,
    [Channel] int NOT NULL,
    [Subject] nvarchar(500) NULL,
    [MessageBody] nvarchar(max) NULL,
    [HTMLContent] nvarchar(max) NULL,
    [MediaUrls] nvarchar(max) NULL,
    [MessageTemplateId] int NULL,
    [PersonalizationTokens] nvarchar(max) NULL,
    [RecipientCount] int NOT NULL,
    [SentCount] int NOT NULL,
    [DeliveredCount] int NOT NULL,
    [FailedCount] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_CampaignVariants] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CampaignVariants_Campaigns_CampaignId] FOREIGN KEY ([CampaignId]) REFERENCES [Campaigns] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_CampaignVariants_MessageTemplates_MessageTemplateId] FOREIGN KEY ([MessageTemplateId]) REFERENCES [MessageTemplates] ([Id]) ON DELETE SET NULL
);
GO

CREATE TABLE [CampaignVariantAnalytics] (
    [Id] int NOT NULL IDENTITY,
    [CampaignVariantId] int NOT NULL,
    [TotalSent] int NOT NULL,
    [TotalDelivered] int NOT NULL,
    [TotalFailed] int NOT NULL,
    [TotalClicks] int NOT NULL,
    [TotalOptOuts] int NOT NULL,
    [TotalBounces] int NOT NULL,
    [TotalOpens] int NOT NULL,
    [TotalReplies] int NOT NULL,
    [TotalConversions] int NOT NULL,
    [DeliveryRate] decimal(5,2) NOT NULL,
    [ClickRate] decimal(5,2) NOT NULL,
    [OptOutRate] decimal(5,2) NOT NULL,
    [OpenRate] decimal(5,2) NOT NULL,
    [BounceRate] decimal(5,2) NOT NULL,
    [ConversionRate] decimal(5,2) NOT NULL,
    [ConfidenceLevel] decimal(5,2) NULL,
    [IsStatisticallySignificant] bit NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_CampaignVariantAnalytics] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CampaignVariantAnalytics_CampaignVariants_CampaignVariantId] FOREIGN KEY ([CampaignVariantId]) REFERENCES [CampaignVariants] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_CampaignMessages_VariantId] ON [CampaignMessages] ([VariantId]);
GO

CREATE UNIQUE INDEX [IX_CampaignVariantAnalytics_CampaignVariantId] ON [CampaignVariantAnalytics] ([CampaignVariantId]);
GO

CREATE INDEX [IX_CampaignVariants_CampaignId] ON [CampaignVariants] ([CampaignId]);
GO

CREATE INDEX [IX_CampaignVariants_CampaignId_IsControl] ON [CampaignVariants] ([CampaignId], [IsControl]);
GO

CREATE INDEX [IX_CampaignVariants_MessageTemplateId] ON [CampaignVariants] ([MessageTemplateId]);
GO

ALTER TABLE [CampaignMessages] ADD CONSTRAINT [FK_CampaignMessages_CampaignVariants_VariantId] FOREIGN KEY ([VariantId]) REFERENCES [CampaignVariants] ([Id]) ON DELETE SET NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260118182817_AddCampaignABTesting', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [WorkflowSteps] ADD [BranchCondition] nvarchar(max) NULL;
GO

ALTER TABLE [WorkflowSteps] ADD [NextNodeOnFalse] int NULL;
GO

ALTER TABLE [WorkflowSteps] ADD [NextNodeOnTrue] int NULL;
GO

ALTER TABLE [WorkflowSteps] ADD [NodeLabel] nvarchar(200) NULL;
GO

ALTER TABLE [WorkflowSteps] ADD [PositionX] float NULL;
GO

ALTER TABLE [WorkflowSteps] ADD [PositionY] float NULL;
GO

CREATE INDEX [IX_WorkflowSteps_WorkflowId_StepOrder] ON [WorkflowSteps] ([WorkflowId], [StepOrder]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260118183433_AddJourneyDesignerFields', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [ExternalAuthProviders] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [DisplayName] nvarchar(max) NOT NULL,
    [ProviderType] nvarchar(max) NOT NULL,
    [ClientId] nvarchar(max) NOT NULL,
    [ClientSecret] nvarchar(max) NOT NULL,
    [Authority] nvarchar(max) NOT NULL,
    [TenantId] nvarchar(max) NULL,
    [Domain] nvarchar(max) NULL,
    [Region] nvarchar(max) NULL,
    [UserPoolId] nvarchar(max) NULL,
    [CallbackPath] nvarchar(max) NOT NULL,
    [Scopes] nvarchar(max) NOT NULL,
    [IsEnabled] bit NOT NULL,
    [IsDefault] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [ConfigurationJson] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_ExternalAuthProviders] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [FileStorageSettings] (
    [Id] int NOT NULL IDENTITY,
    [ProviderName] nvarchar(max) NOT NULL,
    [ConnectionString] nvarchar(max) NULL,
    [ContainerName] nvarchar(max) NULL,
    [BucketName] nvarchar(max) NULL,
    [Region] nvarchar(max) NULL,
    [AccessKey] nvarchar(max) NULL,
    [SecretKey] nvarchar(max) NULL,
    [LocalBasePath] nvarchar(max) NULL,
    [IsEnabled] bit NOT NULL,
    [IsDefault] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [ConfigurationJson] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_FileStorageSettings] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [UserExternalLogins] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ProviderId] int NOT NULL,
    [ProviderUserId] nvarchar(max) NOT NULL,
    [ProviderUserName] nvarchar(max) NOT NULL,
    [ProviderEmail] nvarchar(max) NULL,
    [AccessToken] nvarchar(max) NULL,
    [RefreshToken] nvarchar(max) NULL,
    [TokenExpiresAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [LastLoginAt] datetime2 NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_UserExternalLogins] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserExternalLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserExternalLogins_ExternalAuthProviders_ProviderId] FOREIGN KEY ([ProviderId]) REFERENCES [ExternalAuthProviders] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_UserExternalLogins_ProviderId] ON [UserExternalLogins] ([ProviderId]);
GO

CREATE INDEX [IX_UserExternalLogins_UserId] ON [UserExternalLogins] ([UserId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260118185628_AddOAuth2SSOEntities', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [EncryptionAuditLogs] (
    [Id] int NOT NULL IDENTITY,
    [Operation] nvarchar(50) NOT NULL,
    [EntityType] nvarchar(100) NOT NULL,
    [EntityId] int NULL,
    [FieldName] nvarchar(100) NULL,
    [KeyVersion] nvarchar(20) NOT NULL,
    [UserId] nvarchar(450) NULL,
    [Success] bit NOT NULL,
    [ErrorMessage] nvarchar(1000) NULL,
    [IpAddress] nvarchar(50) NULL,
    [OperationTimestamp] datetime2 NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_EncryptionAuditLogs] PRIMARY KEY ([Id])
);
GO

CREATE INDEX [IX_EncryptionAuditLogs_EntityId] ON [EncryptionAuditLogs] ([EntityId]);
GO

CREATE INDEX [IX_EncryptionAuditLogs_EntityType] ON [EncryptionAuditLogs] ([EntityType]);
GO

CREATE INDEX [IX_EncryptionAuditLogs_Operation] ON [EncryptionAuditLogs] ([Operation]);
GO

CREATE INDEX [IX_EncryptionAuditLogs_OperationTimestamp] ON [EncryptionAuditLogs] ([OperationTimestamp]);
GO

CREATE INDEX [IX_EncryptionAuditLogs_UserId] ON [EncryptionAuditLogs] ([UserId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260118190656_AddEncryptionAuditLog', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [ComplianceRules] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(200) NOT NULL,
    [Description] nvarchar(2000) NOT NULL,
    [RuleType] int NOT NULL,
    [Status] int NOT NULL,
    [Configuration] nvarchar(max) NOT NULL,
    [Priority] int NOT NULL,
    [IsMandatory] bit NOT NULL,
    [EffectiveFrom] datetime2 NOT NULL,
    [EffectiveTo] datetime2 NULL,
    [ApplicableRegions] nvarchar(1000) NULL,
    [ApplicableServices] nvarchar(1000) NULL,
    [CreatedBy] nvarchar(450) NOT NULL,
    [ModifiedBy] nvarchar(450) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_ComplianceRules] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [FeatureToggles] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(200) NOT NULL,
    [DisplayName] nvarchar(200) NOT NULL,
    [Description] nvarchar(1000) NULL,
    [Status] int NOT NULL,
    [IsEnabled] bit NOT NULL,
    [EnabledForRoles] nvarchar(500) NULL,
    [EnabledForUsers] nvarchar(2000) NULL,
    [Category] nvarchar(100) NULL,
    [EnableAfter] datetime2 NULL,
    [DisableAfter] datetime2 NULL,
    [ModifiedBy] nvarchar(450) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_FeatureToggles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [PlatformSettings] (
    [Id] int NOT NULL IDENTITY,
    [Key] nvarchar(200) NOT NULL,
    [Value] nvarchar(4000) NOT NULL,
    [DataType] int NOT NULL,
    [Scope] int NOT NULL,
    [Description] nvarchar(1000) NULL,
    [Category] nvarchar(100) NULL,
    [IsEncrypted] bit NOT NULL,
    [IsReadOnly] bit NOT NULL,
    [DefaultValue] nvarchar(4000) NULL,
    [ModifiedBy] nvarchar(450) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_PlatformSettings] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [ComplianceRuleAudits] (
    [Id] int NOT NULL IDENTITY,
    [ComplianceRuleId] int NOT NULL,
    [Action] int NOT NULL,
    [PerformedBy] nvarchar(450) NOT NULL,
    [PreviousState] nvarchar(max) NULL,
    [NewState] nvarchar(max) NULL,
    [Reason] nvarchar(1000) NULL,
    [IpAddress] nvarchar(50) NULL,
    [Metadata] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_ComplianceRuleAudits] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ComplianceRuleAudits_ComplianceRules_ComplianceRuleId] FOREIGN KEY ([ComplianceRuleId]) REFERENCES [ComplianceRules] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_ComplianceRuleAudits_Action] ON [ComplianceRuleAudits] ([Action]);
GO

CREATE INDEX [IX_ComplianceRuleAudits_ComplianceRuleId] ON [ComplianceRuleAudits] ([ComplianceRuleId]);
GO

CREATE INDEX [IX_ComplianceRuleAudits_ComplianceRuleId_CreatedAt] ON [ComplianceRuleAudits] ([ComplianceRuleId], [CreatedAt]);
GO

CREATE INDEX [IX_ComplianceRuleAudits_CreatedAt] ON [ComplianceRuleAudits] ([CreatedAt]);
GO

CREATE INDEX [IX_ComplianceRules_IsDeleted_Status] ON [ComplianceRules] ([IsDeleted], [Status]);
GO

CREATE INDEX [IX_ComplianceRules_Priority] ON [ComplianceRules] ([Priority]);
GO

CREATE INDEX [IX_ComplianceRules_RuleType] ON [ComplianceRules] ([RuleType]);
GO

CREATE INDEX [IX_ComplianceRules_Status] ON [ComplianceRules] ([Status]);
GO

CREATE INDEX [IX_ComplianceRules_Status_EffectiveFrom_EffectiveTo] ON [ComplianceRules] ([Status], [EffectiveFrom], [EffectiveTo]);
GO

CREATE INDEX [IX_FeatureToggles_Category] ON [FeatureToggles] ([Category]);
GO

CREATE INDEX [IX_FeatureToggles_IsDeleted_IsEnabled] ON [FeatureToggles] ([IsDeleted], [IsEnabled]);
GO

CREATE UNIQUE INDEX [IX_FeatureToggles_Name] ON [FeatureToggles] ([Name]);
GO

CREATE INDEX [IX_FeatureToggles_Status] ON [FeatureToggles] ([Status]);
GO

CREATE INDEX [IX_PlatformSettings_Category] ON [PlatformSettings] ([Category]);
GO

CREATE INDEX [IX_PlatformSettings_IsDeleted_Category] ON [PlatformSettings] ([IsDeleted], [Category]);
GO

CREATE UNIQUE INDEX [IX_PlatformSettings_Key] ON [PlatformSettings] ([Key]);
GO

CREATE INDEX [IX_PlatformSettings_Scope] ON [PlatformSettings] ([Scope]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260118195158_AddGlobalConfigurationManagement', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [PlatformConfigurations] (
    [Id] int NOT NULL IDENTITY,
    [Key] nvarchar(200) NOT NULL,
    [Value] nvarchar(4000) NOT NULL,
    [Category] int NOT NULL,
    [DataType] nvarchar(50) NOT NULL,
    [Description] nvarchar(1000) NOT NULL,
    [IsEncrypted] bit NOT NULL,
    [IsActive] bit NOT NULL,
    [LastModifiedBy] nvarchar(450) NULL,
    [LastModifiedAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_PlatformConfigurations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PlatformConfigurations_AspNetUsers_LastModifiedBy] FOREIGN KEY ([LastModifiedBy]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [PrivilegedActionLogs] (
    [Id] int NOT NULL IDENTITY,
    [ActionType] int NOT NULL,
    [Severity] int NOT NULL,
    [PerformedBy] nvarchar(450) NOT NULL,
    [EntityType] nvarchar(100) NULL,
    [EntityId] nvarchar(450) NULL,
    [EntityName] nvarchar(500) NULL,
    [ActionDescription] nvarchar(2000) NOT NULL,
    [BeforeState] nvarchar(max) NULL,
    [AfterState] nvarchar(max) NULL,
    [IPAddress] nvarchar(45) NULL,
    [UserAgent] nvarchar(500) NULL,
    [RequestPath] nvarchar(500) NULL,
    [Success] bit NOT NULL,
    [ErrorMessage] nvarchar(2000) NULL,
    [Timestamp] datetime2 NOT NULL,
    [Metadata] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_PrivilegedActionLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PrivilegedActionLogs_AspNetUsers_PerformedBy] FOREIGN KEY ([PerformedBy]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [SuperAdminRoles] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [AssignedAt] datetime2 NOT NULL,
    [AssignedBy] nvarchar(450) NULL,
    [AssignmentReason] nvarchar(1000) NOT NULL,
    [RevokedAt] datetime2 NULL,
    [RevokedBy] nvarchar(450) NULL,
    [RevocationReason] nvarchar(1000) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_SuperAdminRoles] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SuperAdminRoles_AspNetUsers_AssignedBy] FOREIGN KEY ([AssignedBy]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_SuperAdminRoles_AspNetUsers_RevokedBy] FOREIGN KEY ([RevokedBy]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_SuperAdminRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_PlatformConfigurations_Category] ON [PlatformConfigurations] ([Category]);
GO

CREATE INDEX [IX_PlatformConfigurations_IsActive] ON [PlatformConfigurations] ([IsActive]);
GO

CREATE UNIQUE INDEX [IX_PlatformConfigurations_Key] ON [PlatformConfigurations] ([Key]);
GO

CREATE INDEX [IX_PlatformConfigurations_LastModifiedBy] ON [PlatformConfigurations] ([LastModifiedBy]);
GO

CREATE INDEX [IX_PrivilegedActionLogs_ActionType] ON [PrivilegedActionLogs] ([ActionType]);
GO

CREATE INDEX [IX_PrivilegedActionLogs_EntityType] ON [PrivilegedActionLogs] ([EntityType]);
GO

CREATE INDEX [IX_PrivilegedActionLogs_EntityType_EntityId] ON [PrivilegedActionLogs] ([EntityType], [EntityId]);
GO

CREATE INDEX [IX_PrivilegedActionLogs_PerformedBy] ON [PrivilegedActionLogs] ([PerformedBy]);
GO

CREATE INDEX [IX_PrivilegedActionLogs_Severity] ON [PrivilegedActionLogs] ([Severity]);
GO

CREATE INDEX [IX_PrivilegedActionLogs_Timestamp] ON [PrivilegedActionLogs] ([Timestamp]);
GO

CREATE INDEX [IX_SuperAdminRoles_AssignedAt] ON [SuperAdminRoles] ([AssignedAt]);
GO

CREATE INDEX [IX_SuperAdminRoles_AssignedBy] ON [SuperAdminRoles] ([AssignedBy]);
GO

CREATE INDEX [IX_SuperAdminRoles_IsActive] ON [SuperAdminRoles] ([IsActive]);
GO

CREATE INDEX [IX_SuperAdminRoles_RevokedBy] ON [SuperAdminRoles] ([RevokedBy]);
GO

CREATE INDEX [IX_SuperAdminRoles_UserId] ON [SuperAdminRoles] ([UserId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260118195946_AddSuperAdminAndPrivilegedLogging', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [KeywordAssignments] (
    [Id] int NOT NULL IDENTITY,
    [KeywordId] int NOT NULL,
    [CampaignId] int NOT NULL,
    [AssignedByUserId] nvarchar(450) NOT NULL,
    [AssignedAt] datetime2 NOT NULL,
    [UnassignedAt] datetime2 NULL,
    [IsActive] bit NOT NULL,
    [Notes] nvarchar(1000) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_KeywordAssignments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_KeywordAssignments_AspNetUsers_AssignedByUserId] FOREIGN KEY ([AssignedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_KeywordAssignments_Campaigns_CampaignId] FOREIGN KEY ([CampaignId]) REFERENCES [Campaigns] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_KeywordAssignments_Keywords_KeywordId] FOREIGN KEY ([KeywordId]) REFERENCES [Keywords] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [KeywordConflicts] (
    [Id] int NOT NULL IDENTITY,
    [KeywordText] nvarchar(50) NOT NULL,
    [RequestingUserId] nvarchar(450) NOT NULL,
    [ExistingUserId] nvarchar(450) NOT NULL,
    [ResolvedByUserId] nvarchar(450) NULL,
    [Status] int NOT NULL,
    [ResolvedAt] datetime2 NULL,
    [Resolution] nvarchar(1000) NULL,
    [Notes] nvarchar(1000) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_KeywordConflicts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_KeywordConflicts_AspNetUsers_ExistingUserId] FOREIGN KEY ([ExistingUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_KeywordConflicts_AspNetUsers_RequestingUserId] FOREIGN KEY ([RequestingUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_KeywordConflicts_AspNetUsers_ResolvedByUserId] FOREIGN KEY ([ResolvedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [KeywordReservations] (
    [Id] int NOT NULL IDENTITY,
    [KeywordText] nvarchar(50) NOT NULL,
    [RequestedByUserId] nvarchar(450) NOT NULL,
    [ApprovedByUserId] nvarchar(450) NULL,
    [Status] int NOT NULL,
    [Purpose] nvarchar(500) NULL,
    [ExpiresAt] datetime2 NULL,
    [ApprovedAt] datetime2 NULL,
    [RejectionReason] nvarchar(500) NULL,
    [Priority] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_KeywordReservations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_KeywordReservations_AspNetUsers_ApprovedByUserId] FOREIGN KEY ([ApprovedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_KeywordReservations_AspNetUsers_RequestedByUserId] FOREIGN KEY ([RequestedByUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [PricingModels] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(200) NOT NULL,
    [Description] nvarchar(1000) NULL,
    [Type] int NOT NULL,
    [BasePrice] decimal(18,2) NOT NULL,
    [BillingPeriod] int NOT NULL,
    [IsActive] bit NOT NULL,
    [Configuration] nvarchar(max) NULL,
    [Priority] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_PricingModels] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [TaxConfigurations] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(200) NOT NULL,
    [Type] int NOT NULL,
    [RegionCode] nvarchar(10) NULL,
    [Rate] decimal(10,4) NOT NULL,
    [FlatAmount] decimal(18,2) NULL,
    [IsPercentage] bit NOT NULL,
    [IsActive] bit NOT NULL,
    [Priority] int NOT NULL,
    [Configuration] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_TaxConfigurations] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [ChannelPricings] (
    [Id] int NOT NULL IDENTITY,
    [PricingModelId] int NOT NULL,
    [Channel] int NOT NULL,
    [PricePerUnit] decimal(18,4) NOT NULL,
    [MinimumCharge] decimal(18,2) NULL,
    [FreeUnitsIncluded] int NULL,
    [IsActive] bit NOT NULL,
    [Configuration] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_ChannelPricings] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ChannelPricings_PricingModels_PricingModelId] FOREIGN KEY ([PricingModelId]) REFERENCES [PricingModels] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [RegionPricings] (
    [Id] int NOT NULL IDENTITY,
    [PricingModelId] int NOT NULL,
    [RegionCode] nvarchar(10) NOT NULL,
    [RegionName] nvarchar(200) NOT NULL,
    [PriceMultiplier] decimal(10,4) NOT NULL,
    [FlatAdjustment] decimal(18,2) NULL,
    [IsActive] bit NOT NULL,
    [Configuration] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_RegionPricings] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RegionPricings_PricingModels_PricingModelId] FOREIGN KEY ([PricingModelId]) REFERENCES [PricingModels] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [UsagePricings] (
    [Id] int NOT NULL IDENTITY,
    [PricingModelId] int NOT NULL,
    [Type] int NOT NULL,
    [TierStart] int NOT NULL,
    [TierEnd] int NULL,
    [PricePerUnit] decimal(18,4) NOT NULL,
    [IsActive] bit NOT NULL,
    [Configuration] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_UsagePricings] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UsagePricings_PricingModels_PricingModelId] FOREIGN KEY ([PricingModelId]) REFERENCES [PricingModels] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_ChannelPricings_IsActive] ON [ChannelPricings] ([IsActive]);
GO

CREATE INDEX [IX_ChannelPricings_PricingModelId_Channel] ON [ChannelPricings] ([PricingModelId], [Channel]);
GO

CREATE INDEX [IX_KeywordAssignments_AssignedByUserId] ON [KeywordAssignments] ([AssignedByUserId]);
GO

CREATE INDEX [IX_KeywordAssignments_CampaignId] ON [KeywordAssignments] ([CampaignId]);
GO

CREATE INDEX [IX_KeywordAssignments_IsActive] ON [KeywordAssignments] ([IsActive]);
GO

CREATE INDEX [IX_KeywordAssignments_KeywordId] ON [KeywordAssignments] ([KeywordId]);
GO

CREATE INDEX [IX_KeywordConflicts_ExistingUserId] ON [KeywordConflicts] ([ExistingUserId]);
GO

CREATE INDEX [IX_KeywordConflicts_KeywordText] ON [KeywordConflicts] ([KeywordText]);
GO

CREATE INDEX [IX_KeywordConflicts_RequestingUserId] ON [KeywordConflicts] ([RequestingUserId]);
GO

CREATE INDEX [IX_KeywordConflicts_ResolvedByUserId] ON [KeywordConflicts] ([ResolvedByUserId]);
GO

CREATE INDEX [IX_KeywordConflicts_Status] ON [KeywordConflicts] ([Status]);
GO

CREATE INDEX [IX_KeywordReservations_ApprovedByUserId] ON [KeywordReservations] ([ApprovedByUserId]);
GO

CREATE INDEX [IX_KeywordReservations_KeywordText] ON [KeywordReservations] ([KeywordText]);
GO

CREATE INDEX [IX_KeywordReservations_RequestedByUserId] ON [KeywordReservations] ([RequestedByUserId]);
GO

CREATE INDEX [IX_KeywordReservations_Status] ON [KeywordReservations] ([Status]);
GO

CREATE INDEX [IX_PricingModels_IsActive] ON [PricingModels] ([IsActive]);
GO

CREATE INDEX [IX_PricingModels_Name] ON [PricingModels] ([Name]);
GO

CREATE INDEX [IX_PricingModels_Priority] ON [PricingModels] ([Priority]);
GO

CREATE INDEX [IX_RegionPricings_IsActive] ON [RegionPricings] ([IsActive]);
GO

CREATE INDEX [IX_RegionPricings_PricingModelId_RegionCode] ON [RegionPricings] ([PricingModelId], [RegionCode]);
GO

CREATE INDEX [IX_TaxConfigurations_IsActive] ON [TaxConfigurations] ([IsActive]);
GO

CREATE INDEX [IX_TaxConfigurations_Name] ON [TaxConfigurations] ([Name]);
GO

CREATE INDEX [IX_TaxConfigurations_Priority] ON [TaxConfigurations] ([Priority]);
GO

CREATE INDEX [IX_TaxConfigurations_RegionCode] ON [TaxConfigurations] ([RegionCode]);
GO

CREATE INDEX [IX_TaxConfigurations_Type] ON [TaxConfigurations] ([Type]);
GO

CREATE INDEX [IX_UsagePricings_IsActive] ON [UsagePricings] ([IsActive]);
GO

CREATE INDEX [IX_UsagePricings_PricingModelId_Type_TierStart] ON [UsagePricings] ([PricingModelId], [Type], [TierStart]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260118201650_AddKeywordManagementAndPricingFeatures', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Keywords] DROP CONSTRAINT [FK_Keywords_Campaigns_LinkedCampaignId];
GO

DROP TABLE [PlatformConfiguration];
GO

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PlatformConfigurations]') AND [c].[name] = N'DefaultValue');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [PlatformConfigurations] DROP CONSTRAINT [' + @var5 + '];');
ALTER TABLE [PlatformConfigurations] DROP COLUMN [DefaultValue];
GO

DECLARE @var6 sysname;
SELECT @var6 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PlatformConfigurations]') AND [c].[name] = N'IsReadOnly');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [PlatformConfigurations] DROP CONSTRAINT [' + @var6 + '];');
ALTER TABLE [PlatformConfigurations] DROP COLUMN [IsReadOnly];
GO

DECLARE @var7 sysname;
SELECT @var7 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PlatformConfigurations]') AND [c].[name] = N'ModifiedBy');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [PlatformConfigurations] DROP CONSTRAINT [' + @var7 + '];');
ALTER TABLE [PlatformConfigurations] DROP COLUMN [ModifiedBy];
GO

DECLARE @var8 sysname;
SELECT @var8 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PlatformConfigurations]') AND [c].[name] = N'Scope');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [PlatformConfigurations] DROP CONSTRAINT [' + @var8 + '];');
ALTER TABLE [PlatformConfigurations] DROP COLUMN [Scope];
GO

ALTER TABLE [UserSubscriptions] ADD [PayPalCustomerId] nvarchar(max) NULL;
GO

ALTER TABLE [UserSubscriptions] ADD [PayPalSubscriptionId] nvarchar(max) NULL;
GO

ALTER TABLE [UserSubscriptions] ADD [PaymentProvider] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [SubscriptionPlans] ADD [IsVisible] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [SubscriptionPlans] ADD [PayPalPlanIdMonthly] nvarchar(max) NULL;
GO

ALTER TABLE [SubscriptionPlans] ADD [PayPalPlanIdYearly] nvarchar(max) NULL;
GO

ALTER TABLE [SubscriptionPlans] ADD [PayPalProductId] nvarchar(max) NULL;
GO

ALTER TABLE [SubscriptionPlans] ADD [ShowOnLanding] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

DECLARE @var9 sysname;
SELECT @var9 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PlatformConfigurations]') AND [c].[name] = N'Description');
IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [PlatformConfigurations] DROP CONSTRAINT [' + @var9 + '];');
UPDATE [PlatformConfigurations] SET [Description] = N'' WHERE [Description] IS NULL;
ALTER TABLE [PlatformConfigurations] ALTER COLUMN [Description] nvarchar(1000) NOT NULL;
ALTER TABLE [PlatformConfigurations] ADD DEFAULT N'' FOR [Description];
GO

DECLARE @var10 sysname;
SELECT @var10 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PlatformConfigurations]') AND [c].[name] = N'DataType');
IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [PlatformConfigurations] DROP CONSTRAINT [' + @var10 + '];');
ALTER TABLE [PlatformConfigurations] ALTER COLUMN [DataType] nvarchar(50) NOT NULL;
GO

ALTER TABLE [Invoices] ADD [PayPalInvoiceId] nvarchar(max) NULL;
GO

ALTER TABLE [Invoices] ADD [PaymentProvider] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [BillingHistories] ADD [PayPalTransactionId] nvarchar(max) NULL;
GO

ALTER TABLE [BillingHistories] ADD [PaymentProvider] int NOT NULL DEFAULT 0;
GO

CREATE TABLE [PageContents] (
    [Id] int NOT NULL IDENTITY,
    [PageKey] nvarchar(max) NOT NULL,
    [Title] nvarchar(max) NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [MetaDescription] nvarchar(max) NULL,
    [ImageUrls] nvarchar(max) NULL,
    [IsPublished] bit NOT NULL,
    [LastModifiedBy] nvarchar(max) NULL,
    [LastModifiedByUserId] nvarchar(450) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_PageContents] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PageContents_AspNetUsers_LastModifiedByUserId] FOREIGN KEY ([LastModifiedByUserId]) REFERENCES [AspNetUsers] ([Id])
);
GO

CREATE TABLE [PlatformSettings] (
    [Id] int NOT NULL IDENTITY,
    [Key] nvarchar(200) NOT NULL,
    [Value] nvarchar(4000) NOT NULL,
    [DataType] int NOT NULL,
    [Scope] int NOT NULL,
    [Description] nvarchar(1000) NULL,
    [Category] nvarchar(100) NULL,
    [IsEncrypted] bit NOT NULL,
    [IsReadOnly] bit NOT NULL,
    [DefaultValue] nvarchar(4000) NULL,
    [ModifiedBy] nvarchar(450) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_PlatformSettings] PRIMARY KEY ([Id])
);
GO

CREATE INDEX [IX_PageContents_LastModifiedByUserId] ON [PageContents] ([LastModifiedByUserId]);
GO

CREATE INDEX [IX_PlatformSettings_Category] ON [PlatformSettings] ([Category]);
GO

CREATE INDEX [IX_PlatformSettings_IsDeleted_Category] ON [PlatformSettings] ([IsDeleted], [Category]);
GO

CREATE UNIQUE INDEX [IX_PlatformSettings_Key] ON [PlatformSettings] ([Key]);
GO

CREATE INDEX [IX_PlatformSettings_Scope] ON [PlatformSettings] ([Scope]);
GO

ALTER TABLE [Keywords] ADD CONSTRAINT [FK_Keywords_Campaigns_LinkedCampaignId] FOREIGN KEY ([LinkedCampaignId]) REFERENCES [Campaigns] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [PlatformConfigurations] ADD CONSTRAINT [FK_PlatformConfigurations_AspNetUsers_LastModifiedBy] FOREIGN KEY ([LastModifiedBy]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260119072127_UpdateKeywordForeignKeyConstraint', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [ContactGroupMembers] DROP CONSTRAINT [FK_ContactGroupMembers_ContactGroups_ContactGroupId];
GO

ALTER TABLE [ContactTagAssignments] DROP CONSTRAINT [FK_ContactTagAssignments_ContactTags_ContactTagId];
GO

ALTER TABLE [KeywordAssignments] DROP CONSTRAINT [FK_KeywordAssignments_Campaigns_CampaignId];
GO

ALTER TABLE [ContactGroupMembers] ADD CONSTRAINT [FK_ContactGroupMembers_ContactGroups_ContactGroupId] FOREIGN KEY ([ContactGroupId]) REFERENCES [ContactGroups] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [ContactTagAssignments] ADD CONSTRAINT [FK_ContactTagAssignments_ContactTags_ContactTagId] FOREIGN KEY ([ContactTagId]) REFERENCES [ContactTags] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [KeywordAssignments] ADD CONSTRAINT [FK_KeywordAssignments_Campaigns_CampaignId] FOREIGN KEY ([CampaignId]) REFERENCES [Campaigns] ([Id]) ON DELETE NO ACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260119075924_FixCascadeConstraints', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [AspNetUsers] ADD [IsOnline] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [AspNetUsers] ADD [LastSeenAt] datetime2 NULL;
GO

CREATE TABLE [ChatRooms] (
    [Id] int NOT NULL IDENTITY,
    [GuestName] nvarchar(200) NULL,
    [GuestEmail] nvarchar(256) NULL,
    [CustomerId] nvarchar(450) NULL,
    [AssignedEmployeeId] nvarchar(450) NULL,
    [Status] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_ChatRooms] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ChatRooms_AspNetUsers_AssignedEmployeeId] FOREIGN KEY ([AssignedEmployeeId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ChatRooms_AspNetUsers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [ChatMessages] (
    [Id] int NOT NULL IDENTITY,
    [ChatRoomId] int NOT NULL,
    [SenderId] nvarchar(450) NULL,
    [MessageText] nvarchar(2000) NOT NULL,
    [IsRead] bit NOT NULL,
    [SentAt] datetime2 NOT NULL,
    [MessageType] int NOT NULL,
    [AttachmentUrl] nvarchar(500) NULL,
    [AttachmentFileName] nvarchar(255) NULL,
    CONSTRAINT [PK_ChatMessages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ChatMessages_AspNetUsers_SenderId] FOREIGN KEY ([SenderId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ChatMessages_ChatRooms_ChatRoomId] FOREIGN KEY ([ChatRoomId]) REFERENCES [ChatRooms] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_ChatMessages_ChatRoomId] ON [ChatMessages] ([ChatRoomId]);
GO

CREATE INDEX [IX_ChatMessages_IsRead] ON [ChatMessages] ([IsRead]);
GO

CREATE INDEX [IX_ChatMessages_SenderId] ON [ChatMessages] ([SenderId]);
GO

CREATE INDEX [IX_ChatMessages_SentAt] ON [ChatMessages] ([SentAt]);
GO

CREATE INDEX [IX_ChatRooms_AssignedEmployeeId] ON [ChatRooms] ([AssignedEmployeeId]);
GO

CREATE INDEX [IX_ChatRooms_CreatedAt] ON [ChatRooms] ([CreatedAt]);
GO

CREATE INDEX [IX_ChatRooms_CustomerId] ON [ChatRooms] ([CustomerId]);
GO

CREATE INDEX [IX_ChatRooms_GuestEmail] ON [ChatRooms] ([GuestEmail]);
GO

CREATE INDEX [IX_ChatRooms_Status] ON [ChatRooms] ([Status]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260119080146_AddChatEntities', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [PageContents] DROP CONSTRAINT [FK_PageContents_AspNetUsers_LastModifiedByUserId];
GO

DROP INDEX [IX_PageContents_LastModifiedByUserId] ON [PageContents];
GO

DECLARE @var11 sysname;
SELECT @var11 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PageContents]') AND [c].[name] = N'LastModifiedByUserId');
IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [PageContents] DROP CONSTRAINT [' + @var11 + '];');
ALTER TABLE [PageContents] DROP COLUMN [LastModifiedByUserId];
GO

DECLARE @var12 sysname;
SELECT @var12 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PageContents]') AND [c].[name] = N'PageKey');
IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [PageContents] DROP CONSTRAINT [' + @var12 + '];');
ALTER TABLE [PageContents] ALTER COLUMN [PageKey] nvarchar(100) NOT NULL;
GO

DECLARE @var13 sysname;
SELECT @var13 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PageContents]') AND [c].[name] = N'LastModifiedBy');
IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [PageContents] DROP CONSTRAINT [' + @var13 + '];');
ALTER TABLE [PageContents] ALTER COLUMN [LastModifiedBy] nvarchar(450) NULL;
GO

CREATE INDEX [IX_PageContents_LastModifiedBy] ON [PageContents] ([LastModifiedBy]);
GO

CREATE UNIQUE INDEX [IX_PageContents_PageKey] ON [PageContents] ([PageKey]);
GO

ALTER TABLE [PageContents] ADD CONSTRAINT [FK_PageContents_AspNetUsers_LastModifiedBy] FOREIGN KEY ([LastModifiedBy]) REFERENCES [AspNetUsers] ([Id]) ON DELETE SET NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260119091307_FixPageContentRelationship', N'8.0.0');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [SubscriptionPlans] ADD [IsMostPopular] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [SubscriptionPlans] ADD [PlanCategory] nvarchar(200) NULL;
GO

CREATE TABLE [Features] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(200) NOT NULL,
    [Description] nvarchar(500) NULL,
    [IsActive] bit NOT NULL,
    [DisplayOrder] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_Features] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [PlanFeatureMappings] (
    [Id] int NOT NULL IDENTITY,
    [SubscriptionPlanId] int NOT NULL,
    [FeatureId] int NOT NULL,
    [IsIncluded] bit NOT NULL,
    [FeatureValue] nvarchar(500) NULL,
    [DisplayOrder] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_PlanFeatureMappings] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PlanFeatureMappings_Features_FeatureId] FOREIGN KEY ([FeatureId]) REFERENCES [Features] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_PlanFeatureMappings_SubscriptionPlans_SubscriptionPlanId] FOREIGN KEY ([SubscriptionPlanId]) REFERENCES [SubscriptionPlans] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Features_DisplayOrder] ON [Features] ([DisplayOrder]);
GO

CREATE INDEX [IX_Features_Name] ON [Features] ([Name]);
GO

CREATE INDEX [IX_PlanFeatureMappings_DisplayOrder] ON [PlanFeatureMappings] ([DisplayOrder]);
GO

CREATE INDEX [IX_PlanFeatureMappings_FeatureId] ON [PlanFeatureMappings] ([FeatureId]);
GO

CREATE INDEX [IX_PlanFeatureMappings_SubscriptionPlanId] ON [PlanFeatureMappings] ([SubscriptionPlanId]);
GO

CREATE UNIQUE INDEX [IX_PlanFeatureMappings_SubscriptionPlanId_FeatureId] ON [PlanFeatureMappings] ([SubscriptionPlanId], [FeatureId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260120052557_AddFeatureManagementAndPlanImprovements', N'8.0.0');
GO

COMMIT;
GO

