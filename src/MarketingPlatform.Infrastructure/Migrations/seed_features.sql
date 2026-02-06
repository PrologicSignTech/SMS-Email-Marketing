-- Insert Features
IF NOT EXISTS (SELECT * FROM Features WHERE Name = 'SMS Messages')
INSERT INTO Features (Name, Description, IsActive, DisplayOrder, CreatedAt)
VALUES
    ('SMS Messages', 'Send SMS text messages to your contacts', 1, 1, GETUTCDATE()),
    ('MMS Messages', 'Send multimedia messages with images and videos', 1, 2, GETUTCDATE()),
    ('Email Campaigns', 'Create and send professional email campaigns', 1, 3, GETUTCDATE()),
    ('Contacts', 'Store and manage your contact database', 1, 4, GETUTCDATE()),
    ('Campaign Automation', 'Automate your marketing campaigns', 1, 5, GETUTCDATE()),
    ('A/B Testing', 'Test different campaign variations', 1, 6, GETUTCDATE()),
    ('Advanced Analytics', 'Detailed insights and reporting', 1, 7, GETUTCDATE()),
    ('Multi-Channel Campaigns', 'Run campaigns across multiple channels', 1, 8, GETUTCDATE()),
    ('API Access', 'Programmatic access to the platform', 1, 9, GETUTCDATE()),
    ('Priority Support', '24/7 priority customer support', 1, 10, GETUTCDATE()),
    ('Custom Integrations', 'Build custom integrations', 1, 11, GETUTCDATE()),
    ('Dedicated Account Manager', 'Personal account management', 1, 12, GETUTCDATE()),
    ('White-Label Options', 'Brand the platform as your own', 1, 13, GETUTCDATE()),
    ('Advanced Security', 'Enterprise-grade security features', 1, 14, GETUTCDATE());

PRINT 'Features inserted successfully!';

-- Now insert plan-feature mappings
DECLARE @StarterId INT = (SELECT Id FROM SubscriptionPlans WHERE Name = 'Starter');
DECLARE @ProId INT = (SELECT Id FROM SubscriptionPlans WHERE Name = 'Professional');
DECLARE @EnterpriseId INT = (SELECT Id FROM SubscriptionPlans WHERE Name = 'Enterprise');

-- Starter Plan Features
IF @StarterId IS NOT NULL
BEGIN
    INSERT INTO PlanFeatureMappings (SubscriptionPlanId, FeatureId, FeatureValue, IsIncluded, DisplayOrder, CreatedAt)
    SELECT @StarterId, Id,
        CASE Name
            WHEN 'SMS Messages' THEN '5,000/month'
            WHEN 'Email Campaigns' THEN '10,000/month'
            WHEN 'Contacts' THEN '5,000 contacts'
            WHEN 'Campaign Automation' THEN 'Basic'
            WHEN 'API Access' THEN 'Read-only'
            ELSE NULL
        END,
        CASE Name
            WHEN 'SMS Messages' THEN 1
            WHEN 'Email Campaigns' THEN 1
            WHEN 'Contacts' THEN 1
            WHEN 'Campaign Automation' THEN 1
            WHEN 'Advanced Analytics' THEN 1
            WHEN 'API Access' THEN 1
            ELSE 0
        END,
        DisplayOrder,
        GETUTCDATE()
    FROM Features
    WHERE NOT EXISTS (SELECT * FROM PlanFeatureMappings WHERE SubscriptionPlanId = @StarterId);

    PRINT 'Starter plan features added!';
END

-- Professional Plan Features
IF @ProId IS NOT NULL
BEGIN
    INSERT INTO PlanFeatureMappings (SubscriptionPlanId, FeatureId, FeatureValue, IsIncluded, DisplayOrder, CreatedAt)
    SELECT @ProId, Id,
        CASE Name
            WHEN 'SMS Messages' THEN '25,000/month'
            WHEN 'MMS Messages' THEN '5,000/month'
            WHEN 'Email Campaigns' THEN '50,000/month'
            WHEN 'Contacts' THEN '25,000 contacts'
            WHEN 'Campaign Automation' THEN 'Advanced'
            WHEN 'API Access' THEN 'Full access'
            ELSE NULL
        END,
        CASE Name
            WHEN 'SMS Messages' THEN 1
            WHEN 'MMS Messages' THEN 1
            WHEN 'Email Campaigns' THEN 1
            WHEN 'Contacts' THEN 1
            WHEN 'Campaign Automation' THEN 1
            WHEN 'A/B Testing' THEN 1
            WHEN 'Advanced Analytics' THEN 1
            WHEN 'Multi-Channel Campaigns' THEN 1
            WHEN 'API Access' THEN 1
            ELSE 0
        END,
        DisplayOrder,
        GETUTCDATE()
    FROM Features
    WHERE NOT EXISTS (SELECT * FROM PlanFeatureMappings WHERE SubscriptionPlanId = @ProId);

    PRINT 'Professional plan features added!';
END

-- Enterprise Plan Features
IF @EnterpriseId IS NOT NULL
BEGIN
    INSERT INTO PlanFeatureMappings (SubscriptionPlanId, FeatureId, FeatureValue, IsIncluded, DisplayOrder, CreatedAt)
    SELECT @EnterpriseId, Id,
        CASE Name
            WHEN 'SMS Messages' THEN 'Unlimited'
            WHEN 'MMS Messages' THEN 'Unlimited'
            WHEN 'Email Campaigns' THEN 'Unlimited'
            WHEN 'Contacts' THEN 'Unlimited'
            WHEN 'Campaign Automation' THEN 'AI-Powered'
            WHEN 'API Access' THEN 'Full + Webhooks'
            ELSE NULL
        END,
        1, -- All features included
        DisplayOrder,
        GETUTCDATE()
    FROM Features
    WHERE NOT EXISTS (SELECT * FROM PlanFeatureMappings WHERE SubscriptionPlanId = @EnterpriseId);

    PRINT 'Enterprise plan features added!';
END

PRINT 'All features and mappings inserted successfully!';
