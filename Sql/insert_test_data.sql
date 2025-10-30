/* ============================================================
   STRESS DATA SEEDER for ScoopReportsDb (fixed version)
   ============================================================ */
USE ScoopReportsDb;
SET NOCOUNT ON;

/* ---------------------------
   TUNABLE PARAMETERS
   --------------------------- */
DECLARE 
    @Reports                 INT = 1000,   -- number of ScoopReport rows
    @ArticlesPerReport       INT = 2,      -- avg articles per report
    @ProfilesPerReport       INT = 3,      -- avg social profiles per report
    @DetailsPerProfile       INT = 5,      -- avg details (posts) per profile
    @BigChunkRepeatsArticle  INT = 100,    -- adjust for size (256 * repeats ≈ bytes)
    @BigChunkRepeatsDetail   INT = 300;    -- adjust for size (256 * repeats ≈ bytes)

/* A 256-char Base64-ish chunk we’ll repeat to inflate payload size */
DECLARE @Chunk256 VARCHAR(256) =
'QWxhZGRpbjpvcGVuIHNlc2FtZQ+/abcdefghijklmnopqrstuvwxyz0123456789+/
QWxhZGRpbjpvcGVuIHNlc2FtZQ+/abcdefghijklmnopqrstuvwxyz0123456789+/';

/* Temp tables for capturing IDs */
IF OBJECT_ID('tempdb..#ReportIds') IS NOT NULL DROP TABLE #ReportIds;
CREATE TABLE #ReportIds (Id INT PRIMARY KEY);

IF OBJECT_ID('tempdb..#ProfileIds') IS NOT NULL DROP TABLE #ProfileIds;
CREATE TABLE #ProfileIds (Id INT PRIMARY KEY, ReportId INT);

-------------------------------------------------------------------------------
-- 1) Insert ScoopReport
-------------------------------------------------------------------------------
INSERT dbo.ScoopReport
(
    CaseId, HeaderClaimant, HeaderClientFile, HeaderClientName, HeaderDateFile,
    HeaderServiceDate, HeaderDateOfLoss, FooterMessage,
    SubjectIdentifiers, InjuryInformation, AdditionalInternetInformation,
    ActivityIndicators, EmploymentSummary, CriminalHistory, CivilHistory,
    FinancialDistress, Recommendations, ShowRecommendations, Identification,
    Miscellaneous, Deleted, CreatedByUser, CreatedOn, ModifiedByUser, ModifiedOn,
    Disclaimer, ReportSummary, SocialMediaSummary, OnlineSourcesSummary,
    ContactInformation, KeyFindingSummary, ShowDisclaimer
)
OUTPUT INSERTED.ScoopReportId INTO #ReportIds(Id)
SELECT TOP (@Reports)
    n, 
    CONCAT('Claimant #', n),
    CONCAT('FILE-', RIGHT('000000' + CAST(n AS VARCHAR(6)), 6)),
    CONCAT('ClientName ', n),
    CONVERT(VARCHAR(100), SYSDATETIME()),
    CONVERT(VARCHAR(100), DATEADD(DAY, -ABS(CHECKSUM(NEWID())) % 365, SYSDATETIME())),
    CONVERT(VARCHAR(100), DATEADD(DAY, -ABS(CHECKSUM(NEWID())) % 800, SYSDATETIME())),
    'Footer note',
    'Subject identifiers text ...',
    'Injury info ...',
    'More internet info ...',
    'Activity indicators ...',
    'Employment summary ...',
    'Criminal history ...',
    'Civil history ...',
    'Financial distress ...',
    'Recommendation bla bla ...',
    CASE WHEN n % 2 = 0 THEN 1 ELSE 0 END,
    'Identification ...',
    'Misc ...',
    0,                                -- Deleted
    1000 + (n % 10),                  -- CreatedByUser
    DATEADD(MINUTE, n % 6000, SYSUTCDATETIME()),
    NULL, NULL,
    'Disclaimer text ...',
    'Overall report summary ...',
    'Social media summary ...',
    'Online sources summary ...',
    'Contact info ...',
    'Key findings summary ...',
    CASE WHEN n % 3 = 0 THEN 1 ELSE 0 END
FROM (
    SELECT ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n
    FROM sys.all_objects a CROSS JOIN sys.all_objects b
) s;
PRINT CONCAT('Inserted ScoopReport: ', @@ROWCOUNT);

