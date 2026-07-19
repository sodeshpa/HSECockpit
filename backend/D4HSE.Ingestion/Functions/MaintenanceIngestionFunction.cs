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
/// Lambda function for ingesting asset maintenance records from the S3 landing zone.
/// Triggered by EventBridge Scheduler on a configurable cron schedule.
/// 
/// Flow:
///   1. Read maintenance records from S3 landing zone bucket (maintenance/ prefix)
///   2. Validate each record using FluentValidation
///   3. Valid records → persist to RDS as MaintenanceRecord entities
///   4. Invalid records → log to DynamoDB DataQualityLog with business-readable messages
///   5. Update last-ingestion timestamp in Parameter Store
///   6. Trigger embedding generation for valid records
/// </summary>
public class MaintenanceIngestionFunction
{
    private static readonly ServiceProvider ServiceProvider;

    private const string SourceCategory = "maintenance";
    private const string S3Prefix = "maintenance/";

    static MaintenanceIngestionFunction()
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
        services.AddScoped<IValidator<MaintenanceSourceRecord>, MaintenanceSourceRecordValidator>();

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
        context.Logger.LogInformation($"MaintenanceIngestionFunction invoked at {input.Time}. Request ID: {context.AwsRequestId}");

        var s3Client = ServiceProvider.GetRequiredService<IAmazonS3>();
        var ssmClient = ServiceProvider.GetRequiredService<IAmazonSimpleSystemsManagement>();

        using var scope = ServiceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HseCockpitDbContext>();
        var validator = scope.ServiceProvider.GetRequiredService<IValidator<MaintenanceSourceRecord>>();
        var qualityLogRepo = scope.ServiceProvider.GetRequiredService<IDataQualityLogRepository>();

        var bucketName = System.Environment.GetEnvironmentVariable("S3__LANDINGZONE_BUCKET")
            ?? throw new InvalidOperationException("S3__LANDINGZONE_BUCKET environment variable is not set.");
        var timestampKey = System.Environment.GetEnvironmentVariable("PARAMETERSTORE__INGESTION_TIMESTAMP_KEY")
            ?? "/hse/ingestion/last-run";

        var ingestionDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var validRecords = new List<MaintenanceRecord>();
        var qualityLogEntries = new List<DataQualityLogEntry>();
        int totalProcessed = 0;

        try
        {
            // List objects in the S3 maintenance prefix
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
                        var entity = MapToEntity(record, ingestionDate);
                        validRecords.Add(entity);
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

            // Persist valid records to RDS
            if (validRecords.Count > 0)
            {
                await dbContext.MaintenanceRecords.AddRangeAsync(validRecords);
                await dbContext.SaveChangesAsync();
                context.Logger.LogInformation($"Persisted {validRecords.Count} valid maintenance records to RDS.");
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
            if (validRecords.Count > 0)
            {
                await TriggerEmbeddingGenerationAsync(validRecords, ingestionDate, context);
            }

            context.Logger.LogInformation(
                $"Maintenance ingestion complete. Total processed: {totalProcessed}, Valid: {validRecords.Count}, Invalid: {qualityLogEntries.Count}");
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Maintenance ingestion failed: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Asynchronously invokes the embedding generation Lambda to produce vector embeddings
    /// for newly ingested maintenance records.
    /// </summary>
    private static async Task TriggerEmbeddingGenerationAsync(
        List<MaintenanceRecord> validRecords,
        DateOnly ingestionDate,
        ILambdaContext context)
    {
        var lambdaClient = ServiceProvider.GetRequiredService<Amazon.Lambda.IAmazonLambda>();

        var embeddingFunctionName = System.Environment.GetEnvironmentVariable("EMBEDDING_FUNCTION_NAME")
            ?? "HSECockpit-EmbeddingGeneration";

        var embeddingRequest = new EmbeddingRequest
        {
            SourceCategory = SourceCategory,
            RecordIds = validRecords.Select(r => r.MaintenanceId.ToString()).ToList(),
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
                $"Triggered embedding generation for {validRecords.Count} records. " +
                $"Lambda: {embeddingFunctionName}, StatusCode: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            // Embedding generation failure should not fail the ingestion pipeline.
            // The records are already persisted; embeddings can be regenerated later.
            context.Logger.LogWarning(
                $"Failed to trigger embedding generation Lambda '{embeddingFunctionName}': {ex.Message}. " +
                $"Records are persisted but embeddings may need manual regeneration.");
        }
    }

    /// <summary>
    /// Reads and parses maintenance records from a JSON or CSV file in S3.
    /// </summary>
    private static async Task<List<MaintenanceSourceRecord>> ReadRecordsFromS3Async(
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
    /// Parses maintenance records from JSON content.
    /// Supports both an array of records and a single record.
    /// </summary>
    private static List<MaintenanceSourceRecord> ParseJsonRecords(string content, ILambdaContext context)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var records = JsonSerializer.Deserialize<List<MaintenanceSourceRecord>>(content, options);
            return records ?? [];
        }
        catch (JsonException ex)
        {
            context.Logger.LogWarning($"Failed to parse JSON as array, attempting single record: {ex.Message}");
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var single = JsonSerializer.Deserialize<MaintenanceSourceRecord>(content, options);
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
    /// Parses maintenance records from CSV content.
    /// Expected columns: RecordId,AssetId,SiteId,MaintenanceDate,ActivityType,Outcome,Notes
    /// </summary>
    private static List<MaintenanceSourceRecord> ParseCsvRecords(string content, ILambdaContext context)
    {
        var records = new List<MaintenanceSourceRecord>();
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

            var record = new MaintenanceSourceRecord
            {
                RecordId = fields[0].Trim(),
                AssetId = fields[1].Trim(),
                SiteId = fields[2].Trim(),
                MaintenanceDate = fields[3].Trim(),
                ActivityType = fields[4].Trim(),
                Outcome = fields.Length > 5 ? fields[5].Trim() : null,
                Notes = fields.Length > 6 ? fields[6].Trim() : null
            };

            records.Add(record);
        }

        return records;
    }

    /// <summary>
    /// Maps a validated source record to the MaintenanceRecord entity for RDS persistence.
    /// </summary>
    private static MaintenanceRecord MapToEntity(MaintenanceSourceRecord record, DateOnly ingestionDate)
    {
        return new MaintenanceRecord
        {
            MaintenanceId = Guid.NewGuid(),
            AssetId = Guid.Parse(record.AssetId!),
            SiteId = Guid.Parse(record.SiteId!),
            MaintenanceDate = DateOnly.Parse(record.MaintenanceDate!),
            ActivityType = record.ActivityType,
            Outcome = record.Outcome,
            Notes = record.Notes,
            SourceCategory = SourceCategory,
            DataQualityStatus = "VALID",
            IngestedAt = DateTimeOffset.UtcNow
        };
    }

    /// <summary>
    /// Creates a DynamoDB quality log entry from a validation failure.
    /// </summary>
    private static DataQualityLogEntry CreateQualityLogEntry(
        MaintenanceSourceRecord record,
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
