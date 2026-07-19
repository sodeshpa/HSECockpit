namespace D4HSE.Core.Interfaces;

/// <summary>
/// Interface for reading configuration parameters from AWS Systems Manager Parameter Store.
/// </summary>
public interface IParameterStoreService
{
    /// <summary>
    /// Gets a parameter value by its key/path.
    /// </summary>
    Task<string?> GetParameterAsync(string parameterKey, CancellationToken ct);
}
