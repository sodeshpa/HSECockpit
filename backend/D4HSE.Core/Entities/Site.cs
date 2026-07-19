namespace D4HSE.Core.Entities;

/// <summary>
/// An operational location or facility where HSE barriers, incidents,
/// near misses, assets, risks, and compliance status are monitored.
/// </summary>
public class Site
{
    public Guid SiteId { get; set; }
    public string SiteName { get; set; } = string.Empty;
    public string? Region { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    // Navigation properties
    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
    public ICollection<CriticalBarrier> CriticalBarriers { get; set; } = new List<CriticalBarrier>();
    public ICollection<Incident> Incidents { get; set; } = new List<Incident>();
    public ICollection<NearMiss> NearMisses { get; set; } = new List<NearMiss>();
    public ICollection<MaintenanceRecord> MaintenanceRecords { get; set; } = new List<MaintenanceRecord>();
    public ICollection<RiskItem> RiskItems { get; set; } = new List<RiskItem>();
    public ICollection<ComplianceStatus> ComplianceStatuses { get; set; } = new List<ComplianceStatus>();
}
