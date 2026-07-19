namespace D4HSE.Api.Models;

/// <summary>
/// Response DTO for the site risk score endpoint.
/// Returns the composite score, risk band, contributing factors, and data quality status.
/// </summary>
public class SiteRiskScoreResponse
{
    /// <summary>
    /// Composite risk score on a 0–100 scale. Higher values indicate higher risk.
    /// </summary>
    public double Score { get; set; }

    /// <summary>
    /// Risk band derived from configurable thresholds: Low, Medium, High, Critical.
    /// </summary>
    public string RiskBand { get; set; } = string.Empty;

    /// <summary>
    /// Contributing factor breakdown for the risk score.
    /// </summary>
    public RiskFactorsResponse Factors { get; set; } = new();

    /// <summary>
    /// VALID when all contributing data categories have good quality;
    /// PARTIAL when any contributing data category has quality issues.
    /// </summary>
    public string DataQualityStatus { get; set; } = "VALID";
}

/// <summary>
/// Breakdown of contributing factors for the site risk score.
/// </summary>
public class RiskFactorsResponse
{
    public IncidentSeverityFactorResponse IncidentFactor { get; set; } = new();
    public NearMissFrequencyFactorResponse NearMissFactor { get; set; } = new();
    public OpenRiskFactorResponse OpenRiskFactor { get; set; } = new();
    public BarrierHealthFactorResponse BarrierFactor { get; set; } = new();
}

public class IncidentSeverityFactorResponse
{
    public int HighIncidentCount { get; set; }
    public int CriticalIncidentCount { get; set; }
    public double NormalizedScore { get; set; }
    public double Weight { get; set; }
    public double WeightedScore { get; set; }
}

public class NearMissFrequencyFactorResponse
{
    public int CurrentPeriodCount { get; set; }
    public int BaselineCount { get; set; }
    public double NormalizedScore { get; set; }
    public double Weight { get; set; }
    public double WeightedScore { get; set; }
}

public class OpenRiskFactorResponse
{
    public int OpenCriticalCount { get; set; }
    public int OpenHighCount { get; set; }
    public double NormalizedScore { get; set; }
    public double Weight { get; set; }
    public double WeightedScore { get; set; }
}

public class BarrierHealthFactorResponse
{
    public int TotalBarriers { get; set; }
    public int NonGreenBarriers { get; set; }
    public double NormalizedScore { get; set; }
    public double Weight { get; set; }
    public double WeightedScore { get; set; }
}
