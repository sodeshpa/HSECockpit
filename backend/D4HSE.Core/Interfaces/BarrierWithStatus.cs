namespace D4HSE.Core.Interfaces;

/// <summary>
/// Domain model representing a critical barrier with its current RAG status
/// derived from the latest health observation.
/// </summary>
public class BarrierWithStatus
{
    public Guid BarrierId { get; set; }
    public string BarrierName { get; set; } = string.Empty;
    public string? BarrierType { get; set; }
    public Guid SiteId { get; set; }
    public Guid? AssetId { get; set; }
    public int CriticalityRank { get; set; }
    public string? CurrentRagStatus { get; set; } // from latest observation
    public DateOnly? LastAssessedDate { get; set; }
    public string DataQualityStatus { get; set; } = "VALID"; // aggregated from observations
}
