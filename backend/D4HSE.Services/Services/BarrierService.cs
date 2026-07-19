using D4HSE.Core.Interfaces;

namespace D4HSE.Services.Services;

/// <summary>
/// Business logic layer for barrier operations.
/// Orchestrates repository calls and applies additional business rules.
/// </summary>
public class BarrierService
{
    private readonly IBarrierRepository _barrierRepository;

    public BarrierService(IBarrierRepository barrierRepository)
    {
        _barrierRepository = barrierRepository;
    }

    /// <summary>
    /// Retrieves barriers for a given context (site/asset), sorted by criticality rank.
    /// Returns barriers with current RAG status derived from latest observations.
    /// </summary>
    public async Task<IReadOnlyList<BarrierWithStatus>> GetBarriersByContextAsync(
        Guid? siteId, Guid? assetId, CancellationToken ct)
    {
        return await _barrierRepository.GetBarriersByContextAsync(siteId, assetId, ct);
    }

    /// <summary>
    /// Returns ordered health observations for a given barrier within the specified period,
    /// used for sparkline trend display.
    /// </summary>
    public async Task<IReadOnlyList<BarrierTrendPoint>> GetBarrierTrendAsync(
        Guid barrierId, int periodDays, CancellationToken ct)
    {
        return await _barrierRepository.GetBarrierTrendAsync(barrierId, periodDays, ct);
    }

    /// <summary>
    /// Returns barriers where the latest observation RAG status is worse than the prior observation.
    /// Optionally filtered by site.
    /// </summary>
    public async Task<IReadOnlyList<BarrierWithStatus>> GetDegradedBarriersAsync(
        Guid? siteId, CancellationToken ct)
    {
        return await _barrierRepository.GetDegradedBarriersAsync(siteId, ct);
    }
}
