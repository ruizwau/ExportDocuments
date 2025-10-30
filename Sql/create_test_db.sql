/* =========================================
   Database + Schema
   ========================================= */
IF DB_ID(N'ScoopReportsDb') IS NULL
BEGIN
    CREATE DATABASE ScoopReportsDb;
END
GO
USE ScoopReportsDb;
GO

/* =========================================
   Table: dbo.ScoopReport
   ========================================= */
IF OBJECT_ID(N'dbo.ScoopReport', N'U') IS NOT NULL
    DROP TABLE dbo.ScoopReport;
GO
CREATE TABLE dbo.ScoopReport
(
    ScoopReportId              INT IDENTITY(1,1) NOT NULL,
    CaseId                     INT              NOT NULL,

    -- Header / Footer
    HeaderClaimant             VARCHAR(100)     NULL,
    HeaderClientFile           VARCHAR(100)     NULL,
    HeaderClientName           VARCHAR(100)     NULL,
    HeaderDateFile             VARCHAR(100)     NULL,
    HeaderServiceDate          VARCHAR(100)     NULL,
    HeaderDateOfLoss           VARCHAR(100)     NULL,
    FooterMessage              VARCHAR(100)     NULL,

    -- Body sections
    SubjectIdentifiers         VARCHAR(MAX)     NULL,
    InjuryInformation          VARCHAR(MAX)     NULL,
    AdditionalInternetInformation VARCHAR(MAX)  NULL,
    ActivityIndicators         VARCHAR(MAX)     NULL,
    EmploymentSummary          VARCHAR(MAX)     NULL,
    CriminalHistory            VARCHAR(MAX)     NULL,
    CivilHistory               VARCHAR(MAX)     NULL,
    FinancialDistress          VARCHAR(MAX)     NULL,
    Recommendations            VARCHAR(MAX)     NULL,
    ShowRecommendations        BIT              NOT NULL CONSTRAINT DF_ScoopReport_ShowRecommendations DEFAULT (0),

    Identification             VARCHAR(MAX)     NULL,
    Miscellaneous              VARCHAR(MAX)     NULL,

    -- Audit / lifecycle
    Deleted                    BIT              NOT NULL CONSTRAINT DF_ScoopReport_Deleted DEFAULT (0),
    CreatedByUser              INT              NOT NULL,
    CreatedOn                  DATETIME         NOT NULL CONSTRAINT DF_ScoopReport_CreatedOn DEFAULT (GETUTCDATE()),
    ModifiedByUser             INT              NULL,
    ModifiedOn                 DATETIME         NULL,

    -- Summaries / extras
    Disclaimer                 VARCHAR(MAX)     NULL,
    ReportSummary              VARCHAR(MAX)     NULL,
    SocialMediaSummary         VARCHAR(MAX)     NULL,
    OnlineSourcesSummary       VARCHAR(MAX)     NULL,
    ContactInformation         VARCHAR(MAX)     NULL,
    KeyFindingSummary          VARCHAR(MAX)     NULL,
    ShowDisclaimer             BIT              NULL,

    CONSTRAINT PK_ScoopReport PRIMARY KEY CLUSTERED (ScoopReportId)
);
GO

/* =========================================
   Table: dbo.ScoopReportArticle
   ========================================= */
IF OBJECT_ID(N'dbo.ScoopReportArticle', N'U') IS NOT NULL
    DROP TABLE dbo.ScoopReportArticle;
GO
CREATE TABLE dbo.ScoopReportArticle
(
    ScoopReportArticleId   INT IDENTITY(1,1) NOT NULL,
    ScoopReportId          INT                NOT NULL,  -- FK -> ScoopReport
    Title                  VARCHAR(MAX)       NOT NULL,
    Url                    VARCHAR(1000)      NULL,
    Summary                VARCHAR(MAX)       NULL,
    ArticleDate            DATETIME           NULL,

    Deleted                BIT                NOT NULL DEFAULT(0),
    CreatedByUser          INT                NOT NULL,
    CreatedOn              DATETIME           NOT NULL DEFAULT(GETUTCDATE()),
    ModifiedByUser         INT                NULL,
    ModifiedOn             DATETIME           NULL,

    IsKeyFinding           BIT                NOT NULL DEFAULT(0),
    KeyFindingContent      VARCHAR(500)       NULL,

    CONSTRAINT PK_ScoopReportArticle PRIMARY KEY CLUSTERED (ScoopReportArticleId),
    CONSTRAINT FK_ScoopReportArticle_ScoopReport
        FOREIGN KEY (ScoopReportId) REFERENCES dbo.ScoopReport (ScoopReportId)
            ON DELETE CASCADE
);
GO

