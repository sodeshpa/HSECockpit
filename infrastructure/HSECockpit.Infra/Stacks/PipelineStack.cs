using Amazon.CDK;
using Amazon.CDK.AWS.CodeBuild;
using Amazon.CDK.AWS.CodePipeline;
using Amazon.CDK.AWS.CodePipeline.Actions;
using Amazon.CDK.AWS.CodeStarConnections;
using Amazon.CDK.AWS.ECR;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.SNS;
using Constructs;

namespace HSECockpit.Infra.Stacks;

/// <summary>
/// Properties required by PipelineStack for configuring the CI/CD pipeline.
/// </summary>
public class PipelineStackProps : StackProps
{
    /// <summary>The ARN of the CodeStar connection to GitHub.</summary>
    public required string ConnectionArn { get; init; }

    /// <summary>The GitHub repository owner (user or organization).</summary>
    public required string RepoOwner { get; init; }

    /// <summary>The GitHub repository name.</summary>
    public required string RepoName { get; init; }

    /// <summary>The branch to trigger the pipeline from. Defaults to "main".</summary>
    public string Branch { get; init; } = "main";

    /// <summary>The ECR repository for the backend API container image.</summary>
    public required IRepository EcrRepository { get; init; }

    /// <summary>The S3 bucket name for frontend static assets (from FrontendStack).</summary>
    public required string FrontendBucketName { get; init; }

    /// <summary>The S3 bucket ARN for frontend static assets (from FrontendStack).</summary>
    public required string FrontendBucketArn { get; init; }

    /// <summary>The CloudFront distribution ID for cache invalidation (from FrontendStack).</summary>
    public required string CloudFrontDistributionId { get; init; }

    /// <summary>The CloudFront distribution ARN for IAM policy (from FrontendStack).</summary>
    public required string CloudFrontDistributionArn { get; init; }

    /// <summary>The ECS cluster name for staging deployment (from ComputeStack).</summary>
    public required string EcsClusterName { get; init; }

    /// <summary>The ECS service name for staging deployment (from ComputeStack).</summary>
    public required string EcsServiceName { get; init; }

    /// <summary>The ECS Fargate service for staging deployment (from ComputeStack).</summary>
    public required IBaseService EcsService { get; init; }
}

/// <summary>
/// CI/CD Pipeline: AWS CodePipeline with GitHub source stage via CodeStar connection,
/// CodeBuild project for backend build/test/Docker, and Build stage.
/// </summary>
public class PipelineStack : Stack
{
    /// <summary>The CodePipeline instance.</summary>
    public Pipeline Pipeline { get; }

    /// <summary>The source output artifact from the GitHub source action.</summary>
    public Artifact_ SourceOutput { get; }

    /// <summary>The build output artifact from the backend build action.</summary>
    public Artifact_ BuildOutput { get; }

    /// <summary>The CodeBuild project for the .NET backend.</summary>
    public PipelineProject BackendBuildProject { get; }

    /// <summary>The CodeBuild project for the React frontend.</summary>
    public PipelineProject FrontendBuildProject { get; }

    /// <summary>The CodeBuild project for CDK infrastructure synthesis.</summary>
    public PipelineProject InfraBuildProject { get; }

