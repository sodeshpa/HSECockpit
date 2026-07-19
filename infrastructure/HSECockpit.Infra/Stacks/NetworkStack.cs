using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Constructs;

namespace HSECockpit.Infra.Stacks;

/// <summary>
/// Network infrastructure: VPC with public and private subnets, NAT Gateway, security groups.
/// </summary>
public class NetworkStack : Stack
{
    /// <summary>The VPC for all HSECockpit workloads.</summary>
    public Vpc Vpc { get; }

    /// <summary>Security group for ECS Fargate tasks (API).</summary>
    public SecurityGroup EcsSecurityGroup { get; }

    /// <summary>Security group for RDS PostgreSQL database.</summary>
    public SecurityGroup RdsSecurityGroup { get; }

    /// <summary>Security group for Lambda functions.</summary>
    public SecurityGroup LambdaSecurityGroup { get; }

    /// <summary>Security group for VPC interface endpoints (Secrets Manager, Bedrock).</summary>
    public SecurityGroup VpcEndpointSecurityGroup { get; }

    /// <summary>S3 Gateway VPC endpoint.</summary>
    public GatewayVpcEndpoint S3Endpoint { get; }

    /// <summary>Secrets Manager interface VPC endpoint.</summary>
    public InterfaceVpcEndpoint SecretsManagerEndpoint { get; }

    /// <summary>Bedrock Runtime interface VPC endpoint.</summary>
    public InterfaceVpcEndpoint BedrockRuntimeEndpoint { get; }

