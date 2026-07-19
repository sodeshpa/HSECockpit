using System.Text.Json;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using D4HSE.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace D4HSE.Infrastructure.AwsServices;

/// <summary>
/// Retrieves and caches the RDS connection string from AWS Secrets Manager.
/// The secret JSON is expected to contain host, port, username, password, and dbname fields.
/// </summary>
public sealed class SecretsManagerService : ISecretsService
{
    private readonly IAmazonSecretsManager _secretsManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SecretsManagerService> _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private string? _cachedConnectionString;

    public SecretsManagerService(
        IAmazonSecretsManager secretsManager,
        IConfiguration configuration,
        ILogger<SecretsManagerService> logger)
    {
        _secretsManager = secretsManager;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> GetDatabaseConnectionStringAsync(CancellationToken ct)
    {
        if (_cachedConnectionString is not null)
        {
            return _cachedConnectionString;
        }

        await _semaphore.WaitAsync(ct);
        try
        {
            // Double-check after acquiring the lock
            if (_cachedConnectionString is not null)
            {
                return _cachedConnectionString;
            }

            var secretArn = _configuration["Aws:DatabaseSecretArn"]
                ?? _configuration["DATABASE__SECRETARN"]
                ?? throw new InvalidOperationException(
                    "Database secret ARN not configured. Set 'Aws:DatabaseSecretArn' or 'DATABASE__SECRETARN' environment variable.");

            _logger.LogInformation("Retrieving database connection string from Secrets Manager");

            var request = new GetSecretValueRequest
            {
                SecretId = secretArn
            };

            var response = await _secretsManager.GetSecretValueAsync(request, ct);
            var connectionString = BuildConnectionString(response.SecretString);

            _cachedConnectionString = connectionString;

            _logger.LogInformation("Successfully retrieved and cached database connection string");

            return connectionString;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Parses the secret JSON and builds a PostgreSQL connection string.
    /// Expected JSON format from RDS Secrets Manager:
    /// { "host": "...", "port": 5432, "username": "...", "password": "...", "dbname": "..." }
    /// </summary>
    private static string BuildConnectionString(string secretJson)
    {
        var secret = JsonSerializer.Deserialize<RdsSecretPayload>(secretJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new InvalidOperationException("Failed to parse database secret JSON.");

        if (string.IsNullOrWhiteSpace(secret.Host))
            throw new InvalidOperationException("Database secret is missing 'host' field.");

        if (string.IsNullOrWhiteSpace(secret.Username))
            throw new InvalidOperationException("Database secret is missing 'username' field.");

        if (string.IsNullOrWhiteSpace(secret.Password))
            throw new InvalidOperationException("Database secret is missing 'password' field.");

        var dbName = string.IsNullOrWhiteSpace(secret.Dbname) ? "hsecockpit" : secret.Dbname;
        var port = secret.Port > 0 ? secret.Port : 5432;

        return $"Host={secret.Host};Port={port};Database={dbName};Username={secret.Username};Password={secret.Password}";
    }

    /// <summary>
    /// Represents the JSON structure stored in AWS Secrets Manager for RDS credentials.
    /// </summary>
    private sealed class RdsSecretPayload
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Dbname { get; set; } = string.Empty;
    }
}
