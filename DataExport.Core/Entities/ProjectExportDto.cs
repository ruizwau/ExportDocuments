using System.Text.Json;

namespace DataExport.Core.Entities;

public class ProjectExportRawDto
{
    public int ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Tasks { get; set; } = string.Empty;
    public string Documents { get; set; } = string.Empty;
}
public class ProjectExportDto
{
    public int ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public JsonDocument Tasks { get; set; } = JsonDocument.Parse("[]");
    public JsonDocument Documents { get; set; } = JsonDocument.Parse("[]");
}