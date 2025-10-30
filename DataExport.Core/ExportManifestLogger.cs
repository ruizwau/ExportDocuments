using Dapper;
using Microsoft.Data.SqlClient;
using DataExport.Core.Entities;
using DataExport.Core.Sql;


namespace DataExport.Core;


public interface IExportManifestLogger
{
    Task LogAsync(ExportManifestEntry entry, CancellationToken cancellationToken);
    Task<bool> WasPageExportedAsync(int batchNumber, int pageIndex, CancellationToken cancellationToken);
}


public class ExportManifestLogger : IExportManifestLogger
{
    private readonly string _connectionString;
    private readonly ISqlProvider _sqlProvider;

    public ExportManifestLogger(string connectionString, ISqlProvider sqlProvider)
    {
        _connectionString = connectionString;
        _sqlProvider = sqlProvider;
    }


    public async Task LogAsync(ExportManifestEntry entry, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        string sql = _sqlProvider.Get("insert_export_manifest");
        await connection.ExecuteAsync(sql, entry);
    }


    public async Task<bool> WasPageExportedAsync(int batchNumber, int pageIndex, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        string sql = _sqlProvider.Get("check_page_exported");
        return await connection.ExecuteScalarAsync<int>(sql, new { BatchNumber = batchNumber, PageIndex = pageIndex }) > 0;
    }
}