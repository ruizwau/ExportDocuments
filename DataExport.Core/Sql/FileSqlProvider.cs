using Microsoft.Extensions.Configuration;

namespace DataExport.Core.Sql;

public class FileSqlProvider : ISqlProvider
{
    private readonly string _basePath;
    private readonly Dictionary<string,string> _map;

    public FileSqlProvider(IConfiguration config)
    {
        _basePath = Path.GetFullPath(
            config["Sql:BasePath"] ?? "./Sql");
        _map = config.GetSection("Sql:Queries")
                     .GetChildren()
                     .ToDictionary(s => s.Key, s => s.Value!);
    }

    public string Get(string name)
    {
        if (!_map.TryGetValue(name, out var file))
            throw new InvalidOperationException($"SQL '{name}' not found in config.");
        var full = Path.Combine(_basePath, file);
        
        // Debug information
        Console.WriteLine($"Looking for SQL file: {name}");
        Console.WriteLine($"Base path: {_basePath}");
        Console.WriteLine($"File name: {file}");
        Console.WriteLine($"Full path: {full}");
        Console.WriteLine($"File exists: {File.Exists(full)}");
        
        if (!File.Exists(full))
            throw new FileNotFoundException($"SQL file not found: {full}");
        return File.ReadAllText(full);
    }
}
