using D4HSE.Core.Interfaces;

namespace D4HSE.Services.Services;

/// <summary>
/// Business logic for data quality summary operations.
/// Reads from DynamoDB DataQualityLog and Parameter Store for freshness information.
/// </summary>
public class DataQualityService
{
    private const string LastIngestionTimestampKey = "/hse/ingestion/last-run";
    private const string FreshnessThresholdHoursKey = "/hse/ingestion/freshness-threshold-hours";
    private const int DefaultFreshnessThresholdHours = 12;

    private static readonly string[] AllCategories =
    {
        "barrier_inspections",
        "incidents",
        "maintenance"
    };

    private readonly IDataQualityLogRepository _qualityLogRepository;
    private readonly IParameterStoreService _parameterStoreService;

    public DataQualityService(
        IDataQualityLogRepository qualityLogRepository,
        IParameterStoreService parameterStoreService)
    {
        _qualityLogRepository = qualityLogRepository;
        _parameterStoreService = parameterStoreService;
    }

    /// <summary>
    /// Retrieves a summary of data quality counts by category, plus freshness status.
    /// </summary>
    /// <param name="siteId">Optional site filter (not applied at DynamoDB level — filtered in memory if provided).</param>
    /// <param name="fromDate">Optional start date for the query range. Defaults to 30 days ago.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<DataQualitySummaryResult> GetSummaryAsync(
        string? siteId, DateOnly? fromDate, CancellationToken ct)
    {
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var startDate = fromDate ?? endDate.AddDays(-30);

        // Retrieve all log entries across the date range
        var entries = await _qualityLogRepository.GetByDateRangeAsync(startDate, endDate, null, ct);

        // Build category summaries
        var categorySummaries = AllCategories.Select(category =>
        {
            var categoryEntries = entries.Where(e => e.SourceCategory == category);

            var validCount = categoryEntries.Count(e =>
                e.Status.Equals("VALID", StringComparison.OrdinalIgnoreCase));
            var flaggedCount = categoryEntries.Count(e =>
                e.Status.Equals("FLAGGED", StringComparison.OrdinalIgnoreCase));
            var conflictCount = categoryEntries.Count(e =>
                e.Status.Equals("CONFLICT", StringComparison.OrdinalIgnoreCase));

            return new CategoryQualitySummaryResult
            {
                SourceCategory = category,
                ValidCount = validCount,
                FlaggedCount = flaggedCount,
                ConflictCount = conflictCount,
                TotalCount = validCount + flaggedCount + conflictCount
            };
        }).ToList();

        // Retrieve last ingestion timestamp from Parameter Store
        var lastIngestionTimestamp = await _parameterStoreService.GetParameterAsync(
            LastIngestionTimestampKey, ct);

        // Compute freshness status
        var freshnessStatus = await ComputeFreshnessStatusAsync(lastIngestionTimestamp, ct);

        return new DataQualitySummaryResult
        {
            Categories = categorySummaries,
            LastIngestionTimestamp = lastIngestionTimestamp ?? string.Empty,
            FreshnessStatus = freshnessStatus
        };
    }

    private async Task<string> ComputeFreshnessStatusAsync(string? lastIngestionTimestamp, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(lastIngestionTimestamp))
        {
            return "UNKNOWN";
        }

        if (!DateTime.TryParse(lastIngestionTimestamp, null,
                System.Globalization.DateTimeStyles.RoundtripKind, out var lastRun))
        {
            return "UNKNOWN";
        }

        // Read freshness threshold from Parameter Store
        var thresholdStr = await _parameterStoreService.GetParameterAsync(
            FreshnessThresholdHoursKey, ct);
        var thresholdHours = int.TryParse(thresholdStr, out var parsed)
            ? parsed
            : DefaultFreshnessThresholdHours;

        var hoursSinceLastRun = (DateTime.UtcNow - lastRun.ToUniversalTime()).TotalHours;

        return hoursSinceLastRun <= thresholdHours ? "FRESH" : "STALE";
    }
}

/// <summary>
/// Result DTO from the DataQualityService (domain layer).
/// </summary>
public class DataQualitySummaryResult
{
    public List<CategoryQualitySummaryResult> Categories { get; set; } = new();
    public string LastIngestionTimestamp { get; set; } = string.Empty;
    public string FreshnessStatus { get; set; } = "UNKNOWN";
}

/// <summary>
/// Category-level quality counts.
/// </summary>
public class CategoryQualitySummaryResult
{
    public string SourceCategory { get; set; } = string.Empty;
    public int ValidCount { get; set; }
    public int FlaggedCount { get; set; }
    public int ConflictCount { get; set; }
    public int TotalCount { get; set; }
}
