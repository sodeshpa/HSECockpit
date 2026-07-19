using D4HSE.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace D4HSE.Services.Services;

/// <summary>
/// Orchestrates semantic search over HSE records by combining embedding generation (Bedrock)
/// with k-NN vector search (OpenSearch).
/// </summary>
public class SemanticSearchService
{
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorStore _vectorStore;
    private readonly ILogger<SemanticSearchService> _logger;

    public SemanticSearchService(
        IEmbeddingService embeddingService,
        IVectorStore vectorStore,
        ILogger<SemanticSearchService> logger)
    {
        _embeddingService = embeddingService;
        _vectorStore = vectorStore;
        _logger = logger;
    }

    /// <summary>
    /// Performs a semantic search: embeds the natural language query via Bedrock Titan Embeddings v2,
    /// then performs a k-NN search in OpenSearch to retrieve the most relevant HSE records.
    /// </summary>
    /// <param name="query">The natural language query to search for.</param>
    /// <param name="topK">Maximum number of results to return.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A ranked list of HSE record references ordered by relevance.</returns>
    public async Task<IReadOnlyList<VectorSearchResult>> SemanticSearchAsync(
        string query, int topK, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);
        if (topK <= 0)
            throw new ArgumentOutOfRangeException(nameof(topK), "topK must be greater than zero.");

        _logger.LogInformation(
            "Executing semantic search: query length={QueryLength}, topK={TopK}",
            query.Length, topK);

        // Step 1: Embed the query text using Bedrock Titan Embeddings v2
        var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query, ct);

        // Step 2: Perform k-NN search in OpenSearch vector index
        var results = await _vectorStore.SearchAsync(queryEmbedding, topK, ct);

        _logger.LogInformation(
            "Semantic search completed: {ResultCount} results returned",
            results.Count);

        return results;
    }
}
