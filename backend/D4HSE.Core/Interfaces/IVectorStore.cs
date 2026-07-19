namespace D4HSE.Core.Interfaces;

/// <summary>
/// Abstraction for a vector store that supports indexing and k-NN search over HSE record embeddings.
/// </summary>
public interface IVectorStore
{
    /// <summary>
    /// Indexes a document (HSE record) with its embedding into the vector store.
    /// </summary>
    /// <param name="recordId">Unique identifier of the HSE record.</param>
    /// <param name="recordType">Type of record (barrier_observation, incident, near_miss, maintenance).</param>
    /// <param name="siteId">The site this record belongs to.</param>
    /// <param name="assetId">The asset this record is associated with, if any.</param>
    /// <param name="textContent">The textual content to store alongside the embedding.</param>
    /// <param name="embedding">The 1024-dimensional embedding vector.</param>
    /// <param name="ct">Cancellation token.</param>
    Task IndexDocumentAsync(
        string recordId,
        string recordType,
        Guid siteId,
        Guid? assetId,
        string textContent,
        float[] embedding,
        CancellationToken ct);

    /// <summary>
    /// Performs a k-NN similarity search using the provided query embedding.
    /// </summary>
    /// <param name="queryEmbedding">The query embedding vector (1024 dimensions).</param>
    /// <param name="topK">The maximum number of results to return.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A ranked list of search results ordered by descending similarity score.</returns>
    Task<IReadOnlyList<VectorSearchResult>> SearchAsync(float[] queryEmbedding, int topK, CancellationToken ct);
}