    public PipelineStack(Construct scope, string id, PipelineStackProps props) : base(scope, id, props)
    {
        // ─── Artifacts ────────────────────────────────────────────────────────────────
        SourceOutput = new Artifact_("SourceOutput");
        BuildOutput = new Artifact_("BuildOutput");

        // ─── Source Action: GitHub via CodeStar Connection ────────────────────────────
        var sourceAction = new CodeStarConnectionsSourceAction(new CodeStarConnectionsSourceActionProps
        {
            ActionName = "GitHub_Source",
            ConnectionArn = props.ConnectionArn,
            Owner = props.RepoOwner,
            Repo = props.RepoName,
            Branch = props.Branch,
            Output = SourceOutput,
            TriggerOnPush = true
        });

        // ─── CodeBuild Project: Backend (.NET 8 build + test + Docker → ECR) ─────────
        BackendBuildProject = new PipelineProject(this, "HSECockpit-Backend-Build", new PipelineProjectProps
        {
            ProjectName = "HSECockpit-Backend-Build",
            Description = "Builds .NET 8 backend, runs tests, builds Docker image and pushes to ECR",
            Environment = new BuildEnvironment
            {
                BuildImage = LinuxBuildImage.STANDARD_7_0,
                ComputeType = ComputeType.SMALL,
                Privileged = true // Required for Docker builds
            },
            EnvironmentVariables = new Dictionary<string, IBuildEnvironmentVariable>
            {
                ["ECR_REPOSITORY_URI"] = new BuildEnvironmentVariable
                {
                    Value = props.EcrRepository.RepositoryUri,
                    Type = BuildEnvironmentVariableType.PLAINTEXT
                },
                ["AWS_ACCOUNT_ID"] = new BuildEnvironmentVariable
                {
                    Value = this.Account,
                    Type = BuildEnvironmentVariableType.PLAINTEXT
                }
            },
            BuildSpec = BuildSpec.FromSourceFilename("buildspec-backend.yml"),
            Timeout = Duration.Minutes(30)
        });

        // Grant CodeBuild project permission to push images to ECR
        props.EcrRepository.GrantPullPush(BackendBuildProject.GrantPrincipal);

        // Grant CodeBuild project permission to get ECR authorization token
        BackendBuildProject.AddToRolePolicy(new PolicyStatement(new PolicyStatementProps
        {
            Effect = Effect.ALLOW,
            Actions = new[] { "ecr:GetAuthorizationToken" },
            Resources = new[] { "*" }
        }));

        // ─── CodeBuild Project: Frontend (React build + S3 sync + CloudFront invalidation) ─
        FrontendBuildProject = new PipelineProject(this, "HSECockpit-Frontend-Build", new PipelineProjectProps
        {
            ProjectName = "HSECockpit-Frontend-Build",
            Description = "Builds React frontend, syncs to S3, and invalidates CloudFront cache",
            Environment = new BuildEnvironment
            {
                BuildImage = LinuxBuildImage.STANDARD_7_0,
                ComputeType = ComputeType.SMALL
            },
            EnvironmentVariables = new Dictionary<string, IBuildEnvironmentVariable>
            {
                ["FRONTEND_BUCKET_NAME"] = new BuildEnvironmentVariable
                {
                    Value = props.FrontendBucketName,
                    Type = BuildEnvironmentVariableType.PLAINTEXT
                },
                ["CLOUDFRONT_DISTRIBUTION_ID"] = new BuildEnvironmentVariable
                {
                    Value = props.CloudFrontDistributionId,
                    Type = BuildEnvironmentVariableType.PLAINTEXT
                }
            },
            BuildSpec = BuildSpec.FromSourceFilename("buildspec-frontend.yml"),
            Timeout = Duration.Minutes(20)
        });

        // Grant CodeBuild project permission to sync files to S3 frontend bucket
        FrontendBuildProject.AddToRolePolicy(new PolicyStatement(new PolicyStatementProps
        {
            Effect = Effect.ALLOW,
            Actions = new[]
            {
                "s3:PutObject",
                "s3:DeleteObject",
                "s3:GetObject",
                "s3:ListBucket"
            },
            Resources = new[]
            {
                props.FrontendBucketArn,
                $"{props.FrontendBucketArn}/*"
            }
        }));

        // Grant CodeBuild project permission to create CloudFront invalidation
        FrontendBuildProject.AddToRolePolicy(new PolicyStatement(new PolicyStatementProps
        {
            Effect = Effect.ALLOW,
            Actions = new[] { "cloudfront:CreateInvalidation" },
            Resources = new[] { props.CloudFrontDistributionArn }
        }));

        // ─── CodeBuild Project: Infrastructure (CDK synth → CloudFormation templates) ─
        InfraBuildProject = new PipelineProject(this, "HSECockpit-Infra-Build", new PipelineProjectProps
        {
            ProjectName = "HSECockpit-Infra-Build",
            Description = "Builds CDK infrastructure project and synthesizes CloudFormation templates",
            Environment = new BuildEnvironment
            {
                BuildImage = LinuxBuildImage.STANDARD_7_0,
                ComputeType = ComputeType.SMALL
            },
            BuildSpec = BuildSpec.FromSourceFilename("buildspec-infra.yml"),
            Timeout = Duration.Minutes(15)
        });

        // ─── Build Action: Backend ─────────────────────────────────────────────────
        var buildAction = new CodeBuildAction(new CodeBuildActionProps
        {
            ActionName = "Backend_Build",
            Project = BackendBuildProject,
            Input = SourceOutput,
            Outputs = new[] { BuildOutput },
            RunOrder = 1
        });

        // ─── Build Action: Frontend ──────────────────────────────────────────────────
        var frontendBuildOutput = new Artifact_("FrontendBuildOutput");
        var frontendBuildAction = new CodeBuildAction(new CodeBuildActionProps
        {
            ActionName = "Frontend_Build",
            Project = FrontendBuildProject,
            Input = SourceOutput,
            Outputs = new[] { frontendBuildOutput },
            RunOrder = 1
        });

        // ─── Build Action: Infrastructure ────────────────────────────────────────────
        var infraBuildOutput = new Artifact_("InfraBuildOutput");
        var infraBuildAction = new CodeBuildAction(new CodeBuildActionProps
        {
            ActionName = "Infra_Build",
            Project = InfraBuildProject,
            Input = SourceOutput,
            Outputs = new[] { infraBuildOutput },
            RunOrder = 1
        });

        // ─── Staging Deploy Action: ECS Rolling Update ─────────────────────────────────
        var stagingDeployAction = new EcsDeployAction(new EcsDeployActionProps
        {
            ActionName = "Staging_ECS_Deploy",
            Service = props.EcsService,
            Input = BuildOutput,
            DeploymentTimeout = Duration.Minutes(15),
            RunOrder = 1
        });

        // ─── SNS Topic: Pipeline Approval Notifications ──────────────────────────────
        var approvalNotificationTopic = new Topic(this, "PipelineApprovalTopic", new TopicProps
        {
            TopicName = "HSECockpit-Pipeline-Approval-Notifications",
            DisplayName = "HSECockpit Pipeline Production Approval"
        });

        // ─── Approval Action: Manual Gate before Production ──────────────────────────
        var manualApprovalAction = new ManualApprovalAction(new ManualApprovalActionProps
        {
            ActionName = "Production_Approval",
            NotificationTopic = approvalNotificationTopic,
            AdditionalInformation = "Please review the staging deployment and approve for production",
            RunOrder = 1
        });

        // ─── Production Deploy Action: ECS Blue/Green Deploy ─────────────────────────
        var productionDeployAction = new EcsDeployAction(new EcsDeployActionProps
        {
            ActionName = "Production_ECS_Deploy",
            Service = props.EcsService,
            Input = BuildOutput,
            DeploymentTimeout = Duration.Minutes(15),
            RunOrder = 1
        });

        // ─── CodePipeline ─────────────────────────────────────────────────────────────
        Pipeline = new Pipeline(this, "HSECockpit-Pipeline", new PipelineProps
        {
            PipelineName = "HSECockpit-Pipeline",
            RestartExecutionOnUpdate = true,
            Stages = new Amazon.CDK.AWS.CodePipeline.IStageProps[]
            {
                new Amazon.CDK.AWS.CodePipeline.StageProps
                {
                    StageName = "Source",
                    Actions = new IAction[] { sourceAction }
                },
                new Amazon.CDK.AWS.CodePipeline.StageProps
                {
                    StageName = "Build",
                    Actions = new IAction[] { buildAction, frontendBuildAction, infraBuildAction }
                },
                new Amazon.CDK.AWS.CodePipeline.StageProps
                {
                    StageName = "Staging",
                    Actions = new IAction[] { stagingDeployAction }
                },
                new Amazon.CDK.AWS.CodePipeline.StageProps
                {
                    StageName = "Approval",
                    Actions = new IAction[] { manualApprovalAction }
                },
                new Amazon.CDK.AWS.CodePipeline.StageProps
                {
                    StageName = "Production",
                    Actions = new IAction[] { productionDeployAction }
                }
            }
        });

        // ─── CloudFormation Outputs ───────────────────────────────────────────────────
        _ = new CfnOutput(this, "PipelineArn", new CfnOutputProps
        {
            Value = Pipeline.PipelineArn,
            Description = "ARN of the HSECockpit CI/CD pipeline",
            ExportName = "HSECockpit-Pipeline-Arn"
        });

        _ = new CfnOutput(this, "PipelineName", new CfnOutputProps
        {
            Value = Pipeline.PipelineName,
            Description = "Name of the HSECockpit CI/CD pipeline",
            ExportName = "HSECockpit-Pipeline-Name"
        });

        _ = new CfnOutput(this, "BackendBuildProjectName", new CfnOutputProps
        {
            Value = BackendBuildProject.ProjectName,
            Description = "CodeBuild project name for backend build",
            ExportName = "HSECockpit-Backend-Build-ProjectName"
        });

        _ = new CfnOutput(this, "FrontendBuildProjectName", new CfnOutputProps
        {
            Value = FrontendBuildProject.ProjectName,
            Description = "CodeBuild project name for frontend build",
            ExportName = "HSECockpit-Frontend-Build-ProjectName"
        });

        _ = new CfnOutput(this, "InfraBuildProjectName", new CfnOutputProps
        {
            Value = InfraBuildProject.ProjectName,
            Description = "CodeBuild project name for infrastructure CDK synth",
            ExportName = "HSECockpit-Infra-Build-ProjectName"
        });

        _ = new CfnOutput(this, "ApprovalTopicArn", new CfnOutputProps
        {
            Value = approvalNotificationTopic.TopicArn,
            Description = "SNS topic ARN for pipeline production approval notifications",
            ExportName = "HSECockpit-Pipeline-ApprovalTopic-Arn"
        });

        // ─── SNS Topic: Pipeline Failure Notifications ───────────────────────────────
        var failureNotificationTopic = new Topic(this, "PipelineFailureTopic", new TopicProps
        {
            TopicName = "HSECockpit-Pipeline-Failures",
            DisplayName = "HSECockpit Pipeline Failure Notifications"
        });

        // TODO: Add NotificationRule once codestar-notifications service-linked role is created in the account

        // ─── CloudFormation Output: Failure Topic ARN ─────────────────────────────────
        _ = new CfnOutput(this, "FailureTopicArn", new CfnOutputProps
        {
            Value = failureNotificationTopic.TopicArn,
            Description = "SNS topic ARN for pipeline failure notifications",
            ExportName = "HSECockpit-Pipeline-FailureTopic-Arn"
        });
    }
}
