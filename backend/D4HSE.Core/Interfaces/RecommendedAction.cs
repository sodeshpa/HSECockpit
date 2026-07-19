namespace D4HSE.Core.Interfaces;

/// <summary>
/// Represents a predefined rule-based recommended action matched against observed HSE conditions.
/// These actions are never AI-generated — they come from predefined business rules stored in configuration.
/// </summary>
public class RecommendedAction
{
    /// <summary>
    /// Unique identifier of the recommendation rule that matched.
    /// </summary>
    public string RuleId { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable label for the recommendation (e.g., "Immediate Barrier Inspection Required").
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// The suggested action text describing what should be done.
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Description of the condition that triggered this recommendation.
    /// </summary>
    public string TriggeringCondition { get; set; } = string.Empty;
}
