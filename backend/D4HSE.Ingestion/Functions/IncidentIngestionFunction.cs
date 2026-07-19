using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.Lambda.CloudWatchEvents.ScheduledEvents;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using D4HSE.Core.Entities;
using D4HSE.Core.Interfaces;
using D4HSE.Infrastructure.Data;
using D4HSE.Ingestion.Models;
using D4HSE.Ingestion.Validators;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using LambdaModel = Amazon.Lambda.Model;

namespace D4HSE.Ingestion.Functions;

/// <summary>
/// Lambda function for ingesting incident and near-miss data from the S3 landing zone.
/// Triggered by EventBridge Scheduler on a configurable cron schedule.
/// 
/// Flow:
///   1. Read incident/near-miss records from S3 landing zone bucket (incidents/ prefix)
///   2. Validate each record using FluentValidation
///   3. Valid records → persist to RDS as Incident or NearMiss entity (based on IsNearMiss flag)
///   4. Invalid records → log to DynamoDB DataQualityLog with business-readable messages
///   5. Update last-ingestion timestamp in Parameter Store
///   6. Trigger embedding generation for valid records
/// </summary>
public class IncidentIngestionFunction
{
    private static readonly ServiceProvider ServiceProvider;

    private const string SourceCategory = "incidents";
    private const string S3Prefix = "incidents/";

    static IncidentIngestionFunction()
    {
        var services = new ServiceCollection();

        // Database context
        var connectionString = System.Environment.GetEnvironmentVariable("DATABASE__CONNECTIONSTRING")
            ?? throw new InvalidOperationException("DATABASE__CONNECTIONSTRING environment variable is not set.");

        services.AddDbContext<HseCockpitDbContext>(options =>
            options.UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention());

        // AWS services
        services.AddSingleton<IAmazonS3, AmazonS3Client>();
        services.AddSingleton<IAmazonSimpleSystemsManagement, AmazonSimpleSystemsManagementClient>();
        services.AddSingleton<Amazon.Lambda.IAmazonLambda, Amazon.Lambda.AmazonLambdaClient>();
        services.AddSingleton<IAmazonDynamoDB, AmazonDynamoDBClient>();

        // Validators
        services.AddScoped<IValidator<IncidentSourceRecord>, IncidentSourceRecordValidator>();

        // Repositories (from Infrastructure project)
        services.AddScoped<IDataQualityLogRepository>(sp =>
        {
            var tableName = System.Environment.GetEnvironmentVariable("DYNAMODB__QUALITYLOG_TABLE") ?? "DataQualityLog";
            return ActivatorUtilities.CreateInstance<D4HSE.Infrastructure.Repositories.DataQualityLogRepository>(sp, tableName);
        });

        ServiceProvider = services.BuildServiceProvider();
    }

