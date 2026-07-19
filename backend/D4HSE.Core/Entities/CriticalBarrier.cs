namespace D4HSE.Core.Entities;

/// <summary>
/// A safety control or protective measure monitored for health status,
/// degradation, trend, and operational importance.
/// </summary>
public class CriticalBarrier
{
    public Guid BarrierId { get; set; }
    public Guid SiteId { get; set; }
    public Guid? AssetId { get; set; }
    public string BarrierName { get; set; } = string.Empty;
    public string? BarrierType { get; set; }
    public int CriticalityRank { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }

    // Navigation properties
    public Site Site { get; set; } = null!;
    public Asset? Asset { get; set; }
    public ICollection<BarrierHealthObservation> HealthObservations { get; set; } = new List<BarrierHealthObservation>();
    public ICollection<RiskItem> RiskItems { get; set; } = new List<RiskItem>();
}
