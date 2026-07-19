namespace D4HSE.Core.Interfaces;

/// <summary>
/// Abstraction for generating text embeddings via an AI model.
/// Used by the RAG pipeline to embed both documents (at ingestion) and queries (at search time).
/// </summary>
public interface IEmbeddingService
{
    /// <summary>
    /// Generates a 1024-dimensional embedding vector for the given text input.
    /// </summary>
    /// <param name="text">The text to embed.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A float array of length 1024 representing the embedding.</returns>
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct);
}
