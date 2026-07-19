namespace D4HSE.Ingestion.Models;

/// <summary>
/// Represents a raw maintenance record from the S3 landing zone source file.
/// </summary>
public class MaintenanceSourceRecord
{
    public string? RecordId { get; set; }
    public string? AssetId { get; set; }
    public string? SiteId { get; set; }
    public string? MaintenanceDate { get; set; }
    public string? ActivityType { get; set; }
    public string? Outcome { get; set; }
    public string? Notes { get; set; }
}
