using D4HSE.Api.Models;
using D4HSE.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace D4HSE.Api.Controllers;

/// <summary>
/// API controller for risk score operations.
/// Provides composite site risk scores with contributing factor breakdowns.
/// </summary>
[ApiController]
[Route("api/v1/risk")]
[Authorize]
public class RiskController : ControllerBase
{
    private readonly RiskScoreService _riskScoreService;

    public RiskController(RiskScoreService riskScoreService)
    {
        _riskScoreService = riskScoreService;
    }

    /// <summary>
    /// Returns the composite risk score for a given site, including the risk band,
    /// contributing factor breakdown, and data quality status.
    /// </summary>
    /// <param name="siteId">The site identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet("score/{siteId}")]
    [ProducesResponseType(typeof(SiteRiskScoreResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<SiteRiskScoreResponse>> GetSiteRiskScore(
        Guid siteId,
        CancellationToken ct)
    {
        var riskScore = await _riskScoreService.GetSiteRiskScoreAsync(siteId, ct);

        var response = new SiteRiskScoreResponse
        {
            Score = riskScore.Score,
            RiskBand = riskScore.RiskBand,
            DataQualityStatus = riskScore.DataQualityStatus,
            Factors = new RiskFactorsResponse
            {
                IncidentFactor = new IncidentSeverityFactorResponse
                {
                    HighIncidentCount = riskScore.IncidentFactor.HighIncidentCount,
                    CriticalIncidentCount = riskScore.IncidentFactor.CriticalIncidentCount,
                    NormalizedScore = riskScore.IncidentFactor.NormalizedScore,
                    Weight = riskScore.IncidentFactor.Weight,
                    WeightedScore = riskScore.IncidentFactor.WeightedScore
                },
                NearMissFactor = new NearMissFrequencyFactorResponse
                {
                    CurrentPeriodCount = riskScore.NearMissFactor.CurrentPeriodCount,
                    BaselineCount = riskScore.NearMissFactor.BaselineCount,
                    NormalizedScore = riskScore.NearMissFactor.NormalizedScore,
                    Weight = riskScore.NearMissFactor.Weight,
                    WeightedScore = riskScore.NearMissFactor.WeightedScore
                },
                OpenRiskFactor = new OpenRiskFactorResponse
                {
                    OpenCriticalCount = riskScore.OpenRiskFactor.OpenCriticalCount,
                    OpenHighCount = riskScore.OpenRiskFactor.OpenHighCount,
                    NormalizedScore = riskScore.OpenRiskFactor.NormalizedScore,
                    Weight = riskScore.OpenRiskFactor.Weight,
                    WeightedScore = riskScore.OpenRiskFactor.WeightedScore
                },
                BarrierFactor = new BarrierHealthFactorResponse
                {
                    TotalBarriers = riskScore.BarrierFactor.TotalBarriers,
                    NonGreenBarriers = riskScore.BarrierFactor.NonGreenBarriers,
                    NormalizedScore = riskScore.BarrierFactor.NormalizedScore,
                    Weight = riskScore.BarrierFactor.Weight,
                    WeightedScore = riskScore.BarrierFactor.WeightedScore
                }
            }
        };

        return Ok(response);
    }

    /// <summary>
    /// Returns a risk heatmap showing all sites with their composite risk scores and risk bands.
    /// Sites are sorted by score descending (highest risk first).
    /// </summary>
    /// <param name="periodDays">Number of days to consider for risk calculation. Defaults to 30.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet("heatmap")]
    [ProducesResponseType(typeof(IEnumerable<SiteRiskHeatmapResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SiteRiskHeatmapResponse>>> GetRiskHeatmap(
        [FromQuery] int periodDays = 30, CancellationToken ct = default)
    {
        var rows = await _riskScoreService.GetRiskHeatmapAsync(periodDays, ct);

        var response = rows.Select(r => new SiteRiskHeatmapResponse
        {
            SiteId = r.SiteId,
            SiteName = r.SiteName,
            Score = r.Score,
            RiskBand = r.RiskBand
        });

        return Ok(response);
    }
}
