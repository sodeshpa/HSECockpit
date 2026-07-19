namespace D4HSE.Api.Models;

/// <summary>
/// Response DTO for a near-miss entity.
/// Used by the Near Misses API to return near-miss data with quality status.
/// </summary>
public class NearMissResponse : EntityResponseBase
{
    public Guid NearMissId { get; set; }

    public Guid SiteId { get; set; }

    /// <summary>
    /// Date of the near-miss event (ISO 8601).
    /// </summary>
    public DateOnly EventDate { get; set; }

    /// <summary>
    /// Potential severity had the event escalated.
    /// Values: "LOW", "MEDIUM", "HIGH", "CRITICAL"
    /// </summary>
    public string? PotentialSeverity { get; set; }

    public string? Description { get; set; }
}
