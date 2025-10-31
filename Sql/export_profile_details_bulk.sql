SELECT
    d.ScoopReportSocialMediaProfileDetailId,
    d.ScoopReportSocialMediaProfileId,
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
WHERE d.ScoopReportSocialMediaProfileId IN (
    SELECT smp.ScoopReportSocialMediaProfileId
    FROM dbo.ScoopReportSocialMediaProfile smp
    WHERE smp.ScoopReportId = @Id
)
ORDER BY d.ScoopReportSocialMediaProfileDetailId;
