using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECR;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ElasticLoadBalancingV2;
using Amazon.CDK.AWS.ApplicationAutoScaling;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Scheduler;
using Amazon.CDK.AWS.SQS;
using Constructs;

namespace HSECockpit.Infra.Stacks;

/// <summary>
/// Properties required by ComputeStack from other stacks.
/// </summary>
public class ComputeStackProps : StackProps
{
    /// <summary>The VPC where compute resources will be deployed.</summary>
    public required IVpc Vpc { get; init; }

    /// <summary>The security group for ECS Fargate tasks.</summary>
    public required ISecurityGroup EcsSecurityGroup { get; init; }

    /// <summary>The security group for Lambda functions.</summary>
    public required ISecurityGroup LambdaSecurityGroup { get; init; }

    /// <summary>The Cognito User Pool ID for JWT validation.</summary>
    public string? CognitoUserPoolId { get; init; }

    /// <summary>The Cognito App Client ID for JWT validation.</summary>
    public string? CognitoClientId { get; init; }

    /// <summary>The RDS endpoint for the database connection.</summary>
    public string? RdsEndpoint { get; init; }

    /// <summary>The database secret ARN in Secrets Manager.</summary>
    public string? DatabaseSecretArn { get; init; }
}

/// <summary>
/// Compute resources: ECS Fargate cluster, task definitions, ECR repository, Lambda functions for ingestion.
/// </summary>
public class ComputeStack : Stack
{
    /// <summary>The ECS Fargate cluster.</summary>
    public Cluster EcsCluster { get; }

    /// <summary>The ECR repository for the API container image.</summary>
    public Repository EcrRepository { get; }

    /// <summary>The ECS Fargate service running the API.</summary>
    public FargateService ApiService { get; }

    /// <summary>The ALB Listener for API Gateway VPC Link integration.</summary>
    public ApplicationListener AlbListener { get; }

    /// <summary>The Barrier Ingestion Lambda function.</summary>
    public Function BarrierIngestionFunction { get; }

    /// <summary>The Incident Ingestion Lambda function.</summary>
    public Function IncidentIngestionFunction { get; }

    /// <summary>The Maintenance Ingestion Lambda function.</summary>
    public Function MaintenanceIngestionFunction { get; }

    public ComputeStack(Construct scope, string id, ComputeStackProps props) : base(scope, id, props)
    {
        // ─── ECR Repository ───────────────────────────────────────────────────────────
        EcrRepository = new Repository(this, "HSECockpit-ECR", new RepositoryProps
        {
            RepositoryName = "hsecockpit-api",
            ImageScanOnPush = true,
            LifecycleRules = new ILifecycleRule[]
            {
                new LifecycleRule
                {
                    MaxImageCount = 10,
                    Description = "Keep only the last 10 images"
                }
            },
            RemovalPolicy = RemovalPolicy.DESTROY
        });

        // ─── ECS Fargate Cluster ──────────────────────────────────────────────────────
        EcsCluster = new Cluster(this, "HSECockpit-ECS-Cluster", new ClusterProps
        {
            ClusterName = "HSECockpit-Cluster",
            Vpc = props.Vpc,
            ContainerInsights = true
        });

        // ─── Fargate Task Definition ──────────────────────────────────────────────────
        var taskDefinition = new FargateTaskDefinition(this, "HSECockpit-TaskDef", new FargateTaskDefinitionProps
        {
            Cpu = 512,
            MemoryLimitMiB = 1024,
            Family = "HSECockpit-API"
        });

        taskDefinition.AddContainer("HSECockpit-API-Container", new ContainerDefinitionOptions
        {
            Image = ContainerImage.FromEcrRepository(EcrRepository, "latest"),
            Logging = LogDrivers.AwsLogs(new AwsLogDriverProps
            {
                StreamPrefix = "hsecockpit-api"
            }),
            Environment = new Dictionary<string, string>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "Development",
                ["Cognito__Authority"] = props.CognitoUserPoolId != null
                    ? $"https://cognito-idp.{Region}.amazonaws.com/{props.CognitoUserPoolId}"
                    : "",
                ["Cognito__ClientId"] = props.CognitoClientId ?? "",
                ["ConnectionStrings__HseCockpit"] = props.RdsEndpoint != null
                    ? $"Host={props.RdsEndpoint};Port=5432;Database=hsecockpit;Username=hsecockpit_admin;Password=IE3S9x2fjkRRNSAFq3^I,KSvfioRzs"
                    : ""
            },
            PortMappings = new IPortMapping[]
            {
                new PortMapping
                {
                    ContainerPort = 8080,
                    Protocol = Amazon.CDK.AWS.ECS.Protocol.TCP
                }
            }
        });

