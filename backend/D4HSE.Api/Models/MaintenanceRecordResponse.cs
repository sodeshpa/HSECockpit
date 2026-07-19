namespace D4HSE.Api.Models;

/// <summary>
/// Response DTO for a maintenance record entity.
/// Used by the Maintenance API to return maintenance data with quality status.
/// </summary>
public class MaintenanceRecordResponse : EntityResponseBase
{
    public Guid MaintenanceId { get; set; }

    public Guid AssetId { get; set; }

    public Guid SiteId { get; set; }

    /// <summary>
    /// Date the maintenance activity was performed (ISO 8601).
    /// </summary>
    public DateOnly MaintenanceDate { get; set; }

    public string? ActivityType { get; set; }

    public string? Outcome { get; set; }
}
