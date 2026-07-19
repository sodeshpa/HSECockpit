namespace D4HSE.Core.Entities;

/// <summary>
/// A dated assessment or signal describing the condition, status, trend,
/// or quality of a critical barrier.
/// </summary>
public class BarrierHealthObservation
{
    public Guid ObservationId { get; set; }
    public Guid BarrierId { get; set; }
    public DateOnly ObservedAt { get; set; }
    public string RagStatus { get; set; } = string.Empty; // GREEN, AMBER, RED
    public decimal? ConditionScore { get; set; }
    public string? Notes { get; set; }
    public string SourceCategory { get; set; } = string.Empty;
    public string DataQualityStatus { get; set; } = "VALID"; // VALID, FLAGGED, CONFLICT
    public DateTimeOffset IngestedAt { get; set; }

    // Navigation properties
    public CriticalBarrier CriticalBarrier { get; set; } = null!;
}
