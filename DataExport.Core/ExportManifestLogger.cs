using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using DataExport.Core.Entities;


namespace DataExport.Core;


public interface IExportManifestLogger
{
    Task LogAsync(ExportManifestEntry entry, CancellationToken cancellationToken);
    Task<bool> WasPageExportedAsync(int batchNumber, int pageIndex, CancellationToken cancellationToken);
}


public class ExportManifestLogger : IExportManifestLogger
{
    private readonly string _connectionString;


    public ExportManifestLogger(string connectionString)
    {
        _connectionString = connectionString;
    }


    public async Task LogAsync(ExportManifestEntry entry, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);


        string sql = @"
INSERT INTO dbo.ExportManifest
(BatchNumber, PageNumber, PageIndex, S3Key, Success, RowsExported, FirstId, ErrorMessage, CreatedAt, LoggedAt)
VALUES
(@BatchNumber, @PageNumber, @PageIndex, @S3Key, @Success, @RowsExported, @FirstId, @ErrorMessage, @CreatedAt, @LoggedAt);
";


        await connection.ExecuteAsync(sql, entry);
    }


    public async Task<bool> WasPageExportedAsync(int batchNumber, int pageIndex, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);


        string sql = @"
SELECT COUNT(1)
FROM dbo.ExportManifest
WHERE BatchNumber = @BatchNumber AND PageIndex = @PageIndex AND Success = 1;
";


        return await connection.ExecuteScalarAsync<int>(sql, new { BatchNumber = batchNumber, PageIndex = pageIndex }) > 0;
    }
}