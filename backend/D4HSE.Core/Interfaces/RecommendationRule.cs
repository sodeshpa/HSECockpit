namespace D4HSE.Core.Interfaces;

/// <summary>
/// Represents a predefined recommendation rule loaded from configuration (S3/local JSON).
/// Each rule defines a condition pattern and the action to recommend when matched.
/// </summary>
public class RecommendationRule
{
    /// <summary>
    /// Unique identifier for this rule.
    /// </summary>
    public string RuleId { get; set; } = string.Empty;

    /// <summary>
    /// The condition expression that triggers this rule.
    /// Matched against record types and content (e.g., "barrier_red", "near_miss_increase_50", "maintenance_overdue").
    /// </summary>
    public string Condition { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable label for the recommendation category.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// The recommended action text to present to the user.
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// The record types this rule applies to (e.g., "barrier_observation", "incident", "near_miss").
    /// </summary>
    public IReadOnlyList<string> ApplicableRecordTypes { get; set; } = [];

    /// <summary>
    /// Keywords that must appear in the record text content for this rule to match.
    /// </summary>
    public IReadOnlyList<string> Keywords { get; set; } = [];
}