    /// <summary>
    /// Lambda handler entry point triggered by EventBridge Scheduler.
    /// </summary>
    public async Task FunctionHandler(ScheduledEvent input, ILambdaContext context)
    {
        context.Logger.LogInformation($"IncidentIngestionFunction invoked at {input.Time}. Request ID: {context.AwsRequestId}");

        var s3Client = ServiceProvider.GetRequiredService<IAmazonS3>();
        var ssmClient = ServiceProvider.GetRequiredService<IAmazonSimpleSystemsManagement>();

        using var scope = ServiceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HseCockpitDbContext>();
        var validator = scope.ServiceProvider.GetRequiredService<IValidator<IncidentSourceRecord>>();
        var qualityLogRepo = scope.ServiceProvider.GetRequiredService<IDataQualityLogRepository>();

        var bucketName = System.Environment.GetEnvironmentVariable("S3__LANDINGZONE_BUCKET")
            ?? throw new InvalidOperationException("S3__LANDINGZONE_BUCKET environment variable is not set.");
        var timestampKey = System.Environment.GetEnvironmentVariable("PARAMETERSTORE__INGESTION_TIMESTAMP_KEY")
            ?? "/hse/ingestion/last-run";

        var ingestionDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var validIncidents = new List<Incident>();
        var validNearMisses = new List<NearMiss>();
        var qualityLogEntries = new List<DataQualityLogEntry>();
        int totalProcessed = 0;

        try
        {
            // List objects in the S3 incidents prefix
            var listRequest = new ListObjectsV2Request
            {
                BucketName = bucketName,
                Prefix = S3Prefix
            };

            var listResponse = await s3Client.ListObjectsV2Async(listRequest);
            var sourceFiles = listResponse.S3Objects
                .Where(o => o.Key.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
                         || o.Key.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                .ToList();

            context.Logger.LogInformation($"Found {sourceFiles.Count} source file(s) in s3://{bucketName}/{S3Prefix}");

            foreach (var s3Object in sourceFiles)
            {
                var records = await ReadRecordsFromS3Async(s3Client, bucketName, s3Object.Key, context);
                context.Logger.LogInformation($"Parsed {records.Count} records from {s3Object.Key}");

                foreach (var record in records)
                {
                    totalProcessed++;
                    var validationResult = await validator.ValidateAsync(record);

                    if (validationResult.IsValid)
                    {
                        if (record.IsNearMiss)
                        {
                            var nearMiss = MapToNearMissEntity(record, ingestionDate);
                            validNearMisses.Add(nearMiss);
                        }
                        else
                        {
                            var incident = MapToIncidentEntity(record, ingestionDate);
                            validIncidents.Add(incident);
                        }
                    }
                    else
                    {
                        // Log each validation failure to DynamoDB
                        foreach (var error in validationResult.Errors)
                        {
                            var logEntry = CreateQualityLogEntry(record, error, ingestionDate);
                            qualityLogEntries.Add(logEntry);
                        }
                    }
                }
            }

            // Persist valid incidents to RDS
            if (validIncidents.Count > 0)
            {
                await dbContext.Incidents.AddRangeAsync(validIncidents);
                context.Logger.LogInformation($"Queued {validIncidents.Count} incident(s) for persistence.");
            }

            // Persist valid near misses to RDS
            if (validNearMisses.Count > 0)
            {
                await dbContext.NearMisses.AddRangeAsync(validNearMisses);
                context.Logger.LogInformation($"Queued {validNearMisses.Count} near-miss(es) for persistence.");
            }

            if (validIncidents.Count > 0 || validNearMisses.Count > 0)
            {
                await dbContext.SaveChangesAsync();
                context.Logger.LogInformation(
                    $"Persisted {validIncidents.Count} incidents and {validNearMisses.Count} near misses to RDS.");
            }

            // Log invalid records to DynamoDB
            if (qualityLogEntries.Count > 0)
            {
                await qualityLogRepo.LogBatchAsync(qualityLogEntries, CancellationToken.None);
                context.Logger.LogWarning($"Logged {qualityLogEntries.Count} validation failure(s) to DynamoDB DataQualityLog.");
            }

            // Update last-ingestion timestamp in Parameter Store
            await ssmClient.PutParameterAsync(new PutParameterRequest
            {
                Name = timestampKey,
                Value = DateTime.UtcNow.ToString("o"),
                Type = ParameterType.String,
                Overwrite = true
            });

            // Trigger post-ingestion embedding generation asynchronously
            var allRecordIds = validIncidents.Select(i => i.IncidentId.ToString())
                .Concat(validNearMisses.Select(nm => nm.NearMissId.ToString()))
                .ToList();

            if (allRecordIds.Count > 0)
            {
                await TriggerEmbeddingGenerationAsync(allRecordIds, ingestionDate, context);
            }

            context.Logger.LogInformation(
                $"Incident ingestion complete. Total processed: {totalProcessed}, " +
                $"Incidents: {validIncidents.Count}, Near misses: {validNearMisses.Count}, " +
                $"Invalid: {qualityLogEntries.Count}");
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Incident ingestion failed: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Asynchronously invokes the embedding generation Lambda to produce vector embeddings
    /// for newly ingested incident/near-miss records.
    /// </summary>
    private static async Task TriggerEmbeddingGenerationAsync(
        List<string> recordIds,
        DateOnly ingestionDate,
        ILambdaContext context)
    {
        var lambdaClient = ServiceProvider.GetRequiredService<Amazon.Lambda.IAmazonLambda>();

        var embeddingFunctionName = System.Environment.GetEnvironmentVariable("EMBEDDING_FUNCTION_NAME")
            ?? "HSECockpit-EmbeddingGeneration";

        var embeddingRequest = new EmbeddingRequest
        {
            SourceCategory = SourceCategory,
            RecordIds = recordIds,
            IngestionDate = ingestionDate.ToString("yyyy-MM-dd")
        };

        var payload = JsonSerializer.Serialize(embeddingRequest);

        try
        {
            var invokeRequest = new LambdaModel.InvokeRequest
            {
                FunctionName = embeddingFunctionName,
                InvocationType = "Event",
                Payload = payload
            };

            var response = await lambdaClient.InvokeAsync(invokeRequest);
            context.Logger.LogInformation(
                $"Triggered embedding generation for {recordIds.Count} records. " +
                $"Lambda: {embeddingFunctionName}, StatusCode: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            // Embedding generation failure should not fail the ingestion pipeline.
            context.Logger.LogWarning(
                $"Failed to trigger embedding generation Lambda '{embeddingFunctionName}': {ex.Message}. " +
                $"Records are persisted but embeddings may need manual regeneration.");
        }
    }

    /// <summary>
    /// Reads and parses incident/near-miss records from a JSON or CSV file in S3.
    /// </summary>
    private static async Task<List<IncidentSourceRecord>> ReadRecordsFromS3Async(
        IAmazonS3 s3Client, string bucketName, string key, ILambdaContext context)
    {
        var getRequest = new GetObjectRequest
        {
            BucketName = bucketName,
            Key = key
        };

        using var response = await s3Client.GetObjectAsync(getRequest);
        using var reader = new StreamReader(response.ResponseStream);
        var content = await reader.ReadToEndAsync();

        if (key.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            return ParseJsonRecords(content, context);
        }
        else if (key.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return ParseCsvRecords(content, context);
        }

        return [];
    }

    /// <summary>
    /// Parses incident/near-miss records from JSON content.
    /// Supports both an array of records and a single record.
    /// </summary>
    private static List<IncidentSourceRecord> ParseJsonRecords(string content, ILambdaContext context)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var records = JsonSerializer.Deserialize<List<IncidentSourceRecord>>(content, options);
            return records ?? [];
        }
        catch (JsonException ex)
        {
            context.Logger.LogWarning($"Failed to parse JSON as array, attempting single record: {ex.Message}");
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var single = JsonSerializer.Deserialize<IncidentSourceRecord>(content, options);
                return single is not null ? [single] : [];
            }
            catch
            {
                context.Logger.LogError($"Failed to parse JSON content: {ex.Message}");
                return [];
            }
        }
    }

    /// <summary>
    /// Parses incident/near-miss records from CSV content.
    /// Expected columns: RecordId,SiteId,AssetId,IncidentDate,Severity,IncidentType,Description,IsNearMiss
    /// </summary>
    private static List<IncidentSourceRecord> ParseCsvRecords(string content, ILambdaContext context)
    {
        var records = new List<IncidentSourceRecord>();
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length < 2)
        {
            context.Logger.LogWarning("CSV file has no data rows (only header or empty).");
            return records;
        }

        // Skip header row
        for (int i = 1; i < lines.Length; i++)
        {
            var fields = lines[i].Split(',');
            if (fields.Length < 5)
            {
                context.Logger.LogWarning($"CSV row {i + 1} has insufficient columns ({fields.Length}), skipping.");
                continue;
            }

            var record = new IncidentSourceRecord
            {
                RecordId = fields[0].Trim(),
                SiteId = fields[1].Trim(),
                AssetId = string.IsNullOrWhiteSpace(fields[2]) ? null : fields[2].Trim(),
                IncidentDate = fields[3].Trim(),
                Severity = fields[4].Trim(),
                IncidentType = fields.Length > 5 ? fields[5].Trim() : null,
                Description = fields.Length > 6 ? fields[6].Trim() : null,
                IsNearMiss = fields.Length > 7 && bool.TryParse(fields[7].Trim(), out var isNm) && isNm
            };

            records.Add(record);
        }

        return records;
    }

