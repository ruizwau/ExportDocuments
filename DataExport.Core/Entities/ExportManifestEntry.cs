// Entities/ExportManifestEntry.cs
using Microsoft.Data.SqlClient;

namespace DataExport.Core.Entities;

public class ExportManifestEntry
{
    public int BatchNumber { get; set; }
    public int PageNumber { get; set; }
    public int PageIndex { get; set; }
    public string S3Key { get; set; } = string.Empty;
    public bool Success { get; set; } = true;
    public int? RowsExported { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
}