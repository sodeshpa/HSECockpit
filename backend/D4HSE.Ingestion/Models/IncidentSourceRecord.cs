namespace D4HSE.Ingestion.Models;

/// <summary>
/// Represents a raw incident or near-miss record from the S3 landing zone source file.
/// The <see cref="IsNearMiss"/> flag determines whether this record is persisted as an
/// Incident or NearMiss entity in RDS.
/// </summary>
public class IncidentSourceRecord
{
    public string? RecordId { get; set; }
    public string? SiteId { get; set; }
    public string? AssetId { get; set; }
    public string? IncidentDate { get; set; }
    public string? Severity { get; set; }
    public string? IncidentType { get; set; }
    public string? Description { get; set; }
    public bool IsNearMiss { get; set; }
}
