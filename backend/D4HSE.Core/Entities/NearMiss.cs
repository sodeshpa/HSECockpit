namespace D4HSE.Core.Entities;

/// <summary>
/// A recorded event that did not result in an incident but indicates
/// potential risk exposure.
/// </summary>
public class NearMiss
{
    public Guid NearMissId { get; set; }
    public Guid SiteId { get; set; }
    public Guid? AssetId { get; set; }
    public DateOnly EventDate { get; set; }
    public string? PotentialSeverity { get; set; } // LOW, MEDIUM, HIGH, CRITICAL
    public string? Description { get; set; }
    public string SourceCategory { get; set; } = string.Empty;
    public string DataQualityStatus { get; set; } = "VALID"; // VALID, FLAGGED
    public DateTimeOffset IngestedAt { get; set; }

    // Navigation properties
    public Site Site { get; set; } = null!;
    public Asset? Asset { get; set; }
}