        // ─── Fargate Service ──────────────────────────────────────────────────────────
        ApiService = new FargateService(this, "HSECockpit-API-Service", new FargateServiceProps
        {
            Cluster = EcsCluster,
            TaskDefinition = taskDefinition,
            DesiredCount = 0, // Set to 0 for initial deployment (no Docker image in ECR yet). Scale up after first image push.
            SecurityGroups = new[] { props.EcsSecurityGroup },
            VpcSubnets = new SubnetSelection
            {
                SubnetType = SubnetType.PRIVATE_WITH_EGRESS
            },
            AssignPublicIp = false,
            ServiceName = "HSECockpit-API"
        });

        // ─── Internal ALB (routes API Gateway traffic to ECS) ─────────────────────────
        var alb = new ApplicationLoadBalancer(this, "HSECockpit-ALB", new ApplicationLoadBalancerProps
        {
            Vpc = props.Vpc,
            InternetFacing = false,
            SecurityGroup = props.EcsSecurityGroup,
            LoadBalancerName = "HSECockpit-API-ALB",
            VpcSubnets = new SubnetSelection { SubnetType = SubnetType.PRIVATE_WITH_EGRESS }
        });

        AlbListener = alb.AddListener("HSECockpit-ALB-Listener", new BaseApplicationListenerProps
        {
            Port = 80,
            Protocol = ApplicationProtocol.HTTP
        });

        AlbListener.AddTargets("HSECockpit-ECS-Target", new AddApplicationTargetsProps
        {
            Port = 8080,
            Protocol = ApplicationProtocol.HTTP,
            Targets = new[] { ApiService },
            HealthCheck = new Amazon.CDK.AWS.ElasticLoadBalancingV2.HealthCheck
            {
                Path = "/health",
                HealthyHttpCodes = "200",
                Interval = Duration.Seconds(30),
                Timeout = Duration.Seconds(5)
            }
        });

        // ─── Auto-Scaling (CPU-based, 1–4 tasks) ─────────────────────────────────────
        var scaling = ApiService.AutoScaleTaskCount(new EnableScalingProps
        {
            MinCapacity = 1,
            MaxCapacity = 4
        });

        scaling.ScaleOnCpuUtilization("HSECockpit-CPU-Scaling", new CpuUtilizationScalingProps
        {
            TargetUtilizationPercent = 70,
            ScaleInCooldown = Duration.Seconds(60),
            ScaleOutCooldown = Duration.Seconds(60)
        });

        // ─── Lambda: Barrier Ingestion ────────────────────────────────────────────────
        var barrierDlq = new Queue(this, "HSECockpit-BarrierIngestion-DLQ", new QueueProps
        {
            QueueName = "HSECockpit-BarrierIngestion-DLQ",
            RetentionPeriod = Duration.Days(14)
        });

