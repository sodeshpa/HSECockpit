using D4HSE.Api.Models;
using D4HSE.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace D4HSE.Api.Controllers;

/// <summary>
/// API controller for near-miss operations.
/// Provides near-miss summary data with trend comparison for dashboard views.
/// </summary>
[ApiController]
[Route("api/v1/near-misses")]
[Authorize]
public class NearMissesController : ControllerBase
{
    private readonly IncidentService _incidentService;

    public NearMissesController(IncidentService incidentService)
    {
        _incidentService = incidentService;
    }

    /// <summary>
    /// Returns an aggregated near-miss summary for the given date range,
    /// including trend comparison against the prior period.
    /// </summary>
    /// <param name="siteId">Optional site filter.</param>
    /// <param name="assetId">Optional asset filter.</param>
    /// <param name="fromDate">Start of the reporting period (inclusive).</param>
    /// <param name="toDate">End of the reporting period (inclusive).</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(NearMissSummaryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<NearMissSummaryResponse>> GetNearMissSummary(
        [FromQuery] Guid? siteId,
        [FromQuery] Guid? assetId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        CancellationToken ct)
    {
        var effectiveFrom = fromDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        var effectiveTo = toDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var summary = await _incidentService.GetNearMissSummaryAsync(siteId, assetId, effectiveFrom, effectiveTo, ct);

        var response = new NearMissSummaryResponse
        {
            TotalCount = summary.TotalCount,
            PriorPeriodCount = summary.PriorPeriodCount,
            TrendDirection = summary.TrendDirection,
            PeriodStart = summary.PeriodStart,
            PeriodEnd = summary.PeriodEnd
        };

        return Ok(response);
    }
}
