namespace D4HSE.Core.Entities;

/// <summary>
/// Equipment, unit, or operational area associated with safety barriers,
/// maintenance records, incidents, and risk exposure.
/// </summary>
public class Asset
{
    public Guid AssetId { get; set; }
    public Guid SiteId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string? AssetType { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    // Navigation properties
    public Site Site { get; set; } = null!;
    public ICollection<CriticalBarrier> CriticalBarriers { get; set; } = new List<CriticalBarrier>();
    public ICollection<MaintenanceRecord> MaintenanceRecords { get; set; } = new List<MaintenanceRecord>();
}
