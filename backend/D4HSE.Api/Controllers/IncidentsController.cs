using D4HSE.Api.Models;
using D4HSE.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace D4HSE.Api.Controllers;

/// <summary>
/// API controller for incident operations.
/// Provides incident summary data with severity breakdown for dashboard views.
/// </summary>
[ApiController]
[Route("api/v1/incidents")]
[Authorize]
public class IncidentsController : ControllerBase
{
    private readonly IncidentService _incidentService;

    public IncidentsController(IncidentService incidentService)
    {
        _incidentService = incidentService;
    }

    /// <summary>
    /// Returns an aggregated incident summary for the given date range,
    /// with total count and breakdown by severity level.
    /// </summary>
    /// <param name="siteId">Optional site filter.</param>
    /// <param name="assetId">Optional asset filter.</param>
    /// <param name="fromDate">Start of the reporting period (inclusive).</param>
    /// <param name="toDate">End of the reporting period (inclusive).</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(IncidentSummaryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<IncidentSummaryResponse>> GetIncidentSummary(
        [FromQuery] Guid? siteId,
        [FromQuery] Guid? assetId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        CancellationToken ct)
    {
        var effectiveFrom = fromDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        var effectiveTo = toDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var summary = await _incidentService.GetIncidentSummaryAsync(siteId, assetId, effectiveFrom, effectiveTo, ct);

        var response = new IncidentSummaryResponse
        {
            TotalCount = summary.TotalCount,
            LowCount = summary.LowCount,
            MediumCount = summary.MediumCount,
            HighCount = summary.HighCount,
            CriticalCount = summary.CriticalCount,
            PeriodStart = summary.PeriodStart,
            PeriodEnd = summary.PeriodEnd
        };

        return Ok(response);
    }
}
