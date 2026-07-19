using D4HSE.Api.Models;
using D4HSE.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace D4HSE.Api.Controllers;

/// <summary>
/// API controller for critical barrier operations.
/// Provides barrier listing, trend data for sparklines, and degraded barrier detection.
/// </summary>
[ApiController]
[Route("api/v1/barriers")]
[Authorize]
public class BarriersController : ControllerBase
{
    private readonly BarrierService _barrierService;

    public BarriersController(BarrierService barrierService)
    {
        _barrierService = barrierService;
    }

    /// <summary>
    /// Returns barriers filtered by optional site and asset, sorted by criticality rank.
    /// Each barrier includes current RAG status derived from its latest observation.
    /// </summary>
    /// <param name="siteId">Optional site filter.</param>
    /// <param name="assetId">Optional asset filter.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BarrierResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BarrierResponse>>> GetBarriers(
        [FromQuery] Guid? siteId,
        [FromQuery] Guid? assetId,
        CancellationToken ct)
    {
        var barriers = await _barrierService.GetBarriersByContextAsync(siteId, assetId, ct);

        var response = barriers.Select(b => new BarrierResponse
        {
            BarrierId = b.BarrierId,
            BarrierName = b.BarrierName,
            BarrierType = b.BarrierType,
            SiteId = b.SiteId,
            AssetId = b.AssetId,
            CriticalityRank = b.CriticalityRank,
            CurrentRagStatus = b.CurrentRagStatus,
            LastAssessedDate = b.LastAssessedDate,
            DataQualityStatus = b.DataQualityStatus
        });

        return Ok(response);
    }

    /// <summary>
    /// Returns ordered health observations for a given barrier within the specified period,
    /// used for sparkline trend visualisation.
    /// </summary>
    /// <param name="id">The barrier identifier.</param>
    /// <param name="periodDays">Number of days to look back. Defaults to 90.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet("{id}/trend")]
    [ProducesResponseType(typeof(IEnumerable<BarrierTrendResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BarrierTrendResponse>>> GetBarrierTrend(
        Guid id,
        [FromQuery] int periodDays = 90,
        CancellationToken ct = default)
    {
        var trend = await _barrierService.GetBarrierTrendAsync(id, periodDays, ct);

        var response = trend.Select(t => new BarrierTrendResponse
        {
            ObservedAt = t.ObservedAt,
            RagStatus = t.RagStatus,
            ConditionScore = t.ConditionScore,
            DataQualityStatus = t.DataQualityStatus
        });

        return Ok(response);
    }

    /// <summary>
    /// Returns barriers where the latest observation RAG status is worse (higher severity)
    /// than the prior observation. Optionally filtered by site.
    /// </summary>
    /// <param name="siteId">Optional site filter.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet("degraded")]
    [ProducesResponseType(typeof(IEnumerable<BarrierResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BarrierResponse>>> GetDegradedBarriers(
        [FromQuery] Guid? siteId,
        CancellationToken ct = default)
    {
        var barriers = await _barrierService.GetDegradedBarriersAsync(siteId, ct);

        var response = barriers.Select(b => new BarrierResponse
        {
            BarrierId = b.BarrierId,
            BarrierName = b.BarrierName,
            BarrierType = b.BarrierType,
            SiteId = b.SiteId,
            AssetId = b.AssetId,
            CriticalityRank = b.CriticalityRank,
            CurrentRagStatus = b.CurrentRagStatus,
            LastAssessedDate = b.LastAssessedDate,
            DataQualityStatus = b.DataQualityStatus
        });

        return Ok(response);
    }
}
