SELECT (
    SELECT
        sr.ScoopReportId,
        sr.CaseId,
        sr.HeaderClaimant,
        sr.HeaderClientFile,
        sr.HeaderClientName,
        sr.HeaderDateFile,
        sr.HeaderServiceDate,
        sr.HeaderDateOfLoss,
        sr.FooterMessage,
        sr.SubjectIdentifiers,
        sr.InjuryInformation,
        sr.AdditionalInternetInformation,
        sr.ActivityIndicators,
        sr.EmploymentSummary,
        sr.CriminalHistory,
        sr.CivilHistory,
        sr.FinancialDistress,
        sr.Recommendations,
        sr.ShowRecommendations,
        sr.Identification,
        sr.Miscellaneous,
        sr.Deleted,
        sr.CreatedByUser,
        sr.CreatedOn,
        sr.ModifiedByUser,
        sr.ModifiedOn,
        sr.Disclaimer,
        sr.ReportSummary,
        sr.SocialMediaSummary,
        sr.OnlineSourcesSummary,
        sr.ContactInformation,
        sr.KeyFindingSummary,
        sr.ShowDisclaimer
        -- Subquery: Articles
        ,JSON_QUERY((
            SELECT
                a.ScoopReportArticleId,
                a.Title,
                a.Url,
                a.Summary,
                a.ArticleDate,
                a.Deleted,
                a.CreatedByUser,
                a.CreatedOn,
                a.ModifiedByUser,
                a.ModifiedOn,
                a.IsKeyFinding,
                a.KeyFindingContent
            FROM dbo.ScoopReportArticle a
            WHERE a.ScoopReportId = sr.ScoopReportId
            FOR JSON PATH
        )) AS Articles,

        -- Subquery: SocialMediaProfiles
        JSON_QUERY((
            SELECT
                smp.ScoopReportSocialMediaProfileId,
                smp.Url,
                smp.SocialNetworkTypeId,
                smp.ProfileSummary,
                smp.Relationship,
                smp.LastActivityDate,
                smp.Deleted,
                smp.CreatedByUser,
                smp.CreatedOn,
                smp.ModifiedByUser,
                smp.ModifiedOn
                -- Nested JSON for ProfileDetails
                ,JSON_QUERY((
                    SELECT
                        d.ScoopReportSocialMediaProfileDetailId,
                        d.Description,
                        d.Content,
                        d.ActivityDate,
                        d.Deleted,
                        d.CreatedByUser,
                        d.CreatedOn,
                        d.ModifiedByUser,
                        d.ModifiedOn,
                        d.IsKeyFinding,
                        d.KeyFindingContent
                    FROM dbo.ScoopReportSocialMediaProfileDetail d
                    WHERE d.ScoopReportSocialMediaProfileId = smp.ScoopReportSocialMediaProfileId
                    FOR JSON PATH
                )) AS ProfileDetails

            FROM dbo.ScoopReportSocialMediaProfile smp
            WHERE smp.ScoopReportId = sr.ScoopReportId
            FOR JSON PATH
        )) AS SocialMediaProfiles

    FROM dbo.ScoopReport sr
    ORDER BY sr.ScoopReportId
    OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY
    FOR JSON PATH
) AS JsonResult;
