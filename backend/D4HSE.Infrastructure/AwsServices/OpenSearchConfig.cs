namespace D4HSE.Infrastructure.AwsServices;

/// <summary>
/// Configuration constants for the OpenSearch Serverless vector index (hse-embeddings collection).
/// </summary>
public static class OpenSearchConfig
{
    /// <summary>
    /// Name of the OpenSearch Serverless collection.
    /// </summary>
    public const string CollectionName = "hse-embeddings";

    /// <summary>
    /// Name of the vector search index within the collection.
    /// </summary>
    public const string IndexName = "hse-embeddings-index";

    /// <summary>
    /// Dimensionality of the embedding vectors (Amazon Bedrock Titan Embeddings v2).
    /// </summary>
    public const int EmbeddingDimension = 1024;

    /// <summary>
    /// JSON index mapping for the hse-embeddings index.
    /// Uses k-NN vector with HNSW algorithm for approximate nearest-neighbour search.
    /// </summary>
    public const string IndexMapping = """
    {
      "settings": {
        "index": {
          "knn": true
        }
      },
      "mappings": {
        "properties": {
          "record_id": { "type": "keyword" },
          "record_type": { "type": "keyword" },
          "site_id": { "type": "keyword" },
          "asset_id": { "type": "keyword" },
          "text_content": { "type": "text" },
          "embedding": {
            "type": "knn_vector",
            "dimension": 1024,
            "method": {
              "name": "hnsw",
              "engine": "nmslib",
              "space_type": "cosinesimil"
            }
          }
        }
      }
    }
    """;
}
