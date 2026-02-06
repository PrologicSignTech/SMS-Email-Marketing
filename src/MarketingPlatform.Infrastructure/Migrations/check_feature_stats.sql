-- Check Feature Statistics Data
USE MarketingPlatformDb;
GO

PRINT '========================================';
PRINT 'CHECKING LANDING FEATURE STATISTICS';
PRINT '========================================';
PRINT '';

SELECT
    Id,
    Title,
    StatTitle1,
    StatValue1,
    StatTitle2,
    StatValue2,
    StatTitle3,
    StatValue3,
    CASE
        WHEN StatTitle1 IS NULL THEN 'Missing'
        ELSE 'OK'
    END AS Stat1Status,
    CASE
        WHEN StatTitle2 IS NULL THEN 'Missing'
        ELSE 'OK'
    END AS Stat2Status,
    CASE
        WHEN StatTitle3 IS NULL THEN 'Missing'
        ELSE 'OK'
    END AS Stat3Status
FROM LandingFeatures
WHERE IsActive = 1 AND IsDeleted = 0
ORDER BY DisplayOrder;

PRINT '';
PRINT '========================================';
PRINT 'If any stats show as Missing, run:';
PRINT '  add_landing_features.sql';
PRINT 'to populate with sample data';
PRINT '========================================';
