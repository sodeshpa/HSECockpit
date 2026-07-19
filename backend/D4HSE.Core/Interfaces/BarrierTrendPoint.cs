namespace D4HSE.Core.Interfaces;

/// <summary>
/// Represents a single data point in a barrier health trend,
/// used for sparkline visualisation.
/// </summary>
public class BarrierTrendPoint
{
    public DateOnly ObservedAt { get; set; }
    public string RagStatus { get; set; } = string.Empty; // GREEN, AMBER, RED
    public decimal? ConditionScore { get; set; }
    public string DataQualityStatus { get; set; } = "VALID";
}
