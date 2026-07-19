namespace D4HSE.Core.Interfaces;

/// <summary>
/// Represents a single row in the site-level risk heatmap.
/// Each row maps a site to its composite risk score and banding.
/// </summary>
public class SiteRiskHeatmapRow
{
    public Guid SiteId { get; set; }
    public string SiteName { get; set; } = string.Empty;
    public double Score { get; set; }

    /// <summary>
    /// Risk band: Low, Medium, High, or Critical.
    /// </summary>
    public string RiskBand { get; set; } = string.Empty;
}
