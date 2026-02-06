-- Check Landing Page Status
-- Run this to verify tables exist and have data

USE MarketingPlatformDb;
GO

PRINT '========================================';
PRINT 'CHECKING LANDING PAGE TABLES';
PRINT '========================================';
PRINT '';

-- Check LandingFeatures table
IF OBJECT_ID('LandingFeatures', 'U') IS NOT NULL
BEGIN
    PRINT '✓ LandingFeatures table EXISTS';

    DECLARE @featureCount INT;
    SELECT @featureCount = COUNT(*) FROM LandingFeatures WHERE IsDeleted = 0 AND IsActive = 1 AND ShowOnLanding = 1;
    PRINT '  - Total active features: ' + CAST(@featureCount AS VARCHAR);

    IF @featureCount > 0
    BEGIN
        PRINT '  - Features:';
        SELECT '    ' + CAST(Id AS VARCHAR) + '. ' + Title AS Feature
        FROM LandingFeatures
        WHERE IsDeleted = 0 AND IsActive = 1 AND ShowOnLanding = 1
        ORDER BY DisplayOrder;
    END
    ELSE
    BEGIN
        PRINT '  ⚠️  WARNING: No active features found!';
        PRINT '  → Run: add_landing_features.sql';
    END
END
ELSE
BEGIN
    PRINT '✗ LandingFeatures table DOES NOT EXIST';
    PRINT '  → Run: add_landing_features.sql';
END

PRINT '';

-- Check LandingFaqs table
IF OBJECT_ID('LandingFaqs', 'U') IS NOT NULL
BEGIN
    PRINT '✓ LandingFaqs table EXISTS';

    DECLARE @faqCount INT;
    SELECT @faqCount = COUNT(*) FROM LandingFaqs WHERE IsDeleted = 0 AND IsActive = 1 AND ShowOnLanding = 1;
    PRINT '  - Total active FAQs: ' + CAST(@faqCount AS VARCHAR);

    IF @faqCount > 0
    BEGIN
        PRINT '  - FAQs:';
        SELECT '    ' + CAST(Id AS VARCHAR) + '. ' + Question AS FAQ
        FROM LandingFaqs
        WHERE IsDeleted = 0 AND IsActive = 1 AND ShowOnLanding = 1
        ORDER BY DisplayOrder;
    END
    ELSE
    BEGIN
        PRINT '  ⚠️  WARNING: No active FAQs found!';
        PRINT '  → Run: add_landing_faqs.sql';
    END
END
ELSE
BEGIN
    PRINT '✗ LandingFaqs table DOES NOT EXIST';
    PRINT '  → Run: add_landing_faqs.sql';
END

PRINT '';
PRINT '========================================';
PRINT 'NEXT STEPS';
PRINT '========================================';
PRINT '';
PRINT 'If tables are missing or have no data:';
PRINT '1. Run: add_landing_features.sql';
PRINT '2. Run: add_landing_faqs.sql';
PRINT '3. Restart the Web and API applications';
PRINT '4. Open browser DevTools Console (F12)';
PRINT '5. Check for JavaScript errors';
PRINT '';