-------------------------------------------------------------------------------
-- 2) Insert Articles per report
-------------------------------------------------------------------------------
INSERT dbo.ScoopReportArticle
(
    ScoopReportId, Title, Url, Summary, ArticleDate,
    Deleted, CreatedByUser, CreatedOn, ModifiedByUser, ModifiedOn,
    IsKeyFinding, KeyFindingContent
)
SELECT r.Id,
       CONCAT('Article for report ', r.Id, ' #', a.rn),
       CONCAT('https://news.example.com/a/', r.Id, '/', a.rn),
       REPLICATE(@Chunk256, @BigChunkRepeatsArticle),               -- big blob
       DATEADD(DAY, -a.rn, SYSUTCDATETIME()),
       0, 2000 + (r.Id % 5), SYSUTCDATETIME(), NULL, NULL,
       CASE WHEN a.rn = 1 THEN 1 ELSE 0 END,
       CASE WHEN a.rn = 1 THEN 'Key finding from article.' ELSE NULL END
FROM #ReportIds r
CROSS APPLY (
    SELECT TOP (@ArticlesPerReport)
           ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS rn
    FROM sys.all_objects
) a;
PRINT CONCAT('Inserted ScoopReportArticle: ', @@ROWCOUNT);

-------------------------------------------------------------------------------
-- 3) Insert Social Media Profiles per report
-------------------------------------------------------------------------------
INSERT dbo.ScoopReportSocialMediaProfile
(
    ScoopReportId, Url, SocialNetworkTypeId, ProfileSummary,
    IsAssociate, Relationship, LastActivityDate,
    Deleted, CreatedByUser, CreatedOn, ModifiedByUser, ModifiedOn
)
OUTPUT INSERTED.ScoopReportSocialMediaProfileId, INSERTED.ScoopReportId
INTO #ProfileIds(Id, ReportId)
SELECT r.Id,
       CONCAT('https://social.example.com/u/', r.Id, '/', p.rn),
       1 + (p.rn % 5),
       CONCAT('Profile summary for report ', r.Id, ' profile ', p.rn),
       CASE WHEN p.rn % 2 = 0 THEN 1 ELSE 0 END,
       CASE WHEN p.rn % 2 = 0 THEN 'Relative' ELSE 'Friend' END,
       DATEADD(DAY, -p.rn, SYSUTCDATETIME()),
       0, 3000 + (r.Id % 7), SYSUTCDATETIME(), NULL, NULL
FROM #ReportIds r
CROSS APPLY (
    SELECT TOP (@ProfilesPerReport)
           ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS rn
    FROM sys.all_objects
) p;
PRINT CONCAT('Inserted ScoopReportSocialMediaProfile: ', @@ROWCOUNT);

-------------------------------------------------------------------------------
-- 4) Insert Profile Details (posts)
-------------------------------------------------------------------------------
INSERT dbo.ScoopReportSocialMediaProfileDetail
(
    ScoopReportSocialMediaProfileId, Description, Content, ActivityDate,
    Deleted, CreatedByUser, CreatedOn, ModifiedByUser, ModifiedOn,
    IsKeyFinding, KeyFindingContent
)
SELECT p.Id,
       CONCAT('Post ', d.rn, ' for profile ', p.Id),
       REPLICATE(@Chunk256, @BigChunkRepeatsDetail),                -- huge blob
       DATEADD(DAY, -d.rn, SYSUTCDATETIME()),
       0, 4000 + (p.Id % 11), SYSUTCDATETIME(), NULL, NULL,
       CASE WHEN d.rn = 1 THEN 1 ELSE 0 END,
       CASE WHEN d.rn = 1 THEN 'Key finding from post content.' ELSE NULL END
FROM #ProfileIds p
CROSS APPLY (
    SELECT TOP (@DetailsPerProfile)
           ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS rn
    FROM sys.all_objects
) d;
PRINT CONCAT('Inserted ScoopReportSocialMediaProfileDetail: ', @@ROWCOUNT);

-------------------------------------------------------------------------------
-- 5) Row counts summary
-------------------------------------------------------------------------------
SELECT 'ScoopReport' AS TableName, COUNT(*) AS Rows FROM dbo.ScoopReport
UNION ALL SELECT 'ScoopReportArticle', COUNT(*) FROM dbo.ScoopReportArticle
UNION ALL SELECT 'ScoopReportSocialMediaProfile', COUNT(*) FROM dbo.ScoopReportSocialMediaProfile
UNION ALL SELECT 'ScoopReportSocialMediaProfileDetail', COUNT(*) FROM dbo.ScoopReportSocialMediaProfileDetail;
GO
