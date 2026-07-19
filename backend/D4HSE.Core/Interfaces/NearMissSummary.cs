namespace D4HSE.Core.Interfaces;

/// <summary>
/// Aggregated near-miss summary for a given date range,
/// including trend comparison against the prior period.
/// </summary>
public class NearMissSummary
{
    public int TotalCount { get; set; }
    public int PriorPeriodCount { get; set; }
    public string TrendDirection { get; set; } = "STABLE"; // "UP", "DOWN", "STABLE"
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
}
