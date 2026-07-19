using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using D4HSE.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace D4HSE.Infrastructure.AwsServices;

/// <summary>
/// DynamoDB implementation of the data quality log repository.
/// Uses the low-level IAmazonDynamoDB client to put and query items.
/// Table name is read from configuration key "DynamoDB:QualityLogTable".
/// TTL is computed as Unix epoch 90 days from now.
/// </summary>
public class DataQualityLogRepository : IDataQualityLogRepository
{
    private const int TtlDays = 90;
    private const int MaxBatchWriteSize = 25; // DynamoDB batch write limit

    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly string _tableName;

    public DataQualityLogRepository(IAmazonDynamoDB dynamoDb, IConfiguration configuration)
    {
        _dynamoDb = dynamoDb;
        _tableName = configuration["DynamoDB:QualityLogTable"]
            ?? throw new InvalidOperationException("DynamoDB:QualityLogTable configuration is missing.");
    }

    public async Task LogAsync(D4HSE.Core.Interfaces.DataQualityLogEntry entry, CancellationToken ct)
    {
        var item = ToAttributeMap(entry);

        var request = new PutItemRequest
        {
            TableName = _tableName,
            Item = item
        };

        await _dynamoDb.PutItemAsync(request, ct);
    }

    public async Task LogBatchAsync(IEnumerable<D4HSE.Core.Interfaces.DataQualityLogEntry> entries, CancellationToken ct)
    {
        var entryList = entries.ToList();
        if (entryList.Count == 0) return;

        // DynamoDB BatchWriteItem supports max 25 items per request
        foreach (var batch in Chunk(entryList, MaxBatchWriteSize))
        {
            var writeRequests = batch.Select(entry => new WriteRequest
            {
                PutRequest = new PutRequest { Item = ToAttributeMap(entry) }
            }).ToList();

            var request = new BatchWriteItemRequest
            {
                RequestItems = new Dictionary<string, List<WriteRequest>>
                {
                    [_tableName] = writeRequests
                }
            };

            var response = await _dynamoDb.BatchWriteItemAsync(request, ct);

            // Retry unprocessed items (simple single retry)
            if (response.UnprocessedItems.Count > 0 && response.UnprocessedItems.ContainsKey(_tableName))
            {
                var retryRequest = new BatchWriteItemRequest
                {
                    RequestItems = response.UnprocessedItems
                };
                await _dynamoDb.BatchWriteItemAsync(retryRequest, ct);
            }
        }
    }

    public async Task<IReadOnlyList<D4HSE.Core.Interfaces.DataQualityLogEntry>> GetBySourceAndDateAsync(
        string sourceCategory, DateOnly date, CancellationToken ct)
    {
        var pk = $"{sourceCategory}#{date:yyyy-MM-dd}";

        var request = new QueryRequest
        {
            TableName = _tableName,
            KeyConditionExpression = "pk = :pk",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":pk"] = new AttributeValue { S = pk }
            }
        };

        var response = await _dynamoDb.QueryAsync(request, ct);

        return response.Items.Select(FromAttributeMap).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<D4HSE.Core.Interfaces.DataQualityLogEntry>> GetByDateRangeAsync(
        DateOnly fromDate, DateOnly toDate, string? sourceCategory, CancellationToken ct)
    {
        var categories = sourceCategory is not null
            ? new[] { sourceCategory }
            : new[] { "barrier_inspections", "incidents", "maintenance" };

        var results = new List<D4HSE.Core.Interfaces.DataQualityLogEntry>();

        foreach (var category in categories)
        {
            for (var date = fromDate; date <= toDate; date = date.AddDays(1))
            {
                var entries = await GetBySourceAndDateAsync(category, date, ct);
                results.AddRange(entries);
            }
        }

        return results.AsReadOnly();
    }

    private static Dictionary<string, AttributeValue> ToAttributeMap(D4HSE.Core.Interfaces.DataQualityLogEntry entry)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            ["pk"] = new AttributeValue { S = entry.Pk },
            ["sk"] = new AttributeValue { S = entry.Sk },
            ["source_category"] = new AttributeValue { S = entry.SourceCategory },
            ["ingestion_date"] = new AttributeValue { S = entry.IngestionDate },
            ["log_id"] = new AttributeValue { S = entry.LogId },
            ["record_id"] = new AttributeValue { S = entry.RecordId },
            ["status"] = new AttributeValue { S = entry.Status },
            ["severity"] = new AttributeValue { S = entry.Severity },
            ["message"] = new AttributeValue { S = entry.Message },
            ["ttl"] = new AttributeValue { N = entry.Ttl.ToString() }
        };

        if (!string.IsNullOrEmpty(entry.FieldName))
        {
            item["field_name"] = new AttributeValue { S = entry.FieldName };
        }

        if (!string.IsNullOrEmpty(entry.OriginalValue))
        {
            item["original_value"] = new AttributeValue { S = entry.OriginalValue };
        }

        return item;
    }

    private static D4HSE.Core.Interfaces.DataQualityLogEntry FromAttributeMap(Dictionary<string, AttributeValue> item)
    {
        return new D4HSE.Core.Interfaces.DataQualityLogEntry
        {
            Pk = item.GetValueOrDefault("pk")?.S ?? string.Empty,
            Sk = item.GetValueOrDefault("sk")?.S ?? string.Empty,
            SourceCategory = item.GetValueOrDefault("source_category")?.S ?? string.Empty,
            IngestionDate = item.GetValueOrDefault("ingestion_date")?.S ?? string.Empty,
            LogId = item.GetValueOrDefault("log_id")?.S ?? string.Empty,
            RecordId = item.GetValueOrDefault("record_id")?.S ?? string.Empty,
            Status = item.GetValueOrDefault("status")?.S ?? string.Empty,
            Severity = item.GetValueOrDefault("severity")?.S ?? string.Empty,
            Message = item.GetValueOrDefault("message")?.S ?? string.Empty,
            FieldName = item.GetValueOrDefault("field_name")?.S,
            OriginalValue = item.GetValueOrDefault("original_value")?.S,
            Ttl = long.TryParse(item.GetValueOrDefault("ttl")?.N, out var ttl) ? ttl : 0
        };
    }

    private static IEnumerable<List<T>> Chunk<T>(List<T> source, int chunkSize)
    {
        for (int i = 0; i < source.Count; i += chunkSize)
        {
            yield return source.GetRange(i, Math.Min(chunkSize, source.Count - i));
        }
    }
}
