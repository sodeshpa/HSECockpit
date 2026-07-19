namespace D4HSE.Core.Interfaces;

/// <summary>
/// Represents a single result from a vector similarity search in the OpenSearch index.
/// </summary>
public sealed class VectorSearchResult
{
    /// <summary>
    /// The unique identifier of the matched HSE record.
    /// </summary>
    public required string RecordId { get; init; }

    /// <summary>
    /// The type of record (e.g., "barrier_observation", "incident", "near_miss", "maintenance").
    /// </summary>
    public required string RecordType { get; init; }

    /// <summary>
    /// The site associated with this record.
    /// </summary>
    public required Guid SiteId { get; init; }

    /// <summary>
    /// The asset associated with this record, if applicable.
    /// </summary>
    public Guid? AssetId { get; init; }

    /// <summary>
    /// The textual content that was indexed and matched.
    /// </summary>
    public required string TextContent { get; init; }

    /// <summary>
    /// The similarity score (0.0–1.0) indicating how relevant this result is to the query.
    /// Higher values indicate greater similarity.
    /// </summary>
    public required float Score { get; init; }
}
