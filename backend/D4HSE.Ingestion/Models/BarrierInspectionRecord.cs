namespace D4HSE.Ingestion.Models;

/// <summary>
/// Represents a raw barrier inspection record from the S3 landing zone source file.
/// </summary>
public class BarrierInspectionRecord
{
    public string? RecordId { get; set; }
    public string? BarrierId { get; set; }
    public string? SiteId { get; set; }
    public string? AssetId { get; set; }
    public string? ObservedAt { get; set; }
    public string? RagStatus { get; set; }
    public decimal? ConditionScore { get; set; }
    public string? Notes { get; set; }
}
