using D4HSE.Api.Models;
using D4HSE.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace D4HSE.Api.Controllers;

/// <summary>
/// API controller for the AI Copilot natural language query interface.
/// Provides HSE managers with the ability to ask questions about barriers, incidents,
/// near misses, and maintenance records using natural language.
/// </summary>
[ApiController]
[Route("api/v1/copilot")]
[Authorize]
public class CopilotController : ControllerBase
{
    private readonly CopilotService _copilotService;
    private readonly ILogger<CopilotController> _logger;

    public CopilotController(
        CopilotService copilotService,
        ILogger<CopilotController> logger)
    {
        _copilotService = copilotService;
        _logger = logger;
    }

    /// <summary>
    /// Processes a natural language HSE query through the AI Copilot pipeline.
    /// The pipeline: sanitize → embed → semantic search → match rules → invoke Bedrock Claude → compose response.
    /// </summary>
    /// <param name="request">The query request containing the natural language question.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A copilot response with answer, citations, recommended actions, and data scope.</returns>
    [HttpPost("query")]
    [ProducesResponseType(typeof(CopilotQueryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CopilotQueryResponse>> Query(
        [FromBody] CopilotQueryRequest request,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _logger.LogInformation(
            "Copilot query received: length={QueryLength}",
            request.Query.Length);

        var result = await _copilotService.QueryAsync(request.Query, ct);

        var response = new CopilotQueryResponse
        {
            Answer = result.Answer,
            DataScope = result.DataScope,
            Citations = result.Citations.Select(c => new SourceCitationResponse
            {
                RecordId = c.RecordId,
                RecordType = c.RecordType,
                SiteId = c.SiteId,
                AssetId = c.AssetId,
                TextContent = c.TextContent,
                RelevanceScore = c.RelevanceScore
            }).ToList(),
            RecommendedActions = result.RecommendedActions.Select(a => new RecommendedActionResponse
            {
                RuleId = a.RuleId,
                Label = a.Label,
                Action = a.Action,
                TriggeringCondition = a.TriggeringCondition
            }).ToList()
        };

        return Ok(response);
    }
}
