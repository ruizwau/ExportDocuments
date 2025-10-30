-- Create database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'DataMigrationMonitoring')
BEGIN
    CREATE DATABASE DataMigrationMonitoring;
END
GO

-- Use the database
USE DataMigrationMonitoring;
GO

-- Create ExportManifest table to track export progress
-- Drop table if it exists to recreate with correct schema
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.ExportManifest') AND type in (N'U'))
BEGIN
    DROP TABLE dbo.ExportManifest;
END
GO

CREATE TABLE dbo.ExportManifest (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    BatchNumber INT NOT NULL,
    PageNumber INT NOT NULL,
    PageIndex INT NOT NULL,  -- Column that was missing
    S3Key NVARCHAR(500) NOT NULL,
    Success BIT NOT NULL DEFAULT 1,  -- Column that was missing  
    RowsExported INT NULL,
    ErrorMessage NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    LoggedAt DATETIME2 DEFAULT GETUTCDATE()  -- Column that was missing
);

-- Create index for performance
CREATE INDEX IX_ExportManifest_Batch_Page ON dbo.ExportManifest (BatchNumber, PageNumber);
CREATE INDEX IX_ExportManifest_Success ON dbo.ExportManifest (Success);
CREATE INDEX IX_ExportManifest_LoggedAt ON dbo.ExportManifest (LoggedAt);