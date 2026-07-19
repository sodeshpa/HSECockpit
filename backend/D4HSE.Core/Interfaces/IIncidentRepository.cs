namespace D4HSE.Core.Interfaces;

/// <summary>
/// Repository interface for incident and near-miss read operations against the relational store.
/// </summary>
public interface IIncidentRepository
{
    /// <summary>
    /// Returns a summary of incidents within the specified date range,
    /// with total count and breakdown by severity (Low, Medium, High, Critical).
    /// Optionally filtered by site and/or asset.
    /// </summary>
    Task<IncidentSummary> GetIncidentSummaryAsync(Guid? siteId, Guid? assetId, DateOnly fromDate, DateOnly toDate, CancellationToken ct);

    /// <summary>
    /// Returns a summary of near misses within the specified date range,
    /// including a trend comparison against the prior period of the same length.
    /// Optionally filtered by site and/or asset.
    /// </summary>
    Task<NearMissSummary> GetNearMissSummaryAsync(Guid? siteId, Guid? assetId, DateOnly fromDate, DateOnly toDate, CancellationToken ct);
}
