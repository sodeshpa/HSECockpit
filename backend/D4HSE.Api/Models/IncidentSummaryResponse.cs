namespace D4HSE.Api.Models;

/// <summary>
/// Response DTO for the incident summary endpoint.
/// Returns total count and breakdown by severity level for a date range.
/// </summary>
public class IncidentSummaryResponse
{
    public int TotalCount { get; set; }
    public int LowCount { get; set; }
    public int MediumCount { get; set; }
    public int HighCount { get; set; }
    public int CriticalCount { get; set; }
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
}
