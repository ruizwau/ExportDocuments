
public class ExportReportDto
{
    public int ScoopReportId { get; init; }
    public int? CaseId { get; init; }
    public string? HeaderClaimant { get; init; }
    public string? HeaderClientFile { get; init; }
    public string? HeaderClientName { get; init; }
    public string? HeaderDeltaFile { get; init; }
    public string? HeaderServiceDate { get; init; }
    public string? HeaderDateOfLoss { get; init; }
    public string? FooterMessage { get; init; }
    public string? SubjectIdentifiers { get; init; }
    public string? InjuryInformation { get; init; }
    public string? AdditionalInternetInformation { get; init; }
    public string? ActivityIndicators { get; init; }
    public string? EmploymentSummary { get; init; }
    public string? CriminalHistory { get; init; }
    public string? CivilHistory { get; init; }
    public string? FinancialDistress { get; init; }
    public string? Recommendations { get; init; }
    public bool? ShowRecommendations { get; init; }
    public string? Identification { get; init; }
    public string? Miscellaneous { get; init; }
    public bool Deleted { get; init; }
    public int? CreatedByUser { get; init; }
    public DateTime? CreatedOn { get; init; }
    public int? ModifiedByUser { get; init; }
    public DateTime? ModifiedOn { get; init; }
    public string? Disclaimer { get; init; }
    public string? ReportSummary { get; init; }
    public string? SocialMediaSummary { get; init; }
    public string? OnlineSourcesSummary { get; init; }
    public string? ContactInformation { get; init; }
    public string? KeyFindingSummary { get; init; }
    public bool? ShowDisclaimer { get; init; }
    public List<ExportArticleDto> Articles { get; init; } = new();
    public List<ExportProfileDto> SocialMediaProfiles { get; init; } = new();

     public static ExportReportDto From(ScoopReportRow r) => new()
    {
        ScoopReportId = r.ScoopReportId,
        CaseId = r.CaseId,
        HeaderClaimant = r.HeaderClaimant,
        HeaderClientFile = r.HeaderClientFile,
        HeaderClientName = r.HeaderClientName,
        HeaderDeltaFile = r.HeaderDeltaFile,
        HeaderServiceDate = r.HeaderServiceDate,
        HeaderDateOfLoss = r.HeaderDateOfLoss,
        FooterMessage = r.FooterMessage,
        SubjectIdentifiers = r.SubjectIdentifiers,
        InjuryInformation = r.InjuryInformation,
        AdditionalInternetInformation = r.AdditionalInternetInformation,
        ActivityIndicators = r.ActivityIndicators,
        EmploymentSummary = r.EmploymentSummary,
        CriminalHistory = r.CriminalHistory,
        CivilHistory = r.CivilHistory,
        FinancialDistress = r.FinancialDistress,
        Recommendations = r.Recommendations,
        ShowRecommendations = r.ShowRecommendations,
        Identification = r.Identification,
        Miscellaneous = r.Miscellaneous,
        Deleted = r.Deleted,
        CreatedByUser = r.CreatedByUser,
        CreatedOn = r.CreatedOn,
        ModifiedByUser = r.ModifiedByUser,
        ModifiedOn = r.ModifiedOn,
        Disclaimer = r.Disclaimer,
        ReportSummary = r.ReportSummary,
        SocialMediaSummary = r.SocialMediaSummary,
        OnlineSourcesSummary = r.OnlineSourcesSummary,
        ContactInformation = r.ContactInformation,
        KeyFindingSummary = r.KeyFindingSummary,
        ShowDisclaimer = r.ShowDisclaimer
    };
}

public class ExportArticleDto
{
    public int ScoopReportArticleId { get; init; }
    public string? Title { get; init; }
    public string? Url { get; init; }
    public string? Summary { get; init; }
    public DateTime? ArticleDate { get; init; }
    public bool Deleted { get; init; }
    public int? CreatedByUser { get; init; }
    public DateTime? CreatedOn { get; init; }
    public int? ModifiedByUser { get; init; }
    public DateTime? ModifiedOn { get; init; }
    public bool? IsKeyFinding { get; init; }
    public string? KeyFindingContent { get; init; }

    public static ExportArticleDto From(ArticleRow r) => new()
    {
        ScoopReportArticleId = r.ScoopReportArticleId,
        Title = r.Title,
        Url = r.Url,
        Summary = r.Summary,
        ArticleDate = r.ArticleDate,
        Deleted = r.Deleted,
        CreatedByUser = r.CreatedByUser,
        CreatedOn = r.CreatedOn,
        ModifiedByUser = r.ModifiedByUser,
        ModifiedOn = r.ModifiedOn,
        IsKeyFinding = r.IsKeyFinding,
        KeyFindingContent = r.KeyFindingContent
    };
}

public class ExportProfileDto
{
    public int ScoopReportSocialMediaProfileId { get; init; }
    public string? Url { get; init; }
    public int? SocialNetworkTypeId { get; init; }
    public string? ProfileSummary { get; init; }
    public string? Relationship { get; init; }
    public DateTime? LastActivityDate { get; init; }
    public bool Deleted { get; init; }
    public int? CreatedByUser { get; init; }
    public DateTime? CreatedOn { get; init; }
    public int? ModifiedByUser { get; init; }
    public DateTime? ModifiedOn { get; init; }
    public List<ExportProfileDetailDto> ProfileDetails { get; init; } = new();


    public static ExportProfileDto From(ProfileRow r) => new()
    {
        ScoopReportSocialMediaProfileId = r.ScoopReportSocialMediaProfileId,
        Url = r.Url,
        SocialNetworkTypeId = r.SocialNetworkTypeId,
        ProfileSummary = r.ProfileSummary,
        Relationship = r.Relationship,
        LastActivityDate = r.LastActivityDate,
        Deleted = r.Deleted,
        CreatedByUser = r.CreatedByUser,
        CreatedOn = r.CreatedOn,
        ModifiedByUser = r.ModifiedByUser,
        ModifiedOn = r.ModifiedOn
    };
}

public class ExportProfileDetailDto
{
    public int ScoopReportSocialMediaProfileDetailId { get; init; }
    public string? Description { get; init; }
    public string? Content { get; init; }
    public DateTime? ActivityDate { get; init; }
    public bool Deleted { get; init; }
    public int? CreatedByUser { get; init; }
    public DateTime? CreatedOn { get; init; }
    public int? ModifiedByUser { get; init; }
    public DateTime? ModifiedOn { get; init; }
    public bool? IsKeyFinding { get; init; }
    public string? KeyFindingContent { get; init; }

    public static ExportProfileDetailDto From(DetailRow r) => new()
    {
        ScoopReportSocialMediaProfileDetailId = r.ScoopReportSocialMediaProfileDetailId,
        Description = r.Description,
        Content = r.Content,
        ActivityDate = r.ActivityDate,
        Deleted = r.Deleted,
        CreatedByUser = r.CreatedByUser,
        CreatedOn = r.CreatedOn,
        ModifiedByUser = r.ModifiedByUser,
        ModifiedOn = r.ModifiedOn,
        IsKeyFinding = r.IsKeyFinding,
        KeyFindingContent = r.KeyFindingContent
    };
}