    /// <summary>
    /// Maps a validated source record to the Incident entity for RDS persistence.
    /// </summary>
    private static Incident MapToIncidentEntity(IncidentSourceRecord record, DateOnly ingestionDate)
    {
        return new Incident
        {
            IncidentId = Guid.NewGuid(),
            SiteId = Guid.Parse(record.SiteId!),
            AssetId = string.IsNullOrWhiteSpace(record.AssetId) ? null : Guid.Parse(record.AssetId),
            IncidentDate = DateOnly.Parse(record.IncidentDate!),
            Severity = record.Severity!.ToUpperInvariant(),
            IncidentType = record.IncidentType,
            Description = record.Description,
            SourceCategory = SourceCategory,
            DataQualityStatus = "VALID",
            IngestedAt = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Maps a validated source record to the NearMiss entity for RDS persistence.
    /// </summary>
    private static NearMiss MapToNearMissEntity(IncidentSourceRecord record, DateOnly ingestionDate)
    {
        return new NearMiss
        {
            NearMissId = Guid.NewGuid(),
            SiteId = Guid.Parse(record.SiteId!),
            AssetId = string.IsNullOrWhiteSpace(record.AssetId) ? null : Guid.Parse(record.AssetId),
            EventDate = DateOnly.Parse(record.IncidentDate!),
            PotentialSeverity = record.Severity!.ToUpperInvariant(),
            Description = record.Description,
            SourceCategory = SourceCategory,
            DataQualityStatus = "VALID",
            IngestedAt = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Creates a DynamoDB quality log entry from a validation failure.
    /// </summary>
    private static DataQualityLogEntry CreateQualityLogEntry(
        IncidentSourceRecord record,
        FluentValidation.Results.ValidationFailure error,
        DateOnly ingestionDate)
    {
        var logId = Guid.NewGuid().ToString();
        var ttlEpoch = DateTimeOffset.UtcNow.AddDays(90).ToUnixTimeSeconds();

        return new DataQualityLogEntry
        {
            Pk = $"{SourceCategory}#{ingestionDate:yyyy-MM-dd}",
            Sk = logId,
            SourceCategory = SourceCategory,
            IngestionDate = ingestionDate.ToString("yyyy-MM-dd"),
            LogId = logId,
            RecordId = record.RecordId ?? "unknown",
            Status = "FLAGGED",
            Severity = error.Severity == FluentValidation.Severity.Error ? "Error" : "Warning",
            Message = error.ErrorMessage,
            FieldName = error.PropertyName,
            OriginalValue = error.AttemptedValue?.ToString(),
            Ttl = ttlEpoch
        };
    }
}
