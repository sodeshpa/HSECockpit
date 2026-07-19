namespace D4HSE.Api.Models;

/// <summary>
/// Response DTO for a single row in the risk heatmap.
/// Maps a site to its composite risk score and banding.
/// </summary>
public class SiteRiskHeatmapResponse
{
    public Guid SiteId { get; set; }
    public string SiteName { get; set; } = string.Empty;
    public double Score { get; set; }
    public string RiskBand { get; set; } = string.Empty;
}
