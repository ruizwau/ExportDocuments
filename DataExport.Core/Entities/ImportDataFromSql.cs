public record ScoopReportRow(
    int ScoopReportId,
    int CaseId,
    string? HeaderClaimant,
    string? HeaderClientFile,
    string? HeaderClientName,
    string? HeaderDeltaFile,
    string? HeaderServiceDate,
    string? HeaderDateOfLoss,
    string? FooterMessage,
    string? SubjectIdentifiers,
    string? InjuryInformation,
    string? AdditionalInternetInformation,
    string? ActivityIndicators,
    string? EmploymentSummary,
    string? CriminalHistory,
    string? CivilHistory,
    string? FinancialDistress,
    string? Recommendations,
    bool ShowRecommendations,
    string? Identification,
    string? Miscellaneous,
    bool Deleted,
    int CreatedByUser,
    DateTime CreatedOn,
    int ModifiedByUser,
    DateTime? ModifiedOn,
    string? Disclaimer,
    string? ReportSummary,
    string? SocialMediaSummary,
    string? OnlineSourcesSummary,
    string? ContactInformation,
    string? KeyFindingSummary,
    bool ShowDisclaimer
);


public record ArticleRow(
    int ScoopReportArticleId,
    int ScoopReportId,
    string? Title,
    string? Url,
    string? Summary,
    DateTime? ArticleDate,
    bool Deleted,
    int CreatedByUser,
    DateTime CreatedOn,
    int ModifiedByUser,
    DateTime? ModifiedOn,
    bool IsKeyFinding,
    string? KeyFindingContent
);


public record ProfileRow(
    int ScoopReportSocialMediaProfileId,
    int ScoopReportId,
    string Url,
    int SocialNetworkTypeId,
    string ProfileSummary,
    string Relationship,
    DateTime LastActivityDate,
    bool Deleted,
    int CreatedByUser,
    DateTime CreatedOn,
    int ModifiedByUser,
    DateTime ModifiedOn
);
public record DetailRow(
    int ScoopReportSocialMediaProfileDetailId,
    int ScoopReportSocialMediaProfileId,
    string Description,
    string Content,
    DateTime? ActivityDate,
    bool Deleted,
    int CreatedByUser,
    DateTime CreatedOn,
    int ModifiedByUser,
    DateTime? ModifiedOn,
    bool IsKeyFinding,
    string? KeyFindingContent
);
