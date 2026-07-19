namespace D4HSE.Core.Entities;

/// <summary>
/// An identified risk associated with a site, asset, barrier,
/// incident pattern, or compliance concern.
/// </summary>
public class RiskItem
{
    public Guid RiskId { get; set; }
    public Guid SiteId { get; set; }
    public Guid? AssetId { get; set; }
    public Guid? BarrierId { get; set; }
    public string RiskDescription { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty; // LOW, MEDIUM, HIGH, CRITICAL
    public string Status { get; set; } = "OPEN"; // OPEN, CLOSED, MONITORING
    public DateOnly? IdentifiedAt { get; set; }
    public DateOnly? ResolvedAt { get; set; }

    // Navigation properties
    public Site Site { get; set; } = null!;
    public Asset? Asset { get; set; }
    public CriticalBarrier? CriticalBarrier { get; set; }
}
