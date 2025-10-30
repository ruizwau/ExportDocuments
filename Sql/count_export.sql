-- Count total number of projects for export pagination
-- This query returns the total count of projects to help calculate
-- the number of pages needed for the export process

SELECT COUNT(*) AS TotalReports
FROM dbo.ScoopReport;