/* =========================================
   Table: dbo.ScoopReportSocialMediaProfile
   ========================================= */
IF OBJECT_ID(N'dbo.ScoopReportSocialMediaProfile', N'U') IS NOT NULL
    DROP TABLE dbo.ScoopReportSocialMediaProfile;
GO
CREATE TABLE dbo.ScoopReportSocialMediaProfile
(
    ScoopReportSocialMediaProfileId INT IDENTITY(1,1) NOT NULL,
    ScoopReportId                   INT               NOT NULL, -- FK -> ScoopReport

    Url                             VARCHAR(1000)     NULL,
    SocialNetworkTypeId             INT               NULL,
    ProfileSummary                  VARCHAR(MAX)      NULL,
    IsAssociate                     BIT               NOT NULL DEFAULT(0),
    Relationship                    VARCHAR(150)      NULL,
    LastActivityDate                DATETIME          NULL,

    Deleted                         BIT               NOT NULL DEFAULT(0),
    CreatedByUser                   INT               NOT NULL,
    CreatedOn                       DATETIME          NOT NULL DEFAULT(GETUTCDATE()),
    ModifiedByUser                  INT               NULL,
    ModifiedOn                      DATETIME          NULL,

    CONSTRAINT PK_ScoopReportSocialMediaProfile
        PRIMARY KEY CLUSTERED (ScoopReportSocialMediaProfileId),

    CONSTRAINT FK_ScoopReportSocialMediaProfile_ScoopReport
        FOREIGN KEY (ScoopReportId) REFERENCES dbo.ScoopReport (ScoopReportId)
            ON DELETE CASCADE
);
GO

/* Non-clustered index shown in your screenshots */
CREATE NONCLUSTERED INDEX IX_ScoopReportSocialMediaProfile_ScoopReportId_Deleted
ON dbo.ScoopReportSocialMediaProfile (ScoopReportId, Deleted);
GO

/* =========================================
   Table: dbo.ScoopReportSocialMediaProfileDetail
   ========================================= */
IF OBJECT_ID(N'dbo.ScoopReportSocialMediaProfileDetail', N'U') IS NOT NULL
    DROP TABLE dbo.ScoopReportSocialMediaProfileDetail;
GO
CREATE TABLE dbo.ScoopReportSocialMediaProfileDetail
(
    ScoopReportSocialMediaProfileDetailId INT IDENTITY(1,1) NOT NULL,
    ScoopReportSocialMediaProfileId       INT               NOT NULL, -- FK -> SocialMediaProfile

    Description                           VARCHAR(5000)     NULL,
    Content                               VARCHAR(MAX)      NOT NULL,
    ActivityDate                          DATETIME          NULL,

    Deleted                               BIT               NOT NULL DEFAULT(0),
    CreatedByUser                         INT               NOT NULL,
    CreatedOn                             DATETIME          NOT NULL DEFAULT(GETUTCDATE()),
    ModifiedByUser                        INT               NULL,
    ModifiedOn                            DATETIME          NULL,

    IsKeyFinding                          BIT               NOT NULL DEFAULT(0),
    KeyFindingContent                     VARCHAR(5000)     NULL,

    CONSTRAINT PK_ScoopReportSocialMediaProfileDetail
        PRIMARY KEY CLUSTERED (ScoopReportSocialMediaProfileDetailId),

    CONSTRAINT FK_ScoopReportSocialMediaProfileDetail_ScoopReportSocialMediaProfile
        FOREIGN KEY (ScoopReportSocialMediaProfileId)
        REFERENCES dbo.ScoopReportSocialMediaProfile (ScoopReportSocialMediaProfileId)
            ON DELETE CASCADE
);
GO

/* (Optional) helpful lookup/filters for details:
   Uncomment if you want a query aid like you likely had as a "missing index".
--CREATE NONCLUSTERED INDEX IX_ProfileDetail_Profile_Deleted
--ON dbo.ScoopReportSocialMediaProfileDetail (ScoopReportSocialMediaProfileId, Deleted);
--GO
*/

/* =========================================
   Done
   ========================================= */
PRINT 'ScoopReportsDb schema created.';
