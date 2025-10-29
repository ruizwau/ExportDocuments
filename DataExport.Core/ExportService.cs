using Amazon.S3;
using Amazon.S3.Model;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Data;
using DataExport.Core.Sql;
using DataExport.Core.Entities;

namespace DataExport.Core
{

    /// <summary>
    /// Service to export data from SQL to S3 in batches with robust logging.
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

        /// <summary>
        /// Run the export for all batches or a specific batch if specified.
        /// </summary>
        /// <param name="batchNumber">If set, run only this specific batch (1-indexed). Otherwise, run all batches.</param>
        public async Task RunExportAsync(int? batchNumber = null, CancellationToken cancellationToken = default)
        {
            int totalBatches = _options.BatchCount;
            if (batchNumber.HasValue)
            {
                // Run a specific batch only
                if (batchNumber < 1 || batchNumber > totalBatches)
                    throw new ArgumentOutOfRangeException(nameof(batchNumber), "Batch number is out of range.");
                await ExportBatchAsync(batchNumber.Value, totalBatches, cancellationToken);
            }
            else
            {
                // Run all batches sequentially
                for (int currentBatch = 1; currentBatch <= totalBatches; currentBatch++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await ExportBatchAsync(currentBatch, totalBatches, cancellationToken);
                }
            }
        }

        /// <summary>
        /// Processes a single batch of the export, handling a range of pages.
        /// </summary>
        private async Task ExportBatchAsync(int batchNumber, int totalBatches, CancellationToken cancellationToken)
        {
            // Determine the range of pages this batch should cover.
            int pageSize = _options.PageSize;
            int totalRecords = await GetTotalRecordCountAsync(cancellationToken);
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            int pagesPerBatch = (int)Math.Ceiling(totalPages / (double)totalBatches);
            int startPageIndex = (batchNumber - 1) * pagesPerBatch;
            int endPageIndex = Math.Min(totalPages - 1, batchNumber * pagesPerBatch - 1);

            // Loop through each page in this batch
            for (int pageIndex = startPageIndex; pageIndex <= endPageIndex; pageIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    // 1. Fetch data for the current page
                    var records = await FetchPageDataAsync(pageIndex, pageSize, cancellationToken);
                    int? firstId = records.FirstOrDefault()?.ProjectId;
                    // 2. Upload the data to S3 as a JSON file
                    string objectKey = GenerateObjectKey(batchNumber, pageIndex);

                    var result = records.Select(r => new ProjectExportDto
                    {
                        ProjectId = r.ProjectId,
                        Name = r.Name,
                        Tasks = JsonDocument.Parse(r.Tasks),
                        Documents = JsonDocument.Parse(r.Documents)
                    }).ToList();
                    await UploadToS3Async(objectKey, result, cancellationToken);

                    // 3. Log success in the ExportManifest table
                    Console.WriteLine($"✅ Successfully exported batch {batchNumber}, page {pageIndex} to S3");
                    await _manifestLogger.LogAsync(new ExportManifestEntry
                    {
                        BatchNumber = batchNumber,
                        PageIndex = pageIndex,
                        PageNumber = pageIndex + 1,
                        S3Key = GenerateObjectKey(batchNumber, pageIndex),
                        Success = true,
                        RowsExported = records.Count(),
                        FirstId = firstId,
                        ErrorMessage = ""
                    }, cancellationToken);
                }
                catch (Exception ex)
                {
                    // Log failure for this page, including the error message
                    Console.Error.WriteLine($"Batch {batchNumber}, Page {pageIndex} failed: {ex.Message}");
                    await _manifestLogger.LogAsync(new ExportManifestEntry
                    {
                        BatchNumber = batchNumber,
                        PageIndex = pageIndex,
                        PageNumber = pageIndex + 1,
                        S3Key = GenerateObjectKey(batchNumber, pageIndex),
                        Success = false,
                        RowsExported = null,
                        FirstId = null,
                        ErrorMessage = ex.Message
                    }, cancellationToken);
                }
            }
        }

        /// <summary>
        /// Fetches one page of data from the database using the paged SQL query.
        /// </summary>
        private async Task<IEnumerable<dynamic>> FetchPageDataAsync(int pageIndex, int pageSize, CancellationToken cancellationToken)
        {
            // Open a new database connection for the query (ensure the connection string is set)
            if (string.IsNullOrEmpty(_connectionString))
                throw new InvalidOperationException("Database connection string is not configured.");

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            // Get the SQL query text for the export (expected to use @Offset and @Limit parameters)
            string sql = _sqlProvider.Get("export_projects_paged");
            // Execute the query asynchronously with Dapper, passing parameters and cancellation token
            int offset = pageIndex * pageSize;
            var queryParams = new { Offset = offset, Limit = pageSize };
            var command = new CommandDefinition(sql, queryParams, cancellationToken: cancellationToken);
            // Using Dapper's QueryAsync to retrieve results. This will return a dynamic objects enumeration for flexibility.
            IEnumerable<ProjectExportRawDto> result = await connection.QueryAsync<ProjectExportRawDto>(command);

            // (Alternatively, define a DTO class for strong typing and use QueryAsync<YourDto>)

            return result;
        }

        /// <summary>
        /// Uploads the given records as a JSON file to the configured S3 bucket.
        /// </summary>
        private async Task UploadToS3Async(string objectKey, IEnumerable<dynamic> records, CancellationToken cancellationToken)
        {
            // Serialize records to JSON (writing out as an array)
            string jsonContent = JsonSerializer.Serialize(records);
            // Prepare S3 put request with the bucket, key, and content
            var putRequest = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = objectKey,
                ContentBody = jsonContent
            };
            // Perform the upload to S3 asynchronously
            await _s3Client.PutObjectAsync(putRequest, cancellationToken);
        }

        /// <summary>
        /// Helper to compute total record count (could execute a COUNT query or use other metadata).
        /// </summary>
        private async Task<int> GetTotalRecordCountAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new InvalidOperationException("Database connection string is not configured.");

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            // Get SQL from config (you must define this in your appsettings)
            string countSql = _sqlProvider.Get("export_projects_count");

            var command = new CommandDefinition(countSql, cancellationToken: cancellationToken);
            int total = await connection.ExecuteScalarAsync<int>(command);

            return total;
        }

        /// <summary>
        /// Generates a unique S3 object key (file name) for a given batch and page index.
        /// </summary>
        private string GenerateObjectKey(int batchNumber, int pageIndex)
        {
            // Example: "export_batch1_page10.json"
            return $"export_batch{batchNumber}_page{pageIndex}.json";
        }
    }
}
