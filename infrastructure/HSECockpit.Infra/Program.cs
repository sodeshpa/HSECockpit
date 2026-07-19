using Amazon.CDK;
using HSECockpit.Infra.Stacks;

var app = new App();

// Read environment context from cdk.json
var environmentName = app.Node.TryGetContext("env")?.ToString() ?? "dev";
var environments = app.Node.TryGetContext("environments") as IDictionary<string, object>;
var tags = app.Node.TryGetContext("tags") as IDictionary<string, object>;

// Resolve the target environment configuration
var envConfig = environments?[environmentName] as IDictionary<string, object>;
var account = envConfig?["account"]?.ToString();
var region = envConfig?["region"]?.ToString();

var cdkEnvironment = new Amazon.CDK.Environment
{
    Account = account,
    Region = region
};

// Apply global tags to all resources
if (tags != null)
{
    foreach (var tag in tags)
    {
        Tags.Of(app).Add(tag.Key, tag.Value?.ToString() ?? string.Empty);
    }
}

// Apply environment-specific tag
Tags.Of(app).Add("Environment", environmentName);

// Define stacks — one per concern
// Stacks will be implemented in subsequent tasks
var networkStack = new NetworkStack(app, $"HSECockpit-{environmentName}-NetworkStack", new StackProps
{
    Env = cdkEnvironment,
    Description = "HSECockpit Network infrastructure: VPC, subnets, security groups"
});

var databaseStack = new DatabaseStack(app, $"HSECockpit-{environmentName}-DatabaseStack", new DatabaseStackProps
{
    Env = cdkEnvironment,
    Description = "HSECockpit Data layer: RDS PostgreSQL, DynamoDB",
    Vpc = networkStack.Vpc,
    RdsSecurityGroup = networkStack.RdsSecurityGroup,
    EnvironmentName = environmentName
});

var computeStack = new ComputeStack(app, $"HSECockpit-{environmentName}-ComputeStack", new ComputeStackProps
{
    Env = cdkEnvironment,
    Description = "HSECockpit Compute: ECS Fargate, Lambda functions",
    Vpc = networkStack.Vpc,
    EcsSecurityGroup = networkStack.EcsSecurityGroup,
    LambdaSecurityGroup = networkStack.LambdaSecurityGroup,
    CognitoUserPoolId = "us-east-1_DqjAmxc0t",
    CognitoClientId = "1n1j1g2q6rh4pacsjjpgssr4e9",
    RdsEndpoint = "hsecockpit-dev-databasest-hsecockpitpostgresqlfbfe-wl3pl0hpvaby.ckvsas8s0qv4.us-east-1.rds.amazonaws.com",
    DatabaseSecretArn = databaseStack.PostgresInstance.Secret!.SecretArn
});

var frontendStack = new FrontendStack(app, $"HSECockpit-{environmentName}-FrontendStack", new StackProps
{
    Env = cdkEnvironment,
    Description = "HSECockpit Frontend: S3 static hosting, CloudFront CDN"
});

var apiGatewayStack = new ApiGatewayStack(app, $"HSECockpit-{environmentName}-ApiGatewayStack", new ApiGatewayStackProps
{
    Env = cdkEnvironment,
    Description = "HSECockpit API Gateway: HTTP API, Cognito authorizer",
    Vpc = networkStack.Vpc,
    EcsSecurityGroup = networkStack.EcsSecurityGroup,
    FrontendDomainName = frontendStack.CloudFrontDistribution.DistributionDomainName,
    AlbListenerArn = computeStack.AlbListener.ListenerArn
});

var observabilityStack = new ObservabilityStack(app, $"HSECockpit-{environmentName}-ObservabilityStack", new StackProps
{
    Env = cdkEnvironment,
    Description = "HSECockpit Observability: CloudWatch, X-Ray, alarms"
});

// CI/CD Pipeline — source stage connected to GitHub via CodeStar connection
var pipelineConnectionArn = app.Node.TryGetContext("pipelineConnectionArn")?.ToString()
    ?? "arn:aws:codestar-connections:us-east-1:123456789012:connection/placeholder-connection-id";
var pipelineRepoOwner = app.Node.TryGetContext("pipelineRepoOwner")?.ToString() ?? "HSECockpit";
var pipelineRepoName = app.Node.TryGetContext("pipelineRepoName")?.ToString() ?? "HSECockpit";
var pipelineBranch = app.Node.TryGetContext("pipelineBranch")?.ToString() ?? "main";

var pipelineStack = new PipelineStack(app, $"HSECockpit-{environmentName}-PipelineStack", new PipelineStackProps
{
    Env = cdkEnvironment,
    Description = "HSECockpit CI/CD Pipeline: CodePipeline with GitHub source, backend build + Docker → ECR, frontend build → S3/CloudFront",
    ConnectionArn = pipelineConnectionArn,
    RepoOwner = pipelineRepoOwner,
    RepoName = pipelineRepoName,
    Branch = pipelineBranch,
    EcrRepository = computeStack.EcrRepository,
    FrontendBucketName = frontendStack.WebsiteBucket.BucketName,
    FrontendBucketArn = frontendStack.WebsiteBucket.BucketArn,
    CloudFrontDistributionId = frontendStack.CloudFrontDistribution.DistributionId,
    CloudFrontDistributionArn = $"arn:aws:cloudfront::{account}:distribution/{frontendStack.CloudFrontDistribution.DistributionId}",
    EcsClusterName = computeStack.EcsCluster.ClusterName,
    EcsServiceName = computeStack.ApiService.ServiceName,
    EcsService = computeStack.ApiService
});

app.Synth();
