namespace D4HSE.Core.Interfaces;

/// <summary>
/// Composite site risk score (0–100) with contributing factor breakdown.
/// Score is derived from incident severity, near-miss frequency, open risk count, and barrier health.
/// </summary>
public class SiteRiskScore
{
    public Guid SiteId { get; set; }

    /// <summary>
    /// Composite risk score on a 0–100 scale. Higher values indicate higher risk.
    /// </summary>
    public double Score { get; set; }

    /// <summary>
    /// Risk band derived from configurable thresholds: Low, Medium, High, Critical.
    /// </summary>
    public string RiskBand { get; set; } = string.Empty;

    public IncidentSeverityFactor IncidentFactor { get; set; } = new();
    public NearMissFrequencyFactor NearMissFactor { get; set; } = new();
    public OpenRiskFactor OpenRiskFactor { get; set; } = new();
    public BarrierHealthFactor BarrierFactor { get; set; } = new();

    /// <summary>
    /// VALID when all contributing data categories have good quality;
    /// PARTIAL when any contributing data category has quality issues.
    /// </summary>
    public string DataQualityStatus { get; set; } = "VALID";
}
