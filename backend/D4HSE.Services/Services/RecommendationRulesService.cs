using System.Text.Json;
using D4HSE.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace D4HSE.Services.Services;

/// <summary>
/// Loads predefined recommendation rules from a JSON configuration file (S3 in production, local in dev).
/// Caches rules in memory and provides rule matching against retrieved HSE context.
/// Rules are reloadable without code deploy.
/// </summary>
public class RecommendationRulesService
{
    private readonly ILogger<RecommendationRulesService> _logger;
    private readonly string _rulesFilePath;
    private IReadOnlyList<RecommendationRule> _cachedRules = [];
    private DateTime _lastLoadedAt = DateTime.MinValue;
    private readonly SemaphoreSlim _loadLock = new(1, 1);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public RecommendationRulesService(
        ILogger<RecommendationRulesService> logger,
        string rulesFilePath)
    {
        _logger = logger;
        _rulesFilePath = rulesFilePath;
    }

    /// <summary>
    /// Loads rules from the configured JSON file. Results are cached in memory.
    /// Call ReloadRulesAsync to force a refresh without code deploy.
    /// </summary>
    public async Task<IReadOnlyList<RecommendationRule>> GetRulesAsync(CancellationToken ct)
    {
        if (_cachedRules.Count > 0)
            return _cachedRules;

        await LoadRulesAsync(ct);
        return _cachedRules;
    }

    /// <summary>
    /// Forces a reload of the rules from the configuration file.
    /// Enables rules to be updated in S3 without requiring a code deploy.
    /// </summary>
    public async Task ReloadRulesAsync(CancellationToken ct)
    {
        await LoadRulesAsync(ct);
    }

    /// <summary>
    /// Given retrieved HSE context (vector search results), identifies applicable recommendation rules
    /// and returns them as RecommendedAction instances.
    /// </summary>
    /// <param name="context">The HSE records retrieved by semantic search.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An array of matched recommended actions with rule labels, action text, and triggering conditions.</returns>
    public async Task<IReadOnlyList<RecommendedAction>> MatchRulesAsync(
        IReadOnlyList<VectorSearchResult> context, CancellationToken ct)
    {
        var rules = await GetRulesAsync(ct);

        if (rules.Count == 0 || context.Count == 0)
            return [];

        var matchedActions = new List<RecommendedAction>();
        var matchedRuleIds = new HashSet<string>();

        foreach (var rule in rules)
        {
            foreach (var record in context)
            {
                if (matchedRuleIds.Contains(rule.RuleId))
                    break;

                if (IsRuleApplicable(rule, record))
                {
                    matchedActions.Add(new RecommendedAction
                    {
                        RuleId = rule.RuleId,
                        Label = rule.Label,
                        Action = rule.Action,
                        TriggeringCondition = $"{rule.Condition} detected in {record.RecordType} record (Record: {record.RecordId})"
                    });
                    matchedRuleIds.Add(rule.RuleId);
                }
            }
        }

        _logger.LogInformation(
            "Rule matching completed: {MatchedCount} rules matched against {ContextCount} context records",
            matchedActions.Count, context.Count);

        return matchedActions;
    }

    private static bool IsRuleApplicable(RecommendationRule rule, VectorSearchResult record)
    {
        // Check if the rule applies to this record type
        if (rule.ApplicableRecordTypes.Count > 0 &&
            !rule.ApplicableRecordTypes.Any(rt =>
                string.Equals(rt, record.RecordType, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        // Check condition-based matching against record content
        var contentLower = record.TextContent.ToLowerInvariant();
        var conditionLower = rule.Condition.ToLowerInvariant();

        // Direct condition match in content
        if (contentLower.Contains(conditionLower))
            return true;

        // Keyword-based matching: all keywords must be present
        if (rule.Keywords.Count > 0)
        {
            var allKeywordsMatch = rule.Keywords.All(kw =>
                contentLower.Contains(kw.ToLowerInvariant()));

            if (allKeywordsMatch)
                return true;
        }

        return false;
    }

    private async Task LoadRulesAsync(CancellationToken ct)
    {
        await _loadLock.WaitAsync(ct);
        try
        {
            if (!File.Exists(_rulesFilePath))
            {
                _logger.LogWarning(
                    "Recommendation rules file not found at {Path}. No rules will be applied.",
                    _rulesFilePath);
                _cachedRules = [];
                return;
            }

            var json = await File.ReadAllTextAsync(_rulesFilePath, ct);
            var rules = JsonSerializer.Deserialize<List<RecommendationRule>>(json, JsonOptions);

            _cachedRules = rules ?? [];
            _lastLoadedAt = DateTime.UtcNow;

            _logger.LogInformation(
                "Loaded {RuleCount} recommendation rules from {Path} at {LoadedAt}",
                _cachedRules.Count, _rulesFilePath, _lastLoadedAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to load recommendation rules from {Path}. Retaining previous rules ({Count} rules).",
                _rulesFilePath, _cachedRules.Count);
        }
        finally
        {
            _loadLock.Release();
        }
    }
}
