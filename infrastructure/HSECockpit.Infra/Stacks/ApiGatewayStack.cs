using Amazon.CDK;
using Amazon.CDK.AWS.Apigatewayv2;
using Amazon.CDK.AWS.Cognito;
using Amazon.CDK.AWS.EC2;
using Constructs;

namespace HSECockpit.Infra.Stacks;

/// <summary>
/// Properties required by ApiGatewayStack from other stacks.
/// </summary>
public class ApiGatewayStackProps : StackProps
{
    /// <summary>The VPC where the ECS service runs (needed for VPC Link).</summary>
    public required IVpc Vpc { get; init; }

    /// <summary>The ECS security group to associate with the VPC Link.</summary>
    public required ISecurityGroup EcsSecurityGroup { get; init; }

    /// <summary>The CloudFront domain name for CORS allow-origin (frontend).</summary>
    public required string FrontendDomainName { get; init; }

    /// <summary>The ALB Listener ARN for VPC Link integration.</summary>
    public required string AlbListenerArn { get; init; }
}

/// <summary>
/// API Gateway: HTTP API with VPC Link to ECS, Cognito User Pool and JWT authorizer.
/// </summary>
public class ApiGatewayStack : Stack
{
    /// <summary>The Cognito User Pool for authentication.</summary>
    public UserPool UserPool { get; }

    /// <summary>The Cognito User Pool App Client for the React SPA.</summary>
    public UserPoolClient UserPoolClient { get; }

    /// <summary>The Cognito hosted UI domain.</summary>
    public UserPoolDomain UserPoolDomain { get; }

    /// <summary>The HTTP API Gateway.</summary>
    public CfnApi HttpApi { get; }

