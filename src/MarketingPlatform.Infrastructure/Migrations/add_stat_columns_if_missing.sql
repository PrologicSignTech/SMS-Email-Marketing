-- ================================================================
-- Add Stat Columns to LandingFeatures if they don't exist
-- Run this if you get "Invalid column name 'StatTitle1'" errors
-- ================================================================

USE plogi7dd_SMS;
GO

PRINT '========================================';
PRINT 'ADDING STAT COLUMNS TO LANDINGFEATURES';
PRINT '========================================';
PRINT '';

-- Add StatTitle1 if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'LandingFeatures' AND COLUMN_NAME = 'StatTitle1')
BEGIN
    ALTER TABLE [LandingFeatures] ADD [StatTitle1] NVARCHAR(100) NULL;
    PRINT '✓ Added StatTitle1 column';
END
ELSE
BEGIN
    PRINT '⚠ StatTitle1 column already exists';
END

-- Add StatValue1 if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'LandingFeatures' AND COLUMN_NAME = 'StatValue1')
BEGIN
    ALTER TABLE [LandingFeatures] ADD [StatValue1] NVARCHAR(50) NULL;
    PRINT '✓ Added StatValue1 column';
END
ELSE
BEGIN
    PRINT '⚠ StatValue1 column already exists';
END

-- Add StatTitle2 if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'LandingFeatures' AND COLUMN_NAME = 'StatTitle2')
BEGIN
    ALTER TABLE [LandingFeatures] ADD [StatTitle2] NVARCHAR(100) NULL;
    PRINT '✓ Added StatTitle2 column';
END
ELSE
BEGIN
    PRINT '⚠ StatTitle2 column already exists';
END

-- Add StatValue2 if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'LandingFeatures' AND COLUMN_NAME = 'StatValue2')
BEGIN
    ALTER TABLE [LandingFeatures] ADD [StatValue2] NVARCHAR(50) NULL;
    PRINT '✓ Added StatValue2 column';
END
ELSE
BEGIN
    PRINT '⚠ StatValue2 column already exists';
END

-- Add StatTitle3 if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'LandingFeatures' AND COLUMN_NAME = 'StatTitle3')
BEGIN
    ALTER TABLE [LandingFeatures] ADD [StatTitle3] NVARCHAR(100) NULL;
    PRINT '✓ Added StatTitle3 column';
END
ELSE
BEGIN
    PRINT '⚠ StatTitle3 column already exists';
END

-- Add StatValue3 if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'LandingFeatures' AND COLUMN_NAME = 'StatValue3')
BEGIN
    ALTER TABLE [LandingFeatures] ADD [StatValue3] NVARCHAR(50) NULL;
    PRINT '✓ Added StatValue3 column';
END
ELSE
BEGIN
    PRINT '⚠ StatValue3 column already exists';
END

-- Add CallToActionText if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'LandingFeatures' AND COLUMN_NAME = 'CallToActionText')
BEGIN
    ALTER TABLE [LandingFeatures] ADD [CallToActionText] NVARCHAR(100) NULL;
    PRINT '✓ Added CallToActionText column';
END
ELSE
BEGIN
    PRINT '⚠ CallToActionText column already exists';
END

-- Add CallToActionUrl if it doesn't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'LandingFeatures' AND COLUMN_NAME = 'CallToActionUrl')
BEGIN
    ALTER TABLE [LandingFeatures] ADD [CallToActionUrl] NVARCHAR(500) NULL;
    PRINT '✓ Added CallToActionUrl column';
END
ELSE
BEGIN
    PRINT '⚠ CallToActionUrl column already exists';
END

PRINT '';
PRINT '========================================';
PRINT 'STAT COLUMNS UPDATE COMPLETE';
PRINT '========================================';
PRINT '';
PRINT 'You can now restart your application and the error should be gone.';
PRINT '';
