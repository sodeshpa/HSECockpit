namespace D4HSE.Core.Interfaces;

/// <summary>
/// Aggregated incident summary for a given date range,
/// including total count and breakdown by severity level.
/// </summary>
public class IncidentSummary
{
    public int TotalCount { get; set; }
    public int LowCount { get; set; }
    public int MediumCount { get; set; }
    public int HighCount { get; set; }
    public int CriticalCount { get; set; }
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
}
