namespace D4HSE.Core.Interfaces;

/// <summary>
/// Repository interface for risk item read operations against the relational store.
/// </summary>
public interface IRiskItemRepository
{
    /// <summary>
    /// Returns the count of open risk items for a site, grouped by severity.
    /// Only items with status = OPEN are included.
    /// </summary>
    Task<RiskItemCounts> GetOpenRiskCountsAsync(Guid siteId, CancellationToken ct);
}

/// <summary>
/// Counts of open risk items grouped by severity level for a given site.
/// </summary>
public class RiskItemCounts
{
    public int OpenCriticalCount { get; set; }
    public int OpenHighCount { get; set; }
    public int OpenMediumCount { get; set; }
    public int OpenLowCount { get; set; }
    public int TotalOpenCount { get; set; }
}
