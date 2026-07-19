namespace D4HSE.Api.Models;

/// <summary>
/// Response DTO for the near-miss summary endpoint.
/// Returns total count, prior period comparison, and trend direction for a date range.
/// </summary>
public class NearMissSummaryResponse
{
    public int TotalCount { get; set; }
    public int PriorPeriodCount { get; set; }

    /// <summary>
    /// Trend direction compared to prior period. Values: "UP", "DOWN", "STABLE"
    /// </summary>
    public string TrendDirection { get; set; } = "STABLE";

    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
}
