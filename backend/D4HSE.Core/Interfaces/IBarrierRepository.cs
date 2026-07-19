namespace D4HSE.Core.Interfaces;

/// <summary>
/// Repository interface for critical barrier read operations against the relational store.
/// </summary>
public interface IBarrierRepository
{
    /// <summary>
    /// Retrieves barriers filtered by optional site and asset, sorted by criticality rank ascending.
    /// Each barrier includes the current RAG status derived from its latest observation.
    /// </summary>
    Task<IReadOnlyList<BarrierWithStatus>> GetBarriersByContextAsync(Guid? siteId, Guid? assetId, CancellationToken ct);

    /// <summary>
    /// Returns ordered health observations for a given barrier within the specified period,
    /// sorted by observation date ascending (oldest first) for sparkline display.
    /// </summary>
    Task<IReadOnlyList<BarrierTrendPoint>> GetBarrierTrendAsync(Guid barrierId, int periodDays, CancellationToken ct);

    /// <summary>
    /// Returns barriers where the latest observation RAG status is worse (higher severity)
    /// than the second-latest observation. Optionally filtered by site.
    /// Severity order: GREEN &lt; AMBER &lt; RED.
    /// </summary>
    Task<IReadOnlyList<BarrierWithStatus>> GetDegradedBarriersAsync(Guid? siteId, CancellationToken ct);
}
