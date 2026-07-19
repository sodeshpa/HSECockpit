using System.Text;
using System.Text.Json;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using D4HSE.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace D4HSE.Infrastructure.AwsServices;

/// <summary>
/// Generates text embeddings using Amazon Bedrock Titan Embeddings v2.
/// Produces 1024-dimensional vectors for use in OpenSearch k-NN vector search.
/// </summary>
public sealed class BedrockEmbeddingService : IEmbeddingService
{
    private const string ModelId = "amazon.titan-embed-text-v2:0";
    private const int ExpectedDimension = 1024;

    private readonly IAmazonBedrockRuntime _bedrockRuntime;
    private readonly ILogger<BedrockEmbeddingService> _logger;

    public BedrockEmbeddingService(
        IAmazonBedrockRuntime bedrockRuntime,
        ILogger<BedrockEmbeddingService> logger)
    {
        _bedrockRuntime = bedrockRuntime;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        var requestPayload = new
        {
            inputText = text,
            dimensions = ExpectedDimension,
            normalize = true
        };

        var jsonPayload = JsonSerializer.Serialize(requestPayload);

        var request = new InvokeModelRequest
        {
            ModelId = ModelId,
            ContentType = "application/json",
            Accept = "application/json",
            Body = new MemoryStream(Encoding.UTF8.GetBytes(jsonPayload))
        };

        _logger.LogDebug("Invoking Bedrock Titan Embeddings v2 for text of length {TextLength}", text.Length);

        var response = await _bedrockRuntime.InvokeModelAsync(request, ct);

        using var responseStream = response.Body;
        var responseDoc = await JsonDocument.ParseAsync(responseStream, cancellationToken: ct);

        var embeddingArray = responseDoc.RootElement.GetProperty("embedding");
        var embedding = new float[ExpectedDimension];

        var index = 0;
        foreach (var element in embeddingArray.EnumerateArray())
        {
            if (index >= ExpectedDimension)
                break;

            embedding[index] = element.GetSingle();
            index++;
        }

        if (index != ExpectedDimension)
        {
            _logger.LogWarning(
                "Embedding dimension mismatch: expected {Expected}, got {Actual}",
                ExpectedDimension, index);
        }

        return embedding;
    }
}