    public ApiGatewayStack(Construct scope, string id, ApiGatewayStackProps props) : base(scope, id, props)
    {
        // ─── Cognito User Pool ────────────────────────────────────────────────────────
        UserPool = new UserPool(this, "HSECockpit-UserPool", new UserPoolProps
        {
            UserPoolName = "HSECockpit-UserPool",
            SelfSignUpEnabled = false,
            SignInAliases = new SignInAliases
            {
                Email = true
            },
            AutoVerify = new AutoVerifiedAttrs
            {
                Email = true
            },
            Mfa = Mfa.OPTIONAL,
            MfaSecondFactor = new MfaSecondFactor
            {
                Sms = false,
                Otp = true
            },
            PasswordPolicy = new PasswordPolicy
            {
                MinLength = 12,
                RequireLowercase = true,
                RequireUppercase = true,
                RequireDigits = true,
                RequireSymbols = true,
                TempPasswordValidity = Duration.Days(7)
            },
            AccountRecovery = AccountRecovery.EMAIL_ONLY,
            CustomAttributes = new Dictionary<string, ICustomAttribute>
            {
                ["role"] = new StringAttribute(new StringAttributeProps
                {
                    Mutable = true,
                    MinLen = 1,
                    MaxLen = 50
                })
            },
            RemovalPolicy = RemovalPolicy.DESTROY
        });

        // ─── Cognito Hosted UI Domain ─────────────────────────────────────────────────
        UserPoolDomain = UserPool.AddDomain("HSECockpit-CognitoDomain", new UserPoolDomainOptions
        {
            CognitoDomain = new CognitoDomainOptions
            {
                DomainPrefix = $"hsecockpit-{Account}"
            }
        });

        // ─── Cognito App Client (SPA with PKCE, no client secret) ─────────────────────
        UserPoolClient = UserPool.AddClient("HSECockpit-SPA-Client", new UserPoolClientOptions
        {
            UserPoolClientName = "HSECockpit-SPA",
            GenerateSecret = false,
            AuthFlows = new AuthFlow
            {
                UserSrp = true
            },
            OAuth = new OAuthSettings
            {
                Flows = new OAuthFlows
                {
                    AuthorizationCodeGrant = true
                },
                Scopes = new[]
                {
                    OAuthScope.OPENID,
                    OAuthScope.EMAIL,
                    OAuthScope.PROFILE
                },
                CallbackUrls = new[]
                {
                    "http://localhost:5173/callback",
                    $"https://{props.FrontendDomainName}/callback"
                },
                LogoutUrls = new[]
                {
                    "http://localhost:5173/",
                    $"https://{props.FrontendDomainName}/"
                }
            },
            SupportedIdentityProviders = new[]
            {
                UserPoolClientIdentityProvider.COGNITO
            },
            AccessTokenValidity = Duration.Hours(1),
            IdTokenValidity = Duration.Hours(1),
            RefreshTokenValidity = Duration.Days(30)
        });

        // ─── VPC Link (connects API Gateway to private subnets) ───────────────────────
        var vpcLink = new CfnVpcLink(this, "HSECockpit-VpcLink", new CfnVpcLinkProps
        {
            Name = "HSECockpit-VpcLink",
            SubnetIds = props.Vpc.SelectSubnets(new SubnetSelection
            {
                SubnetType = SubnetType.PRIVATE_WITH_EGRESS
            }).SubnetIds,
            SecurityGroupIds = new[] { props.EcsSecurityGroup.SecurityGroupId }
        });

        // ─── HTTP API Gateway ─────────────────────────────────────────────────────────
        HttpApi = new CfnApi(this, "HSECockpit-HttpApi", new CfnApiProps
        {
            Name = "HSECockpit-HttpApi",
            ProtocolType = "HTTP",
            CorsConfiguration = new CfnApi.CorsProperty
            {
                AllowOrigins = new[]
                {
                    "http://localhost:5173",
                    $"https://{props.FrontendDomainName}"
                },
                AllowMethods = new[] { "GET", "POST", "PUT", "DELETE", "OPTIONS" },
                AllowHeaders = new[] { "Authorization", "Content-Type", "X-Correlation-Id" },
                MaxAge = 3600
            }
        });

        // ─── JWT Authorizer (Cognito) ─────────────────────────────────────────────────
        var jwtAuthorizer = new CfnAuthorizer(this, "HSECockpit-JwtAuthorizer", new CfnAuthorizerProps
        {
            ApiId = HttpApi.Ref,
            AuthorizerType = "JWT",
            Name = "HSECockpit-CognitoAuthorizer",
            IdentitySource = new[] { "$request.header.Authorization" },
            JwtConfiguration = new CfnAuthorizer.JWTConfigurationProperty
            {
                Audience = new[] { UserPoolClient.UserPoolClientId },
                Issuer = $"https://cognito-idp.{Region}.amazonaws.com/{UserPool.UserPoolId}"
            }
        });

        // ─── VPC Link Integration (route to ECS via ALB) ──────────────────────────────
        var vpcLinkIntegration = new CfnIntegration(this, "HSECockpit-VpcLinkIntegration", new CfnIntegrationProps
        {
            ApiId = HttpApi.Ref,
            IntegrationType = "HTTP_PROXY",
            IntegrationMethod = "ANY",
            IntegrationUri = props.AlbListenerArn,
            ConnectionType = "VPC_LINK",
            ConnectionId = vpcLink.Ref,
            PayloadFormatVersion = "1.0"
        });

        // ─── Route: /api/v1/{proxy+} → ECS via VPC Link ──────────────────────────────
        _ = new Amazon.CDK.AWS.Apigatewayv2.CfnRoute(this, "HSECockpit-ApiRoute", new Amazon.CDK.AWS.Apigatewayv2.CfnRouteProps
        {
            ApiId = HttpApi.Ref,
            RouteKey = "ANY /api/v1/{proxy+}",
            Target = $"integrations/{vpcLinkIntegration.Ref}",
            AuthorizationType = "NONE"
        });

        // ─── Stage with throttling ────────────────────────────────────────────────────
        _ = new CfnStage(this, "HSECockpit-DefaultStage", new CfnStageProps
        {
            ApiId = HttpApi.Ref,
            StageName = "$default",
            AutoDeploy = true,
            DefaultRouteSettings = new CfnStage.RouteSettingsProperty
            {
                ThrottlingBurstLimit = 200,
                ThrottlingRateLimit = 100
            }
        });

        // ─── CloudFormation Outputs ───────────────────────────────────────────────────
        _ = new CfnOutput(this, "ApiGatewayUrl", new CfnOutputProps
        {
            Value = $"https://{HttpApi.Ref}.execute-api.{Region}.amazonaws.com",
            Description = "HTTP API Gateway URL",
            ExportName = "HSECockpit-ApiGatewayUrl"
        });

        _ = new CfnOutput(this, "CognitoUserPoolId", new CfnOutputProps
        {
            Value = UserPool.UserPoolId,
            Description = "Cognito User Pool ID",
            ExportName = "HSECockpit-CognitoUserPoolId"
        });

        _ = new CfnOutput(this, "CognitoAppClientId", new CfnOutputProps
        {
            Value = UserPoolClient.UserPoolClientId,
            Description = "Cognito App Client ID for React SPA",
            ExportName = "HSECockpit-CognitoAppClientId"
        });

        _ = new CfnOutput(this, "CognitoDomain", new CfnOutputProps
        {
            Value = $"{UserPoolDomain.DomainName}.auth.{Region}.amazoncognito.com",
            Description = "Cognito Hosted UI Domain",
            ExportName = "HSECockpit-CognitoDomain"
        });
    }
}
