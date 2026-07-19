namespace D4HSE.Api.Models;

/// <summary>
/// Response DTO for the data quality summary endpoint.
/// Provides counts of valid/flagged/conflict records broken down by source category.
/// </summary>
public class DataQualitySummaryResponse
{
    /// <summary>
    /// Quality summary broken down by source category.
    /// </summary>
    public List<CategoryQualitySummary> Categories { get; set; } = new();

    /// <summary>
    /// ISO 8601 timestamp of the last successful ingestion run.
    /// </summary>
    public string LastIngestionTimestamp { get; set; } = string.Empty;

    /// <summary>
    /// Freshness status based on last ingestion timestamp vs configured threshold.
    /// Values: "FRESH", "STALE", "UNKNOWN"
    /// </summary>
    public string FreshnessStatus { get; set; } = "UNKNOWN";
}

/// <summary>
/// Quality summary for a single source category.
/// </summary>
public class CategoryQualitySummary
{
    /// <summary>
    /// The source category (e.g., "barrier_inspections", "incidents", "maintenance").
    /// </summary>
    public string SourceCategory { get; set; } = string.Empty;

    /// <summary>
    /// Count of records with VALID status.
    /// </summary>
    public int ValidCount { get; set; }

    /// <summary>
    /// Count of records with FLAGGED status.
    /// </summary>
    public int FlaggedCount { get; set; }

    /// <summary>
    /// Count of records with CONFLICT status.
    /// </summary>
    public int ConflictCount { get; set; }

    /// <summary>
    /// Total records across all statuses for this category.
    /// </summary>
    public int TotalCount { get; set; }
}
