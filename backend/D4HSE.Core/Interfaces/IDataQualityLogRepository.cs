namespace D4HSE.Core.Interfaces;

/// <summary>
/// Repository interface for data quality log operations against DynamoDB.
/// </summary>
public interface IDataQualityLogRepository
{
    /// <summary>
    /// Logs a single data quality entry.
    /// </summary>
    Task LogAsync(DataQualityLogEntry entry, CancellationToken ct);

    /// <summary>
    /// Logs a batch of data quality entries.
    /// </summary>
    Task LogBatchAsync(IEnumerable<DataQualityLogEntry> entries, CancellationToken ct);

    /// <summary>
    /// Retrieves all log entries for a given source category and date.
    /// </summary>
    Task<IReadOnlyList<DataQualityLogEntry>> GetBySourceAndDateAsync(string sourceCategory, DateOnly date, CancellationToken ct);

    /// <summary>
    /// Retrieves log entries for a date range, optionally filtered by source category.
    /// Used for summary/count operations.
    /// </summary>
    Task<IReadOnlyList<DataQualityLogEntry>> GetByDateRangeAsync(DateOnly fromDate, DateOnly toDate, string? sourceCategory, CancellationToken ct);
}

/// <summary>
/// Represents a single data quality log entry.
/// Defined in Core to keep the interface dependency-free from infrastructure concerns.
/// </summary>
public class DataQualityLogEntry
{
    /// <summary>
    /// Partition key: source_category#ingestion_date (e.g., "barrier_inspections#2024-01-15").
    /// </summary>
    public string Pk { get; set; } = string.Empty;

    /// <summary>
    /// Sort key: UUID log_id.
    /// </summary>
    public string Sk { get; set; } = string.Empty;

    /// <summary>
    /// Source category: barrier_inspections, incidents, maintenance.
    /// </summary>
    public string SourceCategory { get; set; } = string.Empty;

    /// <summary>
    /// ISO 8601 date of ingestion (e.g., "2024-01-15").
    /// </summary>
    public string IngestionDate { get; set; } = string.Empty;

    /// <summary>
    /// Unique identifier for this log entry (UUID).
    /// </summary>
    public string LogId { get; set; } = string.Empty;

    /// <summary>
    /// Reference to the source record that was validated.
    /// </summary>
    public string RecordId { get; set; } = string.Empty;

    /// <summary>
    /// Validation status: VALID, FLAGGED, CONFLICT.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Severity level: Error, Warning, Info.
    /// </summary>
    public string Severity { get; set; } = string.Empty;

    /// <summary>
    /// Business-readable validation message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The field that failed validation (optional).
    /// </summary>
    public string? FieldName { get; set; }

    /// <summary>
    /// The value that failed validation (optional).
    /// </summary>
    public string? OriginalValue { get; set; }

    /// <summary>
    /// Unix epoch timestamp for DynamoDB TTL (90 days from ingestion).
    /// </summary>
    public long Ttl { get; set; }
}
