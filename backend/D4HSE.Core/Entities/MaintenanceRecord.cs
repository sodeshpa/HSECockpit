namespace D4HSE.Core.Entities;

/// <summary>
/// A record of asset work, condition, or maintenance activity that may
/// influence barrier health or operational risk.
/// </summary>
public class MaintenanceRecord
{
    public Guid MaintenanceId { get; set; }
    public Guid AssetId { get; set; }
    public Guid SiteId { get; set; }
    public DateOnly MaintenanceDate { get; set; }
    public string? ActivityType { get; set; }
    public string? Outcome { get; set; }
    public string? Notes { get; set; }
    public string SourceCategory { get; set; } = string.Empty;
    public string DataQualityStatus { get; set; } = "VALID"; // VALID, FLAGGED
    public DateTimeOffset IngestedAt { get; set; }

    // Navigation properties
    public Asset Asset { get; set; } = null!;
    public Site Site { get; set; } = null!;
}
