namespace D4HSE.Api.Models;

/// <summary>
/// Response DTO for the executive KPIs endpoint.
/// Contains portfolio-level KPI values for the executive cockpit.
/// </summary>
public class ExecutiveKPIsResponse
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
