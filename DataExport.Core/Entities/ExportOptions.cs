namespace DataExport.Core.Entities;

public class ExportOptions
{
    public int PageSize { get; set; } = 1000;
    public int BatchCount { get; set; } = 1;
    public int TotalBatches { get; set; } = 1;
    public string BucketName { get; set; } = string.Empty;
    public string S3Prefix { get; set; } = "exports";
    public string Region { get; set; } = "us-east-1";   
    public string? ConnectionString { get; set; }
}