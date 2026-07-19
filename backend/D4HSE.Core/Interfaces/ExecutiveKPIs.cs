namespace D4HSE.Core.Interfaces;

/// <summary>
/// Portfolio-level executive KPI snapshot containing barrier health,
/// open critical risks, incident count month-to-date, and compliance rate.
/// </summary>
public class ExecutiveKPIs
{
    /// <summary>
    /// Portfolio-wide weighted percentage of GREEN barriers (0–100%).
    /// </summary>
    public double BarrierHealthScore { get; set; }

    /// <summary>
    /// Count of risk items with severity = CRITICAL and status = OPEN.
    /// </summary>
    public int OpenCriticalRisks { get; set; }

    /// <summary>
    /// Total incidents from the first of the current month to today.
    /// </summary>
    public int IncidentCountMTD { get; set; }

    /// <summary>
    /// Percentage of sites with status = COMPLIANT in the latest assessment period (0–100%).
    /// </summary>
    public double ComplianceRate { get; set; }

    /// <summary>
    /// Indicates overall data quality: VALID when all KPI inputs are clean,
    /// PARTIAL when any contributing data has quality flags.
    /// </summary>
    public string DataQualityStatus { get; set; } = "VALID";
}