        BarrierIngestionFunction = new Function(this, "HSECockpit-BarrierIngestion", new FunctionProps
        {
            FunctionName = "HSECockpit-BarrierIngestion",
            Runtime = Runtime.DOTNET_8,
            Handler = "D4HSE.Ingestion::D4HSE.Ingestion.Functions.BarrierIngestionFunction::FunctionHandler",
            Code = Code.FromAsset("../../backend/D4HSE.Ingestion/bin/Release/net8.0/publish"),
            MemorySize = 512,
            Timeout = Duration.Minutes(5),
            Vpc = props.Vpc,
            VpcSubnets = new SubnetSelection
            {
                SubnetType = SubnetType.PRIVATE_WITH_EGRESS
            },
            SecurityGroups = new[] { props.LambdaSecurityGroup },
            DeadLetterQueue = barrierDlq
        });

        // ─── Lambda: Incident Ingestion ───────────────────────────────────────────────
        var incidentDlq = new Queue(this, "HSECockpit-IncidentIngestion-DLQ", new QueueProps
        {
            QueueName = "HSECockpit-IncidentIngestion-DLQ",
            RetentionPeriod = Duration.Days(14)
        });

        IncidentIngestionFunction = new Function(this, "HSECockpit-IncidentIngestion", new FunctionProps
        {
            FunctionName = "HSECockpit-IncidentIngestion",
            Runtime = Runtime.DOTNET_8,
            Handler = "D4HSE.Ingestion::D4HSE.Ingestion.Functions.IncidentIngestionFunction::FunctionHandler",
            Code = Code.FromAsset("../../backend/D4HSE.Ingestion/bin/Release/net8.0/publish"),
            MemorySize = 512,
            Timeout = Duration.Minutes(5),
            Vpc = props.Vpc,
            VpcSubnets = new SubnetSelection
            {
                SubnetType = SubnetType.PRIVATE_WITH_EGRESS
            },
            SecurityGroups = new[] { props.LambdaSecurityGroup },
            DeadLetterQueue = incidentDlq
        });

        // ─── Lambda: Maintenance Ingestion ────────────────────────────────────────────
        var maintenanceDlq = new Queue(this, "HSECockpit-MaintenanceIngestion-DLQ", new QueueProps
        {
            QueueName = "HSECockpit-MaintenanceIngestion-DLQ",
            RetentionPeriod = Duration.Days(14)
        });

        MaintenanceIngestionFunction = new Function(this, "HSECockpit-MaintenanceIngestion", new FunctionProps
        {
            FunctionName = "HSECockpit-MaintenanceIngestion",
            Runtime = Runtime.DOTNET_8,
            Handler = "D4HSE.Ingestion::D4HSE.Ingestion.Functions.MaintenanceIngestionFunction::FunctionHandler",
            Code = Code.FromAsset("../../backend/D4HSE.Ingestion/bin/Release/net8.0/publish"),
            MemorySize = 512,
            Timeout = Duration.Minutes(5),
            Vpc = props.Vpc,
            VpcSubnets = new SubnetSelection
            {
                SubnetType = SubnetType.PRIVATE_WITH_EGRESS
            },
            SecurityGroups = new[] { props.LambdaSecurityGroup },
            DeadLetterQueue = maintenanceDlq
        });

        // ─── EventBridge Scheduler: IAM Role ─────────────────────────────────────────
        var schedulerRole = new Role(this, "HSECockpit-SchedulerRole", new RoleProps
        {
            RoleName = "HSECockpit-EventBridge-Scheduler-Role",
            AssumedBy = new ServicePrincipal("scheduler.amazonaws.com"),
            Description = "Allows EventBridge Scheduler to invoke ingestion Lambda functions"
        });

        BarrierIngestionFunction.GrantInvoke(schedulerRole);
        IncidentIngestionFunction.GrantInvoke(schedulerRole);
        MaintenanceIngestionFunction.GrantInvoke(schedulerRole);

