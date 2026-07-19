using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using D4HSE.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace D4HSE.Infrastructure.AwsServices;

/// <summary>
/// AWS Systems Manager Parameter Store implementation.
/// Reads configuration parameters used for RAG thresholds, freshness settings, etc.
/// </summary>
public sealed class ParameterStoreService : IParameterStoreService
{
    private readonly IAmazonSimpleSystemsManagement _ssmClient;
    private readonly ILogger<ParameterStoreService> _logger;

    public ParameterStoreService(
        IAmazonSimpleSystemsManagement ssmClient,
        ILogger<ParameterStoreService> logger)
    {
        _ssmClient = ssmClient;
        _logger = logger;
    }

    public async Task<string?> GetParameterAsync(string parameterKey, CancellationToken ct)
    {
        try
        {
            var response = await _ssmClient.GetParameterAsync(new GetParameterRequest
            {
                Name = parameterKey,
                WithDecryption = true
            }, ct);

            return response.Parameter?.Value;
        }
        catch (ParameterNotFoundException)
        {
            _logger.LogWarning("Parameter not found: {ParameterKey}", parameterKey);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve parameter: {ParameterKey}", parameterKey);
            return null;
        }
    }
}
