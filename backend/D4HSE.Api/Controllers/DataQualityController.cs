using D4HSE.Api.Models;
using D4HSE.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace D4HSE.Api.Controllers;

/// <summary>
/// API controller for data quality operations.
/// Provides summary of valid/flagged/conflict records by category.
/// </summary>
[ApiController]
[Route("api/v1/data-quality")]
public class DataQualityController : ControllerBase
{
    private readonly DataQualityService _dataQualityService;

    public DataQualityController(DataQualityService dataQualityService)
    {
        _dataQualityService = dataQualityService;
    }

    /// <summary>
    /// Returns counts of VALID, FLAGGED, and CONFLICT records broken down by source category,
    /// along with the last-ingestion timestamp and freshness status.
    /// </summary>
    /// <param name="siteId">Optional site filter.</param>
    /// <param name="fromDate">Optional start date (ISO 8601). Defaults to 30 days ago.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(DataQualitySummaryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<DataQualitySummaryResponse>> GetSummary(
        [FromQuery] string? siteId,
        [FromQuery] DateOnly? fromDate,
        CancellationToken ct)
    {
        var result = await _dataQualityService.GetSummaryAsync(siteId, fromDate, ct);

        var response = new DataQualitySummaryResponse
        {
            Categories = result.Categories.Select(c => new CategoryQualitySummary
            {
                SourceCategory = c.SourceCategory,
                ValidCount = c.ValidCount,
                FlaggedCount = c.FlaggedCount,
                ConflictCount = c.ConflictCount,
                TotalCount = c.TotalCount
            }).ToList(),
            LastIngestionTimestamp = result.LastIngestionTimestamp,
            FreshnessStatus = result.FreshnessStatus
        };

        return Ok(response);
    }
}
