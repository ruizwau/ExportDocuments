using Amazon.S3;
using Amazon.S3.Model;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Text.Json;
using DataExport.Core.Sql;
using DataExport.Core.Entities;

namespace DataExport.Core
{

    /// <summary>
    /// Service to export data from SQL to S3 in batches.
    /// </summary>
    public class ExportService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly ISqlProvider _sqlProvider;
        private readonly ExportOptions _options;
        private readonly IExportManifestLogger _manifestLogger;
        private readonly string _bucketName;
        private readonly string? _connectionString;

        public ExportService(IOptions<ExportOptions> options, IAmazonS3 s3Client, ISqlProvider sqlProvider, IExportManifestLogger manifestLogger)
        {
            _options = options.Value;
            _s3Client = s3Client;
            _sqlProvider = sqlProvider;
            _bucketName = _options.BucketName;
            _connectionString = _options.ConnectionString;
            _manifestLogger = manifestLogger;
        }

        /// <summary>Runs export for all batches or specific batch.</summary>
        public async Task RunExportAsync(int? batchNumber = null, CancellationToken cancellationToken = default)
        {
            int totalBatches = _options.BatchCount;
            if (batchNumber.HasValue)
            {
                if (batchNumber < 1 || batchNumber > totalBatches)
                    throw new ArgumentOutOfRangeException(nameof(batchNumber), "Batch number is out of range.");
                await ExportBatchAsync(batchNumber.Value, totalBatches, cancellationToken);
            }
            else
            {
                for (int currentBatch = 1; currentBatch <= totalBatches; currentBatch++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await ExportBatchAsync(currentBatch, totalBatches, cancellationToken);
                }
            }
        }

        /// <summary>Processes a single batch export with pagination.</summary>
        private async Task ExportBatchAsync(int batchNumber, int totalBatches, CancellationToken cancellationToken)
        {
            int pageSize = _options.PageSize;
            int totalRecords = await GetTotalRecordCountAsync(cancellationToken);
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            int pagesPerBatch = (int)Math.Ceiling(totalPages / (double)totalBatches);
            int startPageIndex = (batchNumber - 1) * pagesPerBatch;
            int endPageIndex = Math.Min(totalPages - 1, batchNumber * pagesPerBatch - 1);

            for (int pageIndex = startPageIndex; pageIndex <= endPageIndex; pageIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    string records = await FetchPageDataAsync(pageIndex, pageSize, cancellationToken);
                    string objectKey = GenerateObjectKey(batchNumber, pageIndex);

                    await UploadToS3Async(objectKey, records, cancellationToken);
                    int rowsExported = CountJsonArrayItems(records);

                    Console.WriteLine($"Successfully exported batch {batchNumber}, page {pageIndex} to S3");
                    await _manifestLogger.LogAsync(new ExportManifestEntry
                    {
                        BatchNumber = batchNumber,
                        PageIndex = pageIndex,
                        PageNumber = pageIndex + 1,
                        S3Key = GenerateObjectKey(batchNumber, pageIndex),
                        Success = true,
                        RowsExported = rowsExported,
                        ErrorMessage = ""
                    }, cancellationToken);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Batch {batchNumber}, Page {pageIndex} failed: {ex.Message}");
                    await _manifestLogger.LogAsync(new ExportManifestEntry
                    {
                        BatchNumber = batchNumber,
                        PageIndex = pageIndex,
                        PageNumber = pageIndex + 1,
                        S3Key = GenerateObjectKey(batchNumber, pageIndex),
                        Success = false,
                        RowsExported = null,
                        ErrorMessage = ex.Message
                    }, cancellationToken);
                }
            }
        }

        /// <summary>Fetches paginated data from database as JSON.</summary>
        private async Task<string> FetchPageDataAsync(int pageIndex, int pageSize, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new InvalidOperationException("Database connection string is not configured.");

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            string sql = _sqlProvider.Get("export_paged");
            int offset = pageIndex * pageSize;
            var queryParams = new { Offset = offset, Limit = pageSize };
            var command = new CommandDefinition(sql, queryParams, cancellationToken: cancellationToken, commandTimeout: 180);
            string? jsonResult = await connection.ExecuteScalarAsync<string>(command);
            return jsonResult ?? "[]";
        }

        /// <summary>Uploads JSON content to S3 bucket.</summary>
        private async Task UploadToS3Async(string objectKey, string jsonContent, CancellationToken cancellationToken)
        {
            var putRequest = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = objectKey,
                ContentBody = jsonContent
            };

            await _s3Client.PutObjectAsync(putRequest, cancellationToken);
        }

        /// <summary>Gets total record count from database.</summary>
        private async Task<int> GetTotalRecordCountAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new InvalidOperationException("Database connection string is not configured.");

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            string countSql = _sqlProvider.Get("export_count");
            var command = new CommandDefinition(countSql, cancellationToken: cancellationToken);
            int total = await connection.ExecuteScalarAsync<int>(command);

            return total;
        }

        /// <summary>Counts items in JSON array.</summary>
        private static int CountJsonArrayItems(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return 0;

            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    return doc.RootElement.GetArrayLength();
            }
            catch
            {
                // If it's malformed or not an array, fallback
            }

            return 0;
        }

        /// <summary>Generates S3 object key for batch and page.</summary>
        private string GenerateObjectKey(int batchNumber, int pageIndex)
        {
            return $"export_batch{batchNumber}_page{pageIndex}.json";
        }
    }
}
