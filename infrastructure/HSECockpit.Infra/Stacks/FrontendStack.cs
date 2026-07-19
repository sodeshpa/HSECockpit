using Amazon.CDK;
using Amazon.CDK.AWS.CloudFront;
using Amazon.CDK.AWS.CloudFront.Origins;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.WAFv2;
using Constructs;

namespace HSECockpit.Infra.Stacks;

/// <summary>
/// Frontend hosting: S3 bucket for static files, CloudFront distribution with OAI.
/// </summary>
public class FrontendStack : Stack
{
    /// <summary>The S3 bucket hosting the React SPA static assets.</summary>
    public Bucket WebsiteBucket { get; }

    /// <summary>The CloudFront distribution serving the frontend.</summary>
    public Distribution CloudFrontDistribution { get; }

    public FrontendStack(Construct scope, string id, IStackProps? props = null) : base(scope, id, props)
    {
        // S3 Bucket: Static hosting for React SPA
        WebsiteBucket = new Bucket(this, "HSECockpit-Frontend-Bucket", new BucketProps
        {
            BucketName = $"hsecockpit-frontend-{Account}-{Region}",
            Versioned = true,
            IntelligentTieringConfigurations = new IIntelligentTieringConfiguration[]
            {
                new IntelligentTieringConfiguration
                {
                    Name = "MoveToInfrequentAccess",
                    ArchiveAccessTierTime = Duration.Days(90),
                    DeepArchiveAccessTierTime = Duration.Days(180)
                }
            },
            BlockPublicAccess = BlockPublicAccess.BLOCK_ALL,
            Encryption = BucketEncryption.S3_MANAGED,
            RemovalPolicy = RemovalPolicy.DESTROY,
            AutoDeleteObjects = true
        });

        // Origin Access Identity: restricts S3 access to CloudFront only
        var originAccessIdentity = new OriginAccessIdentity(this, "HSECockpit-OAI", new OriginAccessIdentityProps
        {
            Comment = "OAI for HSECockpit frontend S3 bucket"
        });

        WebsiteBucket.GrantRead(originAccessIdentity);

        // WAF WebACL: basic rate limiting
        var webAcl = new CfnWebACL(this, "HSECockpit-Frontend-WAF", new CfnWebACLProps
        {
            DefaultAction = new CfnWebACL.DefaultActionProperty
            {
                Allow = new CfnWebACL.AllowActionProperty()
            },
            Scope = "CLOUDFRONT",
            VisibilityConfig = new CfnWebACL.VisibilityConfigProperty
            {
                CloudWatchMetricsEnabled = true,
                MetricName = "HSECockpitFrontendWAF",
                SampledRequestsEnabled = true
            },
            Name = "HSECockpit-Frontend-WAF",
            Rules = new object[]
            {
                new CfnWebACL.RuleProperty
                {
                    Name = "RateLimitRule",
                    Priority = 1,
                    Action = new CfnWebACL.RuleActionProperty
                    {
                        Block = new CfnWebACL.BlockActionProperty()
                    },
                    Statement = new CfnWebACL.StatementProperty
                    {
                        RateBasedStatement = new CfnWebACL.RateBasedStatementProperty
                        {
                            Limit = 2000,
                            AggregateKeyType = "IP"
                        }
                    },
                    VisibilityConfig = new CfnWebACL.VisibilityConfigProperty
                    {
                        CloudWatchMetricsEnabled = true,
                        MetricName = "HSECockpitRateLimit",
                        SampledRequestsEnabled = true
                    }
                },
                new CfnWebACL.RuleProperty
                {
                    Name = "AWSManagedRulesCommonRuleSet",
                    Priority = 2,
                    OverrideAction = new CfnWebACL.OverrideActionProperty
                    {
                        None = new Dictionary<string, object>()
                    },
                    Statement = new CfnWebACL.StatementProperty
                    {
                        ManagedRuleGroupStatement = new CfnWebACL.ManagedRuleGroupStatementProperty
                        {
                            VendorName = "AWS",
                            Name = "AWSManagedRulesCommonRuleSet"
                        }
                    },
                    VisibilityConfig = new CfnWebACL.VisibilityConfigProperty
                    {
                        CloudWatchMetricsEnabled = true,
                        MetricName = "HSECockpitCommonRules",
                        SampledRequestsEnabled = true
                    }
                }
            }
        });

        // Cache Policy: optimised for static assets
        var cachePolicy = new CachePolicy(this, "HSECockpit-CachePolicy", new CachePolicyProps
        {
            CachePolicyName = "HSECockpit-StaticAssets-CachePolicy",
            Comment = "Cache policy for HSECockpit static frontend assets",
            DefaultTtl = Duration.Days(1),
            MinTtl = Duration.Seconds(0),
            MaxTtl = Duration.Days(365),
            EnableAcceptEncodingGzip = true,
            EnableAcceptEncodingBrotli = true,
            HeaderBehavior = CacheHeaderBehavior.None(),
            QueryStringBehavior = CacheQueryStringBehavior.None(),
            CookieBehavior = CacheCookieBehavior.None()
        });

        // CloudFront Distribution
        CloudFrontDistribution = new Distribution(this, "HSECockpit-Distribution", new DistributionProps
        {
            Comment = "HSECockpit Frontend CDN Distribution",
            DefaultRootObject = "index.html",
            MinimumProtocolVersion = SecurityPolicyProtocol.TLS_V1_2_2021,
            HttpVersion = HttpVersion.HTTP2_AND_3,
            WebAclId = webAcl.AttrArn,
            DefaultBehavior = new BehaviorOptions
            {
                Origin = S3BucketOrigin.WithOriginAccessIdentity(WebsiteBucket, new S3BucketOriginWithOAIProps
                {
                    OriginAccessIdentity = originAccessIdentity
                }),
                ViewerProtocolPolicy = ViewerProtocolPolicy.REDIRECT_TO_HTTPS,
                CachePolicy = cachePolicy,
                AllowedMethods = AllowedMethods.ALLOW_GET_HEAD_OPTIONS,
                CachedMethods = CachedMethods.CACHE_GET_HEAD_OPTIONS,
                Compress = true
            },
            ErrorResponses = new IErrorResponse[]
            {
                new ErrorResponse
                {
                    HttpStatus = 403,
                    ResponseHttpStatus = 200,
                    ResponsePagePath = "/index.html",
                    Ttl = Duration.Minutes(5)
                },
                new ErrorResponse
                {
                    HttpStatus = 404,
                    ResponseHttpStatus = 200,
                    ResponsePagePath = "/index.html",
                    Ttl = Duration.Minutes(5)
                }
            }
        });

        // CloudFormation Outputs
        _ = new CfnOutput(this, "DistributionDomainName", new CfnOutputProps
        {
            Value = CloudFrontDistribution.DistributionDomainName,
            Description = "CloudFront distribution domain name for the frontend",
            ExportName = "HSECockpit-DistributionDomainName"
        });

        _ = new CfnOutput(this, "FrontendBucketName", new CfnOutputProps
        {
            Value = WebsiteBucket.BucketName,
            Description = "S3 bucket name for frontend static assets",
            ExportName = "HSECockpit-FrontendBucketName"
        });

        _ = new CfnOutput(this, "DistributionId", new CfnOutputProps
        {
            Value = CloudFrontDistribution.DistributionId,
            Description = "CloudFront distribution ID",
            ExportName = "HSECockpit-DistributionId"
        });
    }
}
