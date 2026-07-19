using D4HSE.Core.Interfaces;

namespace D4HSE.Services.Services;

/// <summary>
/// Business logic layer for incident and near-miss operations.
/// Orchestrates repository calls and applies additional business rules.
/// </summary>
public class IncidentService
{
    private readonly IIncidentRepository _incidentRepository;

    public IncidentService(IIncidentRepository incidentRepository)
    {
        _incidentRepository = incidentRepository;
    }

    /// <summary>
    /// Returns an aggregated incident summary for the given date range,
    /// with total count and breakdown by severity level.
    /// </summary>
    public async Task<IncidentSummary> GetIncidentSummaryAsync(
        Guid? siteId, Guid? assetId, DateOnly fromDate, DateOnly toDate, CancellationToken ct)
    {
        return await _incidentRepository.GetIncidentSummaryAsync(siteId, assetId, fromDate, toDate, ct);
    }

    /// <summary>
    /// Returns a near-miss summary for the given date range,
    /// including trend comparison against the prior period.
    /// </summary>
    public async Task<NearMissSummary> GetNearMissSummaryAsync(
        Guid? siteId, Guid? assetId, DateOnly fromDate, DateOnly toDate, CancellationToken ct)
    {
        return await _incidentRepository.GetNearMissSummaryAsync(siteId, assetId, fromDate, toDate, ct);
    }
}
