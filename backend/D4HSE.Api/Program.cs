using System.Security.Claims;
using Amazon.BedrockRuntime;
using Amazon.DynamoDBv2;
using Amazon.SecretsManager;
using Amazon.SimpleSystemsManagement;
using D4HSE.Api.Authorization;
using D4HSE.Core.Interfaces;
using D4HSE.Infrastructure.AwsServices;
using D4HSE.Infrastructure.Data;
using D4HSE.Infrastructure.Seed;
using BarrierRepo = D4HSE.Infrastructure.Repositories.BarrierRepository;
using IncidentRepo = D4HSE.Infrastructure.Repositories.IncidentRepository;
using D4HSE.Services.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure JWT Bearer Authentication (Cognito)
var cognitoAuthority = builder.Configuration["Cognito:Authority"];
var cognitoClientId = builder.Configuration["Cognito:ClientId"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = cognitoAuthority;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = cognitoAuthority,
            ValidateAudience = true,
            ValidAudience = cognitoClientId,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };

        // Map Cognito custom:role claim to a standard claim for policy evaluation
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var identity = context.Principal?.Identity as ClaimsIdentity;
                var customRole = identity?.FindFirst("custom:role")?.Value;

                if (!string.IsNullOrEmpty(customRole) && identity is not null)
                {
                    identity.AddClaim(new Claim(HseRoleAuthorizationHandler.HseRoleClaimType, customRole));
                }

                return Task.CompletedTask;
            }
        };
    });

// Configure Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(PolicyNames.HseManager, policy =>
        policy.Requirements.Add(new HseRoleRequirement(HseRoles.HseManager, HseRoles.Admin)));

    options.AddPolicy(PolicyNames.DataOwner, policy =>
        policy.Requirements.Add(new HseRoleRequirement(HseRoles.DataOwner, HseRoles.Admin)));

    options.AddPolicy(PolicyNames.Executive, policy =>
        policy.Requirements.Add(new HseRoleRequirement(HseRoles.Executive, HseRoles.Admin)));

    options.AddPolicy(PolicyNames.Admin, policy =>
        policy.Requirements.Add(new HseRoleRequirement(HseRoles.Admin)));
});

builder.Services.AddSingleton<IAuthorizationHandler, HseRoleAuthorizationHandler>();

// Register AWS SDK clients
builder.Services.AddSingleton<IAmazonDynamoDB, AmazonDynamoDBClient>();
builder.Services.AddSingleton<IAmazonSimpleSystemsManagement, AmazonSimpleSystemsManagementClient>();

// Register repository and services
builder.Services.AddSingleton<IDataQualityLogRepository, D4HSE.Infrastructure.AwsServices.DataQualityLogRepository>();
builder.Services.AddSingleton<IParameterStoreService, ParameterStoreService>();
builder.Services.AddScoped<DataQualityService>();
builder.Services.AddScoped<IBarrierRepository, BarrierRepo>();
builder.Services.AddScoped<BarrierService>();
builder.Services.AddScoped<IIncidentRepository, IncidentRepo>();
builder.Services.AddScoped<IncidentService>();
builder.Services.AddScoped<IRiskItemRepository, D4HSE.Infrastructure.Repositories.RiskItemRepository>();
builder.Services.AddScoped<ISiteRepository, D4HSE.Infrastructure.Repositories.SiteRepository>();
builder.Services.AddScoped<IComplianceRepository, D4HSE.Infrastructure.Repositories.ComplianceRepository>();
builder.Services.AddScoped<RiskScoreService>();
builder.Services.AddScoped<ExecutiveService>();

// Register AI Copilot services
builder.Services.AddSingleton<IAmazonBedrockRuntime, AmazonBedrockRuntimeClient>();
builder.Services.AddSingleton<IEmbeddingService, BedrockEmbeddingService>();
builder.Services.AddSingleton<IBedrockChatService, BedrockChatService>();

// OpenSearch vector store
var openSearchEndpoint = builder.Configuration["OpenSearch:Endpoint"] ?? "https://localhost:9200";
builder.Services.AddSingleton<OpenSearch.Client.IOpenSearchClient>(sp =>
{
    var settings = new OpenSearch.Client.ConnectionSettings(new Uri(openSearchEndpoint))
        .DefaultIndex(OpenSearchConfig.IndexName);
    return new OpenSearch.Client.OpenSearchClient(settings);
});
builder.Services.AddSingleton<IVectorStore, OpenSearchVectorStore>();

builder.Services.AddScoped<SemanticSearchService>();
builder.Services.AddSingleton<RecommendationRulesService>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<RecommendationRulesService>>();
    var rulesPath = Path.Combine(AppContext.BaseDirectory, "RecommendationRules.json");
    return new RecommendationRulesService(logger, rulesPath);
});
builder.Services.AddScoped<CopilotService>();

// Register AWS Secrets Manager service for non-Development environments
if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<IAmazonSecretsManager, AmazonSecretsManagerClient>();
    builder.Services.AddSingleton<ISecretsService, SecretsManagerService>();
}

// Register DbContext with Npgsql PostgreSQL provider
if (builder.Environment.IsDevelopment())
{
    // In Development, use the connection string from appsettings
    var connectionString = builder.Configuration.GetConnectionString("HseCockpit");
    builder.Services.AddDbContext<HseCockpitDbContext>(options =>
    {
        options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsAssembly("D4HSE.Infrastructure");
        });
        options.UseSnakeCaseNamingConvention();
    });
}
else
{
    // In non-Development environments, retrieve connection string from AWS Secrets Manager
    builder.Services.AddDbContext<HseCockpitDbContext>((serviceProvider, options) =>
    {
        var secretsService = serviceProvider.GetRequiredService<ISecretsService>();
        var connectionString = secretsService
            .GetDatabaseConnectionStringAsync(CancellationToken.None)
            .GetAwaiter()
            .GetResult();

        options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsAssembly("D4HSE.Infrastructure");
        });
        options.UseSnakeCaseNamingConvention();
    });
}

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "https://d146pdy472ewmz.cloudfront.net")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Seed reference data in development
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<HseCockpitDbContext>();
    var seeder = new DatabaseSeeder();
    await seeder.SeedAsync(dbContext);
}

app.UseHttpsRedirection();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.MapControllers();

await app.RunAsync();
