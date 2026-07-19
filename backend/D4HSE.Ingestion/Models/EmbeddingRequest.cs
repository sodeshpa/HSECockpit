namespace D4HSE.Ingestion.Models;

/// <summary>
/// Payload sent to the embedding generation Lambda function after successful data ingestion.
/// Contains the IDs of newly ingested records that need vector embeddings generated.
/// </summary>
public class EmbeddingRequest
{
    /// <summary>
    /// The source category of the ingested data (e.g., "barrier_inspections", "incidents", "maintenance").
    /// </summary>
    public string SourceCategory { get; set; } = string.Empty;

    /// <summary>
    /// The IDs of the newly ingested records that require embedding generation.
    /// </summary>
    public List<string> RecordIds { get; set; } = [];

    /// <summary>
    /// The date of ingestion in ISO 8601 format (yyyy-MM-dd).
    /// </summary>
    public string IngestionDate { get; set; } = string.Empty;
}
