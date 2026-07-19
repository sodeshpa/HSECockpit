namespace D4HSE.Core.Interfaces;

/// <summary>
/// Repository interface for site read operations against the relational store.
/// </summary>
public interface ISiteRepository
{
    /// <summary>
    /// Returns all sites (id and name only) for use in aggregation queries.
    /// </summary>
    Task<IReadOnlyList<SiteSummary>> GetAllSitesAsync(CancellationToken ct);
}

/// <summary>
/// Lightweight site projection for listing and aggregation purposes.
/// </summary>
public class SiteSummary
{
    public Guid SiteId { get; set; }
    public string SiteName { get; set; } = string.Empty;
}
