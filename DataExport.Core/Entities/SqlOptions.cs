

namespace DataExport.Core.Entities;
public class SqlOptions
{
    public const string SectionName = "Sql";
    
    public string BasePath { get; set; } = "./Sql";
    public Dictionary<string, string> Queries { get; set; } = new();
}