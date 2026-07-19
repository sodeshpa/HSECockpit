namespace D4HSE.Api.Models;

/// <summary>
/// Response DTO for an incident entity.
/// Used by the Incidents API to return incident data with quality status.
/// </summary>
public class IncidentResponse : EntityResponseBase
{
    public Guid IncidentId { get; set; }

    public Guid SiteId { get; set; }

    /// <summary>
    /// Date of the incident (ISO 8601).
    /// </summary>
    public DateOnly IncidentDate { get; set; }

    /// <summary>
    /// Severity level of the incident.
    /// Values: "LOW", "MEDIUM", "HIGH", "CRITICAL"
    /// </summary>
    public string Severity { get; set; } = string.Empty;

    public string? IncidentType { get; set; }

    public string? Description { get; set; }
}
