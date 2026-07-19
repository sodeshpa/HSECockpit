using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using D4HSE.Core.Interfaces;

namespace D4HSE.Infrastructure.Repositories;

/// <summary>
/// DynamoDB-backed implementation of IDataQualityLogRepository.
/// Writes validation failures to the DataQualityLog table.
/// </summary>
public class DataQualityLogRepository : IDataQualityLogRepository
{
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly string _tableName;

    public DataQualityLogRepository(IAmazonDynamoDB dynamoDbClient, string tableName)
    {
        _dynamoDbClient = dynamoDbClient;
        _tableName = tableName;
    }

    public async Task LogAsync(DataQualityLogEntry entry, CancellationToken ct)
    {
        var item = MapToAttributes(entry);

        await _dynamoDbClient.PutItemAsync(new PutItemRequest
        {
            TableName = _tableName,
            Item = item
        }, ct);
    }

    public async Task LogBatchAsync(IEnumerable<DataQualityLogEntry> entries, CancellationToken ct)
    {
        var entryList = entries.ToList();
        if (entryList.Count == 0) return;

        // DynamoDB BatchWriteItem supports max 25 items per request
        var batches = entryList.Chunk(25);

        foreach (var batch in batches)
        {
            var writeRequests = batch.Select(entry => new WriteRequest
            {
                PutRequest = new PutRequest
                {
                    Item = MapToAttributes(entry)
                }
            }).ToList();

            var request = new BatchWriteItemRequest
            {
                RequestItems = new Dictionary<string, List<WriteRequest>>
                {
                    { _tableName, writeRequests }
                }
            };

            await _dynamoDbClient.BatchWriteItemAsync(request, ct);
        }
    }

    public async Task<IReadOnlyList<DataQualityLogEntry>> GetBySourceAndDateAsync(
        string sourceCategory, DateOnly date, CancellationToken ct)
    {
        var pk = $"{sourceCategory}#{date:yyyy-MM-dd}";

        var queryRequest = new QueryRequest
        {
            TableName = _tableName,
            KeyConditionExpression = "pk = :pk",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":pk", new AttributeValue(pk) }
            }
        };

        var response = await _dynamoDbClient.QueryAsync(queryRequest, ct);

        return response.Items.Select(MapFromAttributes).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<DataQualityLogEntry>> GetByDateRangeAsync(
        DateOnly fromDate, DateOnly toDate, string? sourceCategory, CancellationToken ct)
    {
        var categories = sourceCategory is not null
            ? new[] { sourceCategory }
            : new[] { "barrier_inspections", "incidents", "maintenance" };

        var results = new List<DataQualityLogEntry>();

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

    private static Dictionary<string, AttributeValue> MapToAttributes(DataQualityLogEntry entry)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            { "pk", new AttributeValue(entry.Pk) },
            { "sk", new AttributeValue(entry.Sk) },
            { "source_category", new AttributeValue(entry.SourceCategory) },
            { "ingestion_date", new AttributeValue(entry.IngestionDate) },
            { "log_id", new AttributeValue(entry.LogId) },
            { "record_id", new AttributeValue(entry.RecordId) },
            { "status", new AttributeValue(entry.Status) },
            { "severity", new AttributeValue(entry.Severity) },
            { "message", new AttributeValue(entry.Message) },
            { "ttl", new AttributeValue { N = entry.Ttl.ToString() } }
        };

        if (!string.IsNullOrEmpty(entry.FieldName))
            item["field_name"] = new AttributeValue(entry.FieldName);

        if (!string.IsNullOrEmpty(entry.OriginalValue))
            item["original_value"] = new AttributeValue(entry.OriginalValue);

        return item;
    }

    private static DataQualityLogEntry MapFromAttributes(Dictionary<string, AttributeValue> item)
    {
        return new DataQualityLogEntry
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
}