        // ─── EventBridge Scheduler: Barrier Ingestion ─────────────────────────────────
        _ = new CfnSchedule(this, "HSECockpit-BarrierIngestion-Schedule", new CfnScheduleProps
        {
            Name = "HSECockpit-BarrierIngestion-Schedule",
            Description = "Triggers BarrierIngestion Lambda every 6 hours",
            ScheduleExpression = "rate(6 hours)",
            FlexibleTimeWindow = new CfnSchedule.FlexibleTimeWindowProperty
            {
                Mode = "OFF"
            },
            Target = new CfnSchedule.TargetProperty
            {
                Arn = BarrierIngestionFunction.FunctionArn,
                RoleArn = schedulerRole.RoleArn
            },
            State = "ENABLED"
        });

        // ─── EventBridge Scheduler: Incident Ingestion ────────────────────────────────
        _ = new CfnSchedule(this, "HSECockpit-IncidentIngestion-Schedule", new CfnScheduleProps
        {
            Name = "HSECockpit-IncidentIngestion-Schedule",
            Description = "Triggers IncidentIngestion Lambda every 6 hours",
            ScheduleExpression = "rate(6 hours)",
            FlexibleTimeWindow = new CfnSchedule.FlexibleTimeWindowProperty
            {
                Mode = "OFF"
            },
            Target = new CfnSchedule.TargetProperty
            {
                Arn = IncidentIngestionFunction.FunctionArn,
                RoleArn = schedulerRole.RoleArn
            },
            State = "ENABLED"
        });

        // ─── EventBridge Scheduler: Maintenance Ingestion ─────────────────────────────
        _ = new CfnSchedule(this, "HSECockpit-MaintenanceIngestion-Schedule", new CfnScheduleProps
        {
            Name = "HSECockpit-MaintenanceIngestion-Schedule",
            Description = "Triggers MaintenanceIngestion Lambda every 6 hours",
            ScheduleExpression = "rate(6 hours)",
            FlexibleTimeWindow = new CfnSchedule.FlexibleTimeWindowProperty
            {
                Mode = "OFF"
            },
            Target = new CfnSchedule.TargetProperty
            {
                Arn = MaintenanceIngestionFunction.FunctionArn,
                RoleArn = schedulerRole.RoleArn
            },
            State = "ENABLED"
        });

        // ─── CloudFormation Outputs ───────────────────────────────────────────────────
        _ = new CfnOutput(this, "EcsClusterArn", new CfnOutputProps
        {
            Value = EcsCluster.ClusterArn,
            Description = "ECS Fargate cluster ARN",
            ExportName = "HSECockpit-ECS-ClusterArn"
        });

        _ = new CfnOutput(this, "EcrRepositoryUri", new CfnOutputProps
        {
            Value = EcrRepository.RepositoryUri,
            Description = "ECR repository URI for API container image",
            ExportName = "HSECockpit-ECR-RepositoryUri"
        });

        _ = new CfnOutput(this, "BarrierIngestionFunctionArn", new CfnOutputProps
        {
            Value = BarrierIngestionFunction.FunctionArn,
            Description = "Barrier Ingestion Lambda function ARN",
            ExportName = "HSECockpit-Lambda-BarrierIngestion-Arn"
        });

        _ = new CfnOutput(this, "IncidentIngestionFunctionArn", new CfnOutputProps
        {
            Value = IncidentIngestionFunction.FunctionArn,
            Description = "Incident Ingestion Lambda function ARN",
            ExportName = "HSECockpit-Lambda-IncidentIngestion-Arn"
        });

        _ = new CfnOutput(this, "MaintenanceIngestionFunctionArn", new CfnOutputProps
        {
            Value = MaintenanceIngestionFunction.FunctionArn,
            Description = "Maintenance Ingestion Lambda function ARN",
            ExportName = "HSECockpit-Lambda-MaintenanceIngestion-Arn"
        });

        _ = new CfnOutput(this, "AlbListenerArn", new CfnOutputProps
        {
            Value = AlbListener.ListenerArn,
            Description = "ALB Listener ARN for API Gateway integration",
            ExportName = "HSECockpit-ALB-ListenerArn"
        });
    }
}
