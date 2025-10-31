SELECT
    smp.ScoopReportSocialMediaProfileId,
    smp.ScoopReportId,
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
FROM dbo.ScoopReportSocialMediaProfile smp
WHERE smp.ScoopReportId = @Id
ORDER BY smp.ScoopReportSocialMediaProfileId;