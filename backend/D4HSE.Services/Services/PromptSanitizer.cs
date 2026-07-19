using System.Text.RegularExpressions;

namespace D4HSE.Services.Services;

/// <summary>
/// Utility class for sanitising user input before inclusion in LLM prompts.
/// Prevents prompt injection attacks by stripping or escaping dangerous patterns.
/// </summary>
public static partial class PromptSanitizer
{
    /// <summary>
    /// Maximum allowed input length for copilot queries.
    /// </summary>
    private const int MaxInputLength = 2000;

    /// <summary>
    /// Patterns that may indicate prompt injection attempts.
    /// </summary>
    private static readonly string[] DangerousPatterns =
    [
        "ignore previous",
        "ignore above",
        "ignore all previous",
        "disregard previous",
        "disregard above",
        "forget previous",
        "forget your instructions",
        "new instructions",
        "override instructions",
        "system prompt",
        "you are now",
        "act as",
        "pretend to be",
        "reveal your prompt",
        "show your prompt",
        "repeat your instructions",
        "ignore the system",
        "bypass",
        "jailbreak"
    ];

    /// <summary>
    /// Sanitises user input for safe inclusion in LLM prompts.
    /// Strips dangerous injection patterns, trims excessive whitespace,
    /// and enforces length limits.
    /// </summary>
    /// <param name="input">The raw user input.</param>
    /// <returns>The sanitised input safe for prompt inclusion.</returns>
    public static string Sanitize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Trim and enforce length limit
        var sanitized = input.Trim();
        if (sanitized.Length > MaxInputLength)
            sanitized = sanitized[..MaxInputLength];

        // Remove control characters except newline and tab
        sanitized = ControlCharRegex().Replace(sanitized, string.Empty);

        // Remove markdown-style code fences that could be used to inject system messages
        sanitized = CodeFenceRegex().Replace(sanitized, string.Empty);

        // Strip dangerous prompt injection patterns (case-insensitive)
        foreach (var pattern in DangerousPatterns)
        {
            sanitized = Regex.Replace(
                sanitized,
                Regex.Escape(pattern),
                "[filtered]",
                RegexOptions.IgnoreCase);
        }

        // Collapse multiple whitespace into single space
        sanitized = MultiWhitespaceRegex().Replace(sanitized, " ").Trim();

        return sanitized;
    }

    /// <summary>
    /// Validates whether the input is within acceptable bounds for processing.
    /// </summary>
    /// <param name="input">The raw input to validate.</param>
    /// <returns>True if the input is processable; false if it should be rejected outright.</returns>
    public static bool IsValidInput(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        if (input.Length > MaxInputLength)
            return false;

        return true;
    }

    [GeneratedRegex(@"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]")]
    private static partial Regex ControlCharRegex();

    [GeneratedRegex(@"```[\s\S]*?```")]
    private static partial Regex CodeFenceRegex();

    [GeneratedRegex(@"\s{2,}")]
    private static partial Regex MultiWhitespaceRegex();
}
