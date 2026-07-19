using D4HSE.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace D4HSE.Services.Services;

/// <summary>
/// Orchestrates the AI Copilot query flow:
/// sanitize input → semantic search → check scope → match rules → call Bedrock Claude → compose response.
/// Never allows the LLM to invent actions — only returns predefined rules.
/// </summary>
public class CopilotService
{
    private readonly SemanticSearchService _semanticSearch;
    private readonly RecommendationRulesService _rulesService;
    private readonly IBedrockChatService _bedrockChat;
    private readonly ILogger<CopilotService> _logger;

    /// <summary>
    /// The record types supported in the MVP data scope.
    /// Queries referencing data outside these categories are out of scope.
    /// </summary>
    private static readonly HashSet<string> MvpRecordTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "barrier_observation",
        "incident",
        "near_miss",
        "maintenance"
    };

    /// <summary>
    /// Keywords that indicate a query is referencing data outside MVP scope.
    /// </summary>
    private static readonly string[] OutOfScopeIndicators =
    [
        "financial",
        "budget",
        "salary",
        "personnel",
        "hr ",
        "human resources",
        "procurement",
        "weather forecast",
        "stock price",
        "market",
        "competitor"
    ];

    private const int TopK = 10;

    public CopilotService(
        SemanticSearchService semanticSearch,
        RecommendationRulesService rulesService,
        IBedrockChatService bedrockChat,
        ILogger<CopilotService> logger)
    {
        _semanticSearch = semanticSearch;
        _rulesService = rulesService;
        _bedrockChat = bedrockChat;
        _logger = logger;
    }

    /// <summary>
    /// Processes a natural language HSE query through the full copilot pipeline.
    /// </summary>
    /// <param name="query">The user's raw natural language question.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A copilot query result with answer, citations, recommendations, and data scope.</returns>
    public async Task<CopilotQueryResult> QueryAsync(string query, CancellationToken ct)
    {
        // Step 1: Sanitize user input (prompt injection prevention)
        var sanitizedQuery = PromptSanitizer.Sanitize(query);

        if (string.IsNullOrWhiteSpace(sanitizedQuery))
        {
            return new CopilotQueryResult
            {
                Answer = "Please provide a valid question about HSE barriers, incidents, near misses, or maintenance records.",
                DataScope = "out_of_scope",
                Citations = [],
                RecommendedActions = []
            };
        }

        _logger.LogInformation(
            "Processing copilot query: original length={OriginalLength}, sanitized length={SanitizedLength}",
            query.Length, sanitizedQuery.Length);

        // Step 2: Check if query references data outside MVP categories
        if (IsOutOfScope(sanitizedQuery))
        {
            return new CopilotQueryResult
            {
                Answer = "This question references data outside the current HSE Cockpit scope. " +
                         "The AI Copilot can answer questions about: critical barriers and their health status, " +
                         "incidents and near misses, asset maintenance records, and site risk scores. " +
                         "Please try rephrasing your question to focus on these areas.",
                DataScope = "out_of_scope",
                Citations = [],
                RecommendedActions = []
            };
        }

        // Step 3: Semantic search to retrieve relevant HSE records
        var searchResults = await _semanticSearch.SemanticSearchAsync(sanitizedQuery, TopK, ct);

        if (searchResults.Count == 0)
        {
            return new CopilotQueryResult
            {
                Answer = "No relevant HSE records were found for your question. " +
                         "Please try a different question or check that data has been ingested for the site or area you are asking about.",
                DataScope = "out_of_scope",
                Citations = [],
                RecommendedActions = []
            };
        }

        // Step 4: Determine data scope based on record quality
        var dataScope = DetermineDataScope(searchResults);

        // Step 5: Match recommendation rules against retrieved context
        var recommendedActions = await _rulesService.MatchRulesAsync(searchResults, ct);

        // Step 6: Build citations from search results
        var citations = searchResults.Select(r => new SourceCitation
        {
            RecordId = r.RecordId,
            RecordType = r.RecordType,
            SiteId = r.SiteId,
            AssetId = r.AssetId,
            TextContent = r.TextContent,
            RelevanceScore = r.Score
        }).ToList();

        // Step 7: Call Bedrock Claude with system prompt + context + user query
        var llmAnswer = await _bedrockChat.GenerateResponseAsync(
            sanitizedQuery, searchResults, recommendedActions, ct);

        return new CopilotQueryResult
        {
            Answer = llmAnswer,
            Citations = citations,
            RecommendedActions = recommendedActions,
            DataScope = dataScope
        };
    }

    /// <summary>
    /// Determines whether the query references data outside the MVP scope.
    /// </summary>
    private static bool IsOutOfScope(string query)
    {
        var queryLower = query.ToLowerInvariant();
        return OutOfScopeIndicators.Any(indicator => queryLower.Contains(indicator));
    }

    /// <summary>
    /// Determines the data scope based on retrieved records.
    /// "partial" if any records have quality flags in their content.
    /// "full" when all records appear to be clean.
    /// </summary>
    private static string DetermineDataScope(IReadOnlyList<VectorSearchResult> results)
    {
        // Check if any results contain quality flag indicators
        var hasQualityFlags = results.Any(r =>
            r.TextContent.Contains("FLAGGED", StringComparison.OrdinalIgnoreCase) ||
            r.TextContent.Contains("CONFLICT", StringComparison.OrdinalIgnoreCase) ||
            r.TextContent.Contains("data_quality_status", StringComparison.OrdinalIgnoreCase));

        return hasQualityFlags ? "partial" : "full";
    }
}
