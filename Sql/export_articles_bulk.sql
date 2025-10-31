SELECT
    a.ScoopReportArticleId,
    a.ScoopReportId,
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
WHERE a.ScoopReportId = @Id
ORDER BY a.ScoopReportArticleId;
