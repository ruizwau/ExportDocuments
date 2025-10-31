using Amazon.S3;
using Amazon.S3.Model;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Encodings.Web; // for UnsafeRelaxedJsonEscaping (optional)
using DataExport.Core.Sql;
using DataExport.Core.Entities;

namespace DataExport.Core
{
    /// <summary>
    /// Service to export data from SQL to S3 in batches.
    /// </summary>
    public class ExportService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // optional: preserves HTML/JSON fragments verbatim
        };

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
            long lastId = 0;
            if (batchNumber.HasValue)
            {
                if (batchNumber < 1 || batchNumber > totalBatches)
                    throw new ArgumentOutOfRangeException(nameof(batchNumber), "Batch number is out of range.");
                await ExportBatchAsync(batchNumber.Value, totalBatches, lastId, cancellationToken);
            }
            else
            {
                for (int currentBatch = 1; currentBatch <= totalBatches; currentBatch++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    lastId = await ExportBatchAsync(currentBatch, totalBatches, lastId, cancellationToken);
                }
            }
        }

        /// <summary>Processes a single batch export with pagination.</summary>
        private async Task<long> ExportBatchAsync(int batchNumber, int totalBatches, long lastId, CancellationToken cancellationToken)
        {
            int pageSize = _options.PageSize;
            int totalRecords = await GetTotalRecordCountAsync(cancellationToken);
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            int pagesPerBatch = (int)Math.Ceiling(totalPages / (double)totalBatches);
            int startPageIndex = (batchNumber - 1) * pagesPerBatch;
            int endPageIndex = Math.Min(totalPages - 1, batchNumber * pagesPerBatch - 1);

            // Keyset cursor for this batch

            for (int pageIndex = startPageIndex; pageIndex <= endPageIndex; pageIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    // Force 1 per page
                    var (json, rowCount, onlyId) = await FetchSingleReportPageAsync(lastId, cancellationToken);
                    if (rowCount == 0) break;
                    // advance keyset cursor to that id


                    string objectKey = GenerateObjectKey(batchNumber, pageIndex, onlyId);
                    await UploadToS3Async(objectKey, json, cancellationToken);

                    Console.WriteLine($"Exported batch {batchNumber}, page {pageIndex}, id {onlyId}");

                    await _manifestLogger.LogAsync(new ExportManifestEntry
                    {
                        BatchNumber = batchNumber,
                        PageIndex = pageIndex,
                        PageNumber = pageIndex + 1,
                        S3Key = objectKey,
                        Success = true,
                        RowsExported = rowCount,   // will be 1
                        ErrorMessage = ""
                    }, cancellationToken);

                    lastId = onlyId;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Batch {batchNumber}, Page {pageIndex} failed: {ex.Message}");
                    await _manifestLogger.LogAsync(new ExportManifestEntry
                    {
                        BatchNumber = batchNumber,
                        PageIndex = pageIndex,
                        PageNumber = pageIndex + 1,
                        S3Key = "",
                        Success = false,
                        RowsExported = null,
                        ErrorMessage = ex.Message
                    }, cancellationToken);
                }
            }
            return lastId;
        }

        /// <summary>Fetches exactly one ScoopReport (next by keyset) and its children, serialized as a one-item JSON array.</summary>
        private async Task<(string json, int rowCount, long onlyId)> FetchSingleReportPageAsync(
            long lastId, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new InvalidOperationException("Database connection string is not configured.");

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(ct);

            // 1) One parent (keyset: next id > lastId)
            var parent = await connection.QuerySingleOrDefaultAsync<ScoopReportRow>(
                _sqlProvider.Get("export_report"), // see SQL below
                new { LastId = lastId },
                commandTimeout: 180, commandType: System.Data.CommandType.Text);

            if (parent is null)
                return ("[]", 0, lastId);

            var onlyId = parent.ScoopReportId;

            // 2) Children of that single parent (use = @Id for clarity)
            var articles = (await connection.QueryAsync<ArticleRow>(
                _sqlProvider.Get("export_articles"),
                new { Id = onlyId }, commandTimeout: 180)).ToList();

            var profiles = (await connection.QueryAsync<ProfileRow>(
                _sqlProvider.Get("export_profiles"),
                new { Id = onlyId }, commandTimeout: 180)).ToList();

            var profileIds = profiles.Select(p => p.ScoopReportSocialMediaProfileId).ToArray();

            var details = (await connection.QueryAsync<DetailRow>(
                _sqlProvider.Get("export_profile_details"),
                new { Id = onlyId },
                commandTimeout: 180)).ToList();

            // 3) Stitch
            var reportDto = ExportReportDto.From(parent);

            foreach (var a in articles.OrderBy(x => x.ScoopReportArticleId))
                reportDto.Articles.Add(ExportArticleDto.From(a));

            var profilesById = new Dictionary<int, ExportProfileDto>(profiles.Count);
            foreach (var pr in profiles.OrderBy(x => x.ScoopReportSocialMediaProfileId))
            {
                var prDto = ExportProfileDto.From(pr);
                profilesById[prDto.ScoopReportSocialMediaProfileId] = prDto;
                reportDto.SocialMediaProfiles.Add(prDto);
            }

            foreach (var d in details.OrderBy(x => x.ScoopReportSocialMediaProfileDetailId))
                if (profilesById.TryGetValue(d.ScoopReportSocialMediaProfileId, out var prDto))
                    prDto.ProfileDetails.Add(ExportProfileDetailDto.From(d));

            // 4) Serialize as a single-item array (keeps your downstream consumers happy)
            string json = JsonSerializer.Serialize(new[] { reportDto }, JsonOptions);

            return (json, 1, onlyId);
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

        /// <summary>Generates S3 object key for batch and page.</summary>
        private string GenerateObjectKey(int batchNumber, int pageIndex, long id)
        {
            Console.WriteLine($"Batch {batchNumber}, Page {pageIndex}, ReportId {id}");
            return $"export_batch{batchNumber}_page{pageIndex}_id{id}.json";
        }
    }
}
