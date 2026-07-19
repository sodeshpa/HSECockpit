using D4HSE.Api.Authorization;
using D4HSE.Api.Models;
using D4HSE.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace D4HSE.Api.Controllers;

/// <summary>
/// API controller for executive cockpit operations.
/// Provides portfolio-level KPIs for executive stakeholders.
/// </summary>
[ApiController]
[Route("api/v1/executive")]
[Authorize]
public class ExecutiveController : ControllerBase
{
    private readonly ExecutiveService _executiveService;

    public ExecutiveController(ExecutiveService executiveService)
    {
        _executiveService = executiveService;
    }

    /// <summary>
    /// Returns portfolio-level executive KPIs including Barrier Health Score,
    /// Open Critical Risks count, Incident Count MTD, and Compliance Rate.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet("kpis")]
    [ProducesResponseType(typeof(ExecutiveKPIsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ExecutiveKPIsResponse>> GetExecutiveKPIs(CancellationToken ct)
    {
        var kpis = await _executiveService.GetExecutiveKPIsAsync(ct);

        var response = new ExecutiveKPIsResponse
        {
            BarrierHealthScore = kpis.BarrierHealthScore,
            OpenCriticalRisks = kpis.OpenCriticalRisks,
            IncidentCountMTD = kpis.IncidentCountMTD,
            ComplianceRate = kpis.ComplianceRate,
            DataQualityStatus = kpis.DataQualityStatus
        };

        return Ok(response);
    }
}
