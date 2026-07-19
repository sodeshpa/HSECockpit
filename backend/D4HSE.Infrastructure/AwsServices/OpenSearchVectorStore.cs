using System.Text;
using System.Text.Json;
using D4HSE.Core.Interfaces;
using Microsoft.Extensions.Logging;
using OpenSearch.Client;

namespace D4HSE.Infrastructure.AwsServices;

/// <summary>
/// OpenSearch Serverless implementation of <see cref="IVectorStore"/>.
/// Indexes HSE record embeddings and performs k-NN similarity search.
/// </summary>
public sealed class OpenSearchVectorStore : IVectorStore
{
    private readonly IOpenSearchClient _client;
    private readonly ILogger<OpenSearchVectorStore> _logger;

    public OpenSearchVectorStore(
        IOpenSearchClient client,
        ILogger<OpenSearchVectorStore> logger)
    {
        _client = client;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task IndexDocumentAsync(
        string recordId,
        string recordType,
        Guid siteId,
        Guid? assetId,
        string textContent,
        float[] embedding,
        CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recordId);
        ArgumentException.ThrowIfNullOrWhiteSpace(recordType);
        ArgumentException.ThrowIfNullOrWhiteSpace(textContent);
        ArgumentNullException.ThrowIfNull(embedding);

        var document = new HseEmbeddingDocument
        {
            RecordId = recordId,
            RecordType = recordType,
            SiteId = siteId.ToString(),
            AssetId = assetId?.ToString(),
            TextContent = textContent,
            Embedding = embedding
        };

        var response = await _client.IndexAsync(
            document,
            idx => idx
                .Index(OpenSearchConfig.IndexName)
                .Id(recordId),
            ct);

        if (!response.IsValid)
        {
            _logger.LogError(
                "Failed to index document {RecordId} in OpenSearch: {Error}",
                recordId, response.DebugInformation);
            throw new InvalidOperationException(
                $"Failed to index document {recordId}: {response.ServerError?.Error?.Reason}");
        }

        _logger.LogDebug("Indexed document {RecordId} of type {RecordType}", recordId, recordType);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<VectorSearchResult>> SearchAsync(
        float[] queryEmbedding,
        int topK,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(queryEmbedding);
        if (topK <= 0)
            throw new ArgumentOutOfRangeException(nameof(topK), "topK must be greater than zero.");

        var searchResponse = await _client.SearchAsync<HseEmbeddingDocument>(s => s
            .Index(OpenSearchConfig.IndexName)
            .Size(topK)
            .Query(q => q
                .Knn(knn => knn
                    .Field(f => f.Embedding)
                    .Vector(queryEmbedding)
                    .K(topK)
                )
            ),
            ct);

        if (!searchResponse.IsValid)
        {
            _logger.LogError(
                "OpenSearch k-NN search failed: {Error}",
                searchResponse.DebugInformation);
            throw new InvalidOperationException(
                $"Vector search failed: {searchResponse.ServerError?.Error?.Reason}");
        }

        var results = searchResponse.Hits.Select(hit => new VectorSearchResult
        {
            RecordId = hit.Source.RecordId,
            RecordType = hit.Source.RecordType,
            SiteId = Guid.Parse(hit.Source.SiteId),
            AssetId = string.IsNullOrEmpty(hit.Source.AssetId) ? null : Guid.Parse(hit.Source.AssetId),
            TextContent = hit.Source.TextContent,
            Score = (float)(hit.Score ?? 0.0)
        }).ToList();

        _logger.LogDebug("k-NN search returned {Count} results (requested topK={TopK})", results.Count, topK);

        return results;
    }

    /// <summary>
    /// Internal document model matching the OpenSearch index mapping.
    /// </summary>
    private sealed class HseEmbeddingDocument
    {
        [PropertyName("record_id")]
        public string RecordId { get; set; } = string.Empty;

        [PropertyName("record_type")]
        public string RecordType { get; set; } = string.Empty;

        [PropertyName("site_id")]
        public string SiteId { get; set; } = string.Empty;

        [PropertyName("asset_id")]
        public string? AssetId { get; set; }

        [PropertyName("text_content")]
        public string TextContent { get; set; } = string.Empty;

        [PropertyName("embedding")]
        public float[] Embedding { get; set; } = Array.Empty<float>();
    }
}
