namespace D4HSE.Api.Models;

/// <summary>
/// Response DTO for a single barrier health trend data point.
/// Used by the sparkline/trend endpoint.
/// </summary>
public class BarrierTrendResponse
{
    public DateOnly ObservedAt { get; set; }
    public string RagStatus { get; set; } = string.Empty;
    public decimal? ConditionScore { get; set; }
    public string DataQualityStatus { get; set; } = "VALID";
}
