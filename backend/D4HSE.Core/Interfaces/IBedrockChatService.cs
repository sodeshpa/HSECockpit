namespace D4HSE.Core.Interfaces;

/// <summary>
/// Abstraction for invoking an LLM (Amazon Bedrock Claude) to generate natural language responses
/// grounded in retrieved HSE records. The implementation handles prompt construction with
/// system constraints and context formatting.
/// </summary>
public interface IBedrockChatService
{
    /// <summary>
    /// Generates a natural language response using the LLM, grounded in the provided HSE context.
    /// The system prompt constrains the model to only reference provided records
    /// and to never invent recommended actions.
    /// </summary>
    /// <param name="userQuery">The sanitised user query.</param>
    /// <param name="context">Retrieved HSE records providing factual grounding.</param>
    /// <param name="applicableActions">Predefined rule-based actions that apply to the context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The LLM-generated answer text.</returns>
    Task<string> GenerateResponseAsync(
        string userQuery,
        IReadOnlyList<VectorSearchResult> context,
        IReadOnlyList<RecommendedAction> applicableActions,
        CancellationToken ct);
}
