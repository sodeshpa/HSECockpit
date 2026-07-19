namespace D4HSE.Core.Interfaces;

/// <summary>
/// Contributing factor for incident severity in the site risk score calculation.
/// Normalized based on count of HIGH+CRITICAL incidents in last 30 days.
/// </summary>
public class IncidentSeverityFactor
{
    public int HighIncidentCount { get; set; }
    public int CriticalIncidentCount { get; set; }
    public double NormalizedScore { get; set; } // 0.0 to 1.0
    public double Weight { get; set; }
    public double WeightedScore { get; set; }
}

/// <summary>
/// Contributing factor for near-miss frequency in the site risk score calculation.
/// Normalized based on count vs baseline.
/// </summary>
public class NearMissFrequencyFactor
{
    public int CurrentPeriodCount { get; set; }
    public int BaselineCount { get; set; }
    public double NormalizedScore { get; set; } // 0.0 to 1.0
    public double Weight { get; set; }
    public double WeightedScore { get; set; }
}

/// <summary>
/// Contributing factor for open risk count in the site risk score calculation.
/// Count of OPEN + CRITICAL risk items.
/// </summary>
public class OpenRiskFactor
{
    public int OpenCriticalCount { get; set; }
    public int OpenHighCount { get; set; }
    public double NormalizedScore { get; set; } // 0.0 to 1.0
    public double Weight { get; set; }
    public double WeightedScore { get; set; }
}

/// <summary>
/// Contributing factor for barrier health in the site risk score calculation.
/// Percentage of barriers NOT GREEN.
/// </summary>
public class BarrierHealthFactor
{
    public int TotalBarriers { get; set; }
    public int NonGreenBarriers { get; set; }
    public double NormalizedScore { get; set; } // 0.0 to 1.0
    public double Weight { get; set; }
    public double WeightedScore { get; set; }
}
