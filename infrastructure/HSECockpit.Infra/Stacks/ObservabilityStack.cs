using Amazon.CDK;
using Amazon.CDK.AWS.CloudWatch;
using Amazon.CDK.AWS.CloudWatch.Actions;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.SNS;
using Amazon.CDK.AWS.XRay;
using Constructs;

namespace HSECockpit.Infra.Stacks;

/// <summary>
/// Observability: CloudWatch log groups, metric alarms, X-Ray tracing configuration.
/// </summary>
public class ObservabilityStack : Stack
{
    /// <summary>SNS topic for critical alarm notifications.</summary>
    public Topic CriticalAlarmsTopic { get; }

    /// <summary>CloudWatch log group for the ECS .NET API.</summary>
    public LogGroup ApiLogGroup { get; }

    /// <summary>CloudWatch log group for the barrier ingestion Lambda.</summary>
    public LogGroup BarrierIngestionLogGroup { get; }

    /// <summary>CloudWatch log group for the incident ingestion Lambda.</summary>
    public LogGroup IncidentIngestionLogGroup { get; }

    /// <summary>CloudWatch log group for the maintenance ingestion Lambda.</summary>
    public LogGroup MaintenanceIngestionLogGroup { get; }

    public ObservabilityStack(Construct scope, string id, IStackProps? props = null) : base(scope, id, props)
    {
        // ─── SNS Topic for Critical Alarms ───────────────────────────────────────
        CriticalAlarmsTopic = new Topic(this, "CriticalAlarmsTopic", new TopicProps
        {
            TopicName = "HSECockpit-CriticalAlarms",
            DisplayName = "HSECockpit Critical Alarm Notifications"
        });

        // ─── CloudWatch Log Groups ───────────────────────────────────────────────
        ApiLogGroup = new LogGroup(this, "ApiLogGroup", new LogGroupProps
        {
            LogGroupName = "/hsecockpit/api",
            Retention = RetentionDays.ONE_MONTH,
            RemovalPolicy = RemovalPolicy.DESTROY
        });

        BarrierIngestionLogGroup = new LogGroup(this, "BarrierIngestionLogGroup", new LogGroupProps
        {
            LogGroupName = "/hsecockpit/lambda/barrier-ingestion",
            Retention = RetentionDays.TWO_WEEKS,
            RemovalPolicy = RemovalPolicy.DESTROY
        });

        IncidentIngestionLogGroup = new LogGroup(this, "IncidentIngestionLogGroup", new LogGroupProps
        {
            LogGroupName = "/hsecockpit/lambda/incident-ingestion",
            Retention = RetentionDays.TWO_WEEKS,
            RemovalPolicy = RemovalPolicy.DESTROY
        });

        MaintenanceIngestionLogGroup = new LogGroup(this, "MaintenanceIngestionLogGroup", new LogGroupProps
        {
            LogGroupName = "/hsecockpit/lambda/maintenance-ingestion",
            Retention = RetentionDays.TWO_WEEKS,
            RemovalPolicy = RemovalPolicy.DESTROY
        });

        // ─── CloudWatch Alarms ───────────────────────────────────────────────────

        // API error rate alarm: 5xx responses > 5%
        var apiErrorRateAlarm = new Alarm(this, "ApiErrorRateAlarm", new AlarmProps
        {
            AlarmName = "HSECockpit-API-ErrorRate-High",
            AlarmDescription = "API 5xx error rate exceeds 5%",
            Metric = new Metric(new MetricProps
            {
                Namespace = "AWS/ApiGateway",
                MetricName = "5XXError",
                Statistic = "Average",
                Period = Duration.Minutes(5)
            }),
            Threshold = 0.05,
            EvaluationPeriods = 3,
            ComparisonOperator = ComparisonOperator.GREATER_THAN_THRESHOLD,
            TreatMissingData = TreatMissingData.NOT_BREACHING
        });
        apiErrorRateAlarm.AddAlarmAction(new SnsAction(CriticalAlarmsTopic));

        // Lambda DLQ depth alarm: any messages in the ingestion DLQs
        var dlqDepthAlarm = new Alarm(this, "LambdaDlqDepthAlarm", new AlarmProps
        {
            AlarmName = "HSECockpit-Lambda-DLQ-Depth",
            AlarmDescription = "Ingestion Lambda DLQ has messages (failures not processed)",
            Metric = new Metric(new MetricProps
            {
                Namespace = "AWS/SQS",
                MetricName = "ApproximateNumberOfMessagesVisible",
                DimensionsMap = new Dictionary<string, string>
                {
                    { "QueueName", "HSECockpit-Ingestion-DLQ" }
                },
                Statistic = "Maximum",
                Period = Duration.Minutes(5)
            }),
            Threshold = 0,
            EvaluationPeriods = 1,
            ComparisonOperator = ComparisonOperator.GREATER_THAN_THRESHOLD,
            TreatMissingData = TreatMissingData.NOT_BREACHING
        });
        dlqDepthAlarm.AddAlarmAction(new SnsAction(CriticalAlarmsTopic));

        // RDS CPU utilization alarm: > 80%
        var rdsCpuAlarm = new Alarm(this, "RdsCpuAlarm", new AlarmProps
        {
            AlarmName = "HSECockpit-RDS-CPU-High",
            AlarmDescription = "RDS PostgreSQL CPU utilization exceeds 80%",
            Metric = new Metric(new MetricProps
            {
                Namespace = "AWS/RDS",
                MetricName = "CPUUtilization",
                DimensionsMap = new Dictionary<string, string>
                {
                    { "DBInstanceIdentifier", "hsecockpit-db" }
                },
                Statistic = "Average",
                Period = Duration.Minutes(5)
            }),
            Threshold = 80,
            EvaluationPeriods = 3,
            ComparisonOperator = ComparisonOperator.GREATER_THAN_THRESHOLD,
            TreatMissingData = TreatMissingData.MISSING
        });
        rdsCpuAlarm.AddAlarmAction(new SnsAction(CriticalAlarmsTopic));

        // Ingestion failure alarm: Lambda errors
        var ingestionFailureAlarm = new Alarm(this, "IngestionFailureAlarm", new AlarmProps
        {
            AlarmName = "HSECockpit-Ingestion-Failures",
            AlarmDescription = "Ingestion Lambda function errors detected",
            Metric = new Metric(new MetricProps
            {
                Namespace = "AWS/Lambda",
                MetricName = "Errors",
                Statistic = "Sum",
                Period = Duration.Minutes(5)
            }),
            Threshold = 1,
            EvaluationPeriods = 1,
            ComparisonOperator = ComparisonOperator.GREATER_THAN_OR_EQUAL_TO_THRESHOLD,
            TreatMissingData = TreatMissingData.NOT_BREACHING
        });
        ingestionFailureAlarm.AddAlarmAction(new SnsAction(CriticalAlarmsTopic));

        // ─── X-Ray Tracing Configuration ─────────────────────────────────────────
        // X-Ray sampling rule for HSECockpit services.
        // Actual X-Ray enablement occurs on API Gateway and ECS task definitions.
        _ = new CfnSamplingRule(this, "XRaySamplingRule", new CfnSamplingRuleProps
        {
            SamplingRule = new CfnSamplingRule.SamplingRuleProperty
            {
                RuleName = "HSECockpit-Tracing",
                Priority = 1000,
                FixedRate = 0.05,
                ReservoirSize = 1,
                ServiceName = "HSECockpit",
                ServiceType = "*",
                Host = "*",
                HttpMethod = "*",
                UrlPath = "*",
                ResourceArn = "*",
                Version = 1
            }
        });

        // ─── CloudFormation Outputs ──────────────────────────────────────────────
        _ = new CfnOutput(this, "CriticalAlarmsTopicArn", new CfnOutputProps
        {
            Value = CriticalAlarmsTopic.TopicArn,
            Description = "ARN of the SNS topic for critical alarm notifications",
            ExportName = "HSECockpit-CriticalAlarms-TopicArn"
        });

        _ = new CfnOutput(this, "ApiLogGroupName", new CfnOutputProps
        {
            Value = ApiLogGroup.LogGroupName,
            Description = "CloudWatch Log Group name for the ECS .NET API",
            ExportName = "HSECockpit-ApiLogGroup-Name"
        });

        _ = new CfnOutput(this, "BarrierIngestionLogGroupName", new CfnOutputProps
        {
            Value = BarrierIngestionLogGroup.LogGroupName,
            Description = "CloudWatch Log Group name for barrier ingestion Lambda",
            ExportName = "HSECockpit-BarrierIngestionLogGroup-Name"
        });

        _ = new CfnOutput(this, "IncidentIngestionLogGroupName", new CfnOutputProps
        {
            Value = IncidentIngestionLogGroup.LogGroupName,
            Description = "CloudWatch Log Group name for incident ingestion Lambda",
            ExportName = "HSECockpit-IncidentIngestionLogGroup-Name"
        });

        _ = new CfnOutput(this, "MaintenanceIngestionLogGroupName", new CfnOutputProps
        {
            Value = MaintenanceIngestionLogGroup.LogGroupName,
            Description = "CloudWatch Log Group name for maintenance ingestion Lambda",
            ExportName = "HSECockpit-MaintenanceIngestionLogGroup-Name"
        });
    }
}
