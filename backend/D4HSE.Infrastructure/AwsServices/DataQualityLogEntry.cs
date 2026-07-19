using Amazon.DynamoDBv2.DataModel;

namespace D4HSE.Infrastructure.AwsServices;

/// <summary>
/// Represents a single data quality log entry stored in DynamoDB.
/// Partition key format: source_category#ingestion_date (e.g., barrier_inspections#2024-01-15).
/// Sort key: UUID log_id.
/// TTL: 90 days from ingestion.
/// </summary>
[DynamoDBTable("DataQualityLog")]
public class DataQualityLogEntry
{
    /// <summary>
    /// Partition key: source_category#ingestion_date (e.g., "barrier_inspections#2024-01-15").
    /// </summary>
    [DynamoDBHashKey("pk")]
    public string Pk { get; set; } = string.Empty;

    /// <summary>
    /// Sort key: UUID log_id.
    /// </summary>
    [DynamoDBRangeKey("sk")]
    public string Sk { get; set; } = string.Empty;

    /// <summary>
    /// Source category: barrier_inspections, incidents, maintenance.
    /// </summary>
    [DynamoDBProperty("source_category")]
    public string SourceCategory { get; set; } = string.Empty;

    /// <summary>
    /// ISO 8601 date of ingestion (e.g., "2024-01-15").
    /// </summary>
    [DynamoDBProperty("ingestion_date")]
    public string IngestionDate { get; set; } = string.Empty;

    /// <summary>
    /// Unique identifier for this log entry (UUID).
    /// </summary>
    [DynamoDBProperty("log_id")]
    public string LogId { get; set; } = string.Empty;

    /// <summary>
    /// Reference to the source record that was validated.
    /// </summary>
    [DynamoDBProperty("record_id")]
    public string RecordId { get; set; } = string.Empty;

    /// <summary>
    /// Validation status: VALID, FLAGGED, CONFLICT.
    /// </summary>
    [DynamoDBProperty("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Severity level: Error, Warning, Info.
    /// </summary>
    [DynamoDBProperty("severity")]
    public string Severity { get; set; } = string.Empty;

    /// <summary>
    /// Business-readable validation message.
    /// </summary>
    [DynamoDBProperty("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The field that failed validation (optional).
    /// </summary>
    [DynamoDBProperty("field_name")]
    public string? FieldName { get; set; }

    /// <summary>
    /// The value that failed validation (optional).
    /// </summary>
    [DynamoDBProperty("original_value")]
    public string? OriginalValue { get; set; }

    /// <summary>
    /// Unix epoch timestamp for DynamoDB TTL (90 days from ingestion).
    /// </summary>
    [DynamoDBProperty("ttl")]
    public long Ttl { get; set; }
}
