namespace D4HSE.Core.Interfaces;

/// <summary>
/// Service interface for retrieving secrets from AWS Secrets Manager.
/// </summary>
public interface ISecretsService
{
    /// <summary>
    /// Retrieves the database connection string from AWS Secrets Manager.
    /// The secret is cached after the first retrieval to avoid repeated calls.
    /// </summary>
    Task<string> GetDatabaseConnectionStringAsync(CancellationToken ct);
}
