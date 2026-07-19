namespace D4HSE.Api.Models;

/// <summary>
/// Response DTO for a critical barrier entity.
/// Used by the Barriers API to return barrier data with quality status.
/// </summary>
public class BarrierResponse : EntityResponseBase
{
    public Guid BarrierId { get; set; }

    public string BarrierName { get; set; } = string.Empty;

    public string? BarrierType { get; set; }

    public Guid SiteId { get; set; }

    public Guid? AssetId { get; set; }

    public int CriticalityRank { get; set; }

    /// <summary>
    /// Current RAG status derived from latest health observation.
    /// Values: "GREEN", "AMBER", "RED"
    /// </summary>
    public string? CurrentRagStatus { get; set; }

    /// <summary>
    /// Date of the most recent health observation (ISO 8601).
    /// </summary>
    public DateOnly? LastAssessedDate { get; set; }
}
