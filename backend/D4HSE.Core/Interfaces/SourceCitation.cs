namespace D4HSE.Core.Interfaces;

/// <summary>
/// Represents a citation to a source HSE record used to ground an AI Copilot response.
/// </summary>
public class SourceCitation
{
    /// <summary>
    /// The unique identifier of the cited HSE record.
    /// </summary>
    public string RecordId { get; set; } = string.Empty;

    /// <summary>
    /// The type of record (e.g., "barrier_observation", "incident", "near_miss", "maintenance").
    /// </summary>
    public string RecordType { get; set; } = string.Empty;

    /// <summary>
    /// The site associated with the cited record.
    /// </summary>
    public Guid SiteId { get; set; }

    /// <summary>
    /// The asset associated with the cited record, if applicable.
    /// </summary>
    public Guid? AssetId { get; set; }

    /// <summary>
    /// Summary text content of the cited record.
    /// </summary>
    public string TextContent { get; set; } = string.Empty;

    /// <summary>
    /// Relevance score from the semantic search (0.0–1.0).
    /// </summary>
    public float RelevanceScore { get; set; }
}
