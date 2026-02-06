-- Update existing plans with proper data
UPDATE SubscriptionPlans
SET
    Name = 'Starter',
    Description = 'Perfect for small businesses getting started',
    PlanCategory = 'For Small Businesses',
    IsMostPopular = 0,
    PriceMonthly = 29.99,
    PriceYearly = 299.99
WHERE Id = 1;

UPDATE SubscriptionPlans
SET
    Name = 'Professional',
    Description = 'Ideal for growing teams and businesses',
    PlanCategory = 'For Growing Teams',
    IsMostPopular = 1,
    PriceMonthly = 79.99,
    PriceYearly = 799.99
WHERE Id = 2;

UPDATE SubscriptionPlans
SET
    Name = 'Enterprise',
    Description = 'Advanced features for large organizations',
    PlanCategory = 'For Large Organizations',
    IsMostPopular = 0,
    PriceMonthly = 249.99,
    PriceYearly = 2499.99
WHERE Id = 3;

PRINT 'Plans updated successfully!';