    public NetworkStack(Construct scope, string id, IStackProps? props = null) : base(scope, id, props)
    {
        // VPC with 2 AZs, 2 public subnets, 2 private subnets, and a NAT Gateway
        Vpc = new Vpc(this, "HSECockpit-VPC", new VpcProps
        {
            VpcName = "HSECockpit-VPC",
            MaxAzs = 2,
            NatGateways = 1,
            SubnetConfiguration = new ISubnetConfiguration[]
            {
                new SubnetConfiguration
                {
                    Name = "HSECockpit-Public",
                    SubnetType = SubnetType.PUBLIC,
                    CidrMask = 24
                },
                new SubnetConfiguration
                {
                    Name = "HSECockpit-Private",
                    SubnetType = SubnetType.PRIVATE_WITH_EGRESS,
                    CidrMask = 24
                }
            }
        });

        // Security Group: ECS Fargate tasks
        EcsSecurityGroup = new SecurityGroup(this, "HSECockpit-ECS-SG", new SecurityGroupProps
        {
            Vpc = Vpc,
            SecurityGroupName = "HSECockpit-ECS-SG",
            Description = "Security group for ECS Fargate tasks running the .NET API",
            AllowAllOutbound = true
        });

        // Security Group: Lambda functions
        LambdaSecurityGroup = new SecurityGroup(this, "HSECockpit-Lambda-SG", new SecurityGroupProps
        {
            Vpc = Vpc,
            SecurityGroupName = "HSECockpit-Lambda-SG",
            Description = "Security group for Lambda ingestion functions",
            AllowAllOutbound = true
        });

        // Security Group: RDS PostgreSQL - only accessible from ECS and Lambda
        RdsSecurityGroup = new SecurityGroup(this, "HSECockpit-RDS-SG", new SecurityGroupProps
        {
            Vpc = Vpc,
            SecurityGroupName = "HSECockpit-RDS-SG",
            Description = "Security group for RDS PostgreSQL - allows port 5432 from ECS and Lambda only",
            AllowAllOutbound = false
        });

        RdsSecurityGroup.AddIngressRule(
            EcsSecurityGroup,
            Port.Tcp(5432),
            "Allow PostgreSQL access from ECS tasks"
        );

        RdsSecurityGroup.AddIngressRule(
            LambdaSecurityGroup,
            Port.Tcp(5432),
            "Allow PostgreSQL access from Lambda functions"
        );

        // Security Group: VPC Interface Endpoints - allows HTTPS from ECS and Lambda
        VpcEndpointSecurityGroup = new SecurityGroup(this, "HSECockpit-VPCEndpoint-SG", new SecurityGroupProps
        {
            Vpc = Vpc,
            SecurityGroupName = "HSECockpit-VPCEndpoint-SG",
            Description = "Security group for VPC interface endpoints - allows HTTPS (443) from ECS and Lambda",
            AllowAllOutbound = false
        });

        VpcEndpointSecurityGroup.AddIngressRule(
            EcsSecurityGroup,
            Port.Tcp(443),
            "Allow HTTPS from ECS tasks"
        );

        VpcEndpointSecurityGroup.AddIngressRule(
            LambdaSecurityGroup,
            Port.Tcp(443),
            "Allow HTTPS from Lambda functions"
        );

        // VPC Endpoint: S3 Gateway - allows private subnet resources to access S3 without NAT
        S3Endpoint = new GatewayVpcEndpoint(this, "HSECockpit-S3-Endpoint", new GatewayVpcEndpointProps
        {
            Vpc = Vpc,
            Service = GatewayVpcEndpointAwsService.S3,
            Subnets = new SubnetSelection[]
            {
                new SubnetSelection { SubnetType = SubnetType.PRIVATE_WITH_EGRESS }
            }
        });

        // VPC Endpoint: Secrets Manager Interface - allows ECS/Lambda to retrieve secrets without NAT
        SecretsManagerEndpoint = new InterfaceVpcEndpoint(this, "HSECockpit-SecretsManager-Endpoint", new InterfaceVpcEndpointProps
        {
            Vpc = Vpc,
            Service = InterfaceVpcEndpointAwsService.SECRETS_MANAGER,
            Subnets = new SubnetSelection { SubnetType = SubnetType.PRIVATE_WITH_EGRESS },
            SecurityGroups = new ISecurityGroup[] { VpcEndpointSecurityGroup },
            PrivateDnsEnabled = true
        });

        // VPC Endpoint: Bedrock Runtime Interface - allows ECS to call Amazon Bedrock without NAT
        BedrockRuntimeEndpoint = new InterfaceVpcEndpoint(this, "HSECockpit-BedrockRuntime-Endpoint", new InterfaceVpcEndpointProps
        {
            Vpc = Vpc,
            Service = InterfaceVpcEndpointAwsService.BEDROCK_RUNTIME,
            Subnets = new SubnetSelection { SubnetType = SubnetType.PRIVATE_WITH_EGRESS },
            SecurityGroups = new ISecurityGroup[] { VpcEndpointSecurityGroup },
            PrivateDnsEnabled = true
        });

        // CloudFormation Outputs for cross-stack references
        _ = new CfnOutput(this, "VpcId", new CfnOutputProps
        {
            Value = Vpc.VpcId,
            Description = "VPC ID for HSECockpit",
            ExportName = "HSECockpit-VpcId"
        });

        _ = new CfnOutput(this, "EcsSecurityGroupId", new CfnOutputProps
        {
            Value = EcsSecurityGroup.SecurityGroupId,
            Description = "Security Group ID for ECS Fargate tasks",
            ExportName = "HSECockpit-ECS-SG-Id"
        });

        _ = new CfnOutput(this, "RdsSecurityGroupId", new CfnOutputProps
        {
            Value = RdsSecurityGroup.SecurityGroupId,
            Description = "Security Group ID for RDS PostgreSQL",
            ExportName = "HSECockpit-RDS-SG-Id"
        });

        _ = new CfnOutput(this, "LambdaSecurityGroupId", new CfnOutputProps
        {
            Value = LambdaSecurityGroup.SecurityGroupId,
            Description = "Security Group ID for Lambda functions",
            ExportName = "HSECockpit-Lambda-SG-Id"
        });
    }
}
