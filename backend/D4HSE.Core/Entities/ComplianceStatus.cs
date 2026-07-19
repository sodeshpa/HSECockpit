namespace D4HSE.Core.Entities;

/// <summary>
/// A summary indicator showing whether monitored sites or assets satisfy
/// expected HSE compliance conditions within the MVP scope.
/// </summary>
public class ComplianceStatus
{
    public Guid ComplianceId { get; set; }
    public Guid SiteId { get; set; }
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public string Status { get; set; } = string.Empty; // COMPLIANT, NON_COMPLIANT, UNKNOWN
    public string? Notes { get; set; }
    public DateTimeOffset AssessedAt { get; set; }

    // Navigation properties
    public Site Site { get; set; } = null!;
}
