namespace D4HSE.Core.Entities;

/// <summary>
/// A recorded HSE event with date, site or asset context, severity,
/// description, and risk relevance.
/// </summary>
public class Incident
{
    public Guid IncidentId { get; set; }
    public Guid SiteId { get; set; }
    public Guid? AssetId { get; set; }
    public DateOnly IncidentDate { get; set; }
    public string Severity { get; set; } = string.Empty; // LOW, MEDIUM, HIGH, CRITICAL
    public string? IncidentType { get; set; }
    public string? Description { get; set; }
    public string SourceCategory { get; set; } = string.Empty;
    public string DataQualityStatus { get; set; } = "VALID"; // VALID, FLAGGED
    public DateTimeOffset IngestedAt { get; set; }

    // Navigation properties
    public Site Site { get; set; } = null!;
    public Asset? Asset { get; set; }
}
