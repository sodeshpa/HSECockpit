using System.Text.Json;
using D4HSE.Core.Interfaces;

namespace D4HSE.Services.Services;

/// <summary>
/// Computes a composite site risk score using configurable weights from Parameter Store.
/// The score is a weighted sum of four factors: incident severity, near-miss frequency,
/// open risk count, and barrier health.
/// </summary>
public class RiskScoreService
{
    private readonly IIncidentRepository _incidentRepository;
    private readonly IBarrierRepository _barrierRepository;
    private readonly IRiskItemRepository _riskItemRepository;
    private readonly ISiteRepository _siteRepository;
    private readonly IParameterStoreService _parameterStoreService;

    /// <summary>
    /// Default weights used when Parameter Store is unavailable.
    /// </summary>
    private static readonly RiskWeights DefaultWeights = new()
    {
        IncidentSeverity = 0.25,
        NearMissFrequency = 0.25,
        OpenRiskCount = 0.25,
        BarrierHealth = 0.25
    };

    /// <summary>
    /// Default risk banding thresholds used when Parameter Store is unavailable.
    /// </summary>
    private static readonly RiskBanding DefaultBanding = new()
    {
        Low = 25,
        Medium = 50,
        High = 75
    };

    /// <summary>
    /// Baseline count for near-miss normalization (monthly expected near-miss count).
    /// </summary>
    private const int NearMissBaseline = 5;

    /// <summary>
    /// Threshold for normalizing HIGH+CRITICAL incident count.
    /// At this count or above, the incident factor is fully saturated (1.0).
    /// </summary>
    private const int IncidentSaturationThreshold = 10;

    /// <summary>
    /// Threshold for normalizing open risk count.
    /// At this count or above, the open risk factor is fully saturated (1.0).
    /// </summary>
    private const int OpenRiskSaturationThreshold = 10;

    public RiskScoreService(
        IIncidentRepository incidentRepository,
        IBarrierRepository barrierRepository,
        IRiskItemRepository riskItemRepository,
        ISiteRepository siteRepository,
        IParameterStoreService parameterStoreService)
    {
        _incidentRepository = incidentRepository;
        _barrierRepository = barrierRepository;
        _riskItemRepository = riskItemRepository;
        _siteRepository = siteRepository;
        _parameterStoreService = parameterStoreService;
    }

    /// <summary>
    /// Calculates the composite site risk score for the given site.
    /// Loads weights from Parameter Store at /hse/risk/weights (cached by IParameterStoreService).
    /// </summary>
    public async Task<SiteRiskScore> GetSiteRiskScoreAsync(Guid siteId, CancellationToken ct)
    {
        var weights = await LoadWeightsAsync(ct);
        var banding = await LoadBandingAsync(ct);

        var hasDataQualityIssues = false;

        // Compute each factor in parallel
        var incidentTask = ComputeIncidentFactorAsync(siteId, weights.IncidentSeverity, ct);
        var nearMissTask = ComputeNearMissFactorAsync(siteId, weights.NearMissFrequency, ct);
        var openRiskTask = ComputeOpenRiskFactorAsync(siteId, weights.OpenRiskCount, ct);
        var barrierTask = ComputeBarrierHealthFactorAsync(siteId, weights.BarrierHealth, ct);

        await Task.WhenAll(incidentTask, nearMissTask, openRiskTask, barrierTask);

        var incidentFactor = await incidentTask;
        var nearMissFactor = await nearMissTask;
        var openRiskFactor = await openRiskTask;
        var barrierFactor = await barrierTask;

        // Check data quality: if any factor has no data, mark as partial
        if (incidentFactor.dataQualityIssue || nearMissFactor.dataQualityIssue ||
            openRiskFactor.dataQualityIssue || barrierFactor.dataQualityIssue)
        {
            hasDataQualityIssues = true;
        }

        // Composite formula: score = Σ(factor_score × weight) × 100
        var compositeScore =
            (incidentFactor.factor.WeightedScore +
             nearMissFactor.factor.WeightedScore +
             openRiskFactor.factor.WeightedScore +
             barrierFactor.factor.WeightedScore) * 100;

        // Clamp score to [0, 100]
        compositeScore = Math.Clamp(compositeScore, 0, 100);

        var riskBand = DetermineRiskBand(compositeScore, banding);

        return new SiteRiskScore
        {
            SiteId = siteId,
            Score = Math.Round(compositeScore, 2),
            RiskBand = riskBand,
            IncidentFactor = incidentFactor.factor,
            NearMissFactor = nearMissFactor.factor,
            OpenRiskFactor = openRiskFactor.factor,
            BarrierFactor = barrierFactor.factor,
            DataQualityStatus = hasDataQualityIssues ? "PARTIAL" : "VALID"
        };
    }

    /// <summary>
    /// Returns a risk heatmap with one row per site, sorted by score descending (highest risk first).
    /// Each row contains the site's composite risk score and its risk band.
    /// </summary>
    /// <param name="periodDays">Number of days to consider for the risk calculation (reserved for future use).</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<IReadOnlyList<SiteRiskHeatmapRow>> GetRiskHeatmapAsync(int periodDays, CancellationToken ct)
    {
        var sites = await _siteRepository.GetAllSitesAsync(ct);

        var rows = new List<SiteRiskHeatmapRow>(sites.Count);

        foreach (var site in sites)
        {
            var riskScore = await GetSiteRiskScoreAsync(site.SiteId, ct);

            rows.Add(new SiteRiskHeatmapRow
            {
                SiteId = site.SiteId,
                SiteName = site.SiteName,
                Score = riskScore.Score,
                RiskBand = riskScore.RiskBand
            });
        }

        return rows.OrderByDescending(r => r.Score).ToList();
    }

    private async Task<(IncidentSeverityFactor factor, bool dataQualityIssue)> ComputeIncidentFactorAsync(
        Guid siteId, double weight, CancellationToken ct)
    {
        // Look at HIGH+CRITICAL incidents in the last 30 days
        var toDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var fromDate = toDate.AddDays(-30);

        var summary = await _incidentRepository.GetIncidentSummaryAsync(siteId, null, fromDate, toDate, ct);

        var highCriticalCount = summary.HighCount + summary.CriticalCount;

        // Normalize: saturate at threshold
        var normalizedScore = Math.Min((double)highCriticalCount / IncidentSaturationThreshold, 1.0);

        var factor = new IncidentSeverityFactor
        {
            HighIncidentCount = summary.HighCount,
            CriticalIncidentCount = summary.CriticalCount,
            NormalizedScore = Math.Round(normalizedScore, 4),
            Weight = weight,
            WeightedScore = Math.Round(normalizedScore * weight, 4)
        };

        // No data quality issue for incidents (we get a valid count either way)
        return (factor, false);
    }

    private async Task<(NearMissFrequencyFactor factor, bool dataQualityIssue)> ComputeNearMissFactorAsync(
        Guid siteId, double weight, CancellationToken ct)
    {
        var toDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var fromDate = toDate.AddDays(-30);

        var summary = await _incidentRepository.GetNearMissSummaryAsync(siteId, null, fromDate, toDate, ct);

        var currentCount = summary.TotalCount;
        var baseline = NearMissBaseline;

        // Normalize: ratio of current to baseline, capped at 1.0
        var normalizedScore = baseline > 0
            ? Math.Min((double)currentCount / baseline, 1.0)
            : 0.0;

        var factor = new NearMissFrequencyFactor
        {
            CurrentPeriodCount = currentCount,
            BaselineCount = baseline,
            NormalizedScore = Math.Round(normalizedScore, 4),
            Weight = weight,
            WeightedScore = Math.Round(normalizedScore * weight, 4)
        };

        return (factor, false);
    }

    private async Task<(OpenRiskFactor factor, bool dataQualityIssue)> ComputeOpenRiskFactorAsync(
        Guid siteId, double weight, CancellationToken ct)
    {
        var counts = await _riskItemRepository.GetOpenRiskCountsAsync(siteId, ct);

        var openCriticalAndHigh = counts.OpenCriticalCount + counts.OpenHighCount;

        // Normalize: saturate at threshold
        var normalizedScore = Math.Min((double)openCriticalAndHigh / OpenRiskSaturationThreshold, 1.0);

        var factor = new OpenRiskFactor
        {
            OpenCriticalCount = counts.OpenCriticalCount,
            OpenHighCount = counts.OpenHighCount,
            NormalizedScore = Math.Round(normalizedScore, 4),
            Weight = weight,
            WeightedScore = Math.Round(normalizedScore * weight, 4)
        };

        return (factor, false);
    }

    private async Task<(BarrierHealthFactor factor, bool dataQualityIssue)> ComputeBarrierHealthFactorAsync(
        Guid siteId, double weight, CancellationToken ct)
    {
        var barriers = await _barrierRepository.GetBarriersByContextAsync(siteId, null, ct);

        var totalBarriers = barriers.Count;
        var nonGreenBarriers = barriers.Count(b =>
            b.CurrentRagStatus != null &&
            !b.CurrentRagStatus.Equals("GREEN", StringComparison.OrdinalIgnoreCase));

        // Normalize: percentage of barriers not green
        var normalizedScore = totalBarriers > 0
            ? (double)nonGreenBarriers / totalBarriers
            : 0.0;

        // Check data quality: if any barrier has quality issues, flag it
        var hasQualityIssues = barriers.Any(b =>
            !b.DataQualityStatus.Equals("VALID", StringComparison.OrdinalIgnoreCase));

        var factor = new BarrierHealthFactor
        {
            TotalBarriers = totalBarriers,
            NonGreenBarriers = nonGreenBarriers,
            NormalizedScore = Math.Round(normalizedScore, 4),
            Weight = weight,
            WeightedScore = Math.Round(normalizedScore * weight, 4)
        };

        return (factor, hasQualityIssues);
    }

    private async Task<RiskWeights> LoadWeightsAsync(CancellationToken ct)
    {
        var json = await _parameterStoreService.GetParameterAsync("/hse/risk/weights", ct);

        if (string.IsNullOrEmpty(json))
        {
            return DefaultWeights;
        }

        try
        {
            var weights = JsonSerializer.Deserialize<RiskWeights>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return weights ?? DefaultWeights;
        }
        catch (JsonException)
        {
            return DefaultWeights;
        }
    }

    private async Task<RiskBanding> LoadBandingAsync(CancellationToken ct)
    {
        var json = await _parameterStoreService.GetParameterAsync("/hse/risk/banding", ct);

        if (string.IsNullOrEmpty(json))
        {
            return DefaultBanding;
        }

        try
        {
            var banding = JsonSerializer.Deserialize<RiskBanding>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return banding ?? DefaultBanding;
        }
        catch (JsonException)
        {
            return DefaultBanding;
        }
    }

    private static string DetermineRiskBand(double score, RiskBanding banding)
    {
        if (score >= banding.High)
            return "Critical";
        if (score >= banding.Medium)
            return "High";
        if (score >= banding.Low)
            return "Medium";
        return "Low";
    }

    /// <summary>
    /// Configurable risk weights loaded from Parameter Store at /hse/risk/weights.
    /// </summary>
    private class RiskWeights
    {
        public double IncidentSeverity { get; set; }
        public double NearMissFrequency { get; set; }
        public double OpenRiskCount { get; set; }
        public double BarrierHealth { get; set; }
    }

    /// <summary>
    /// Configurable risk banding thresholds loaded from Parameter Store at /hse/risk/banding.
    /// Scores below Low are "Low", between Low and Medium are "Medium", etc.
    /// </summary>
    private class RiskBanding
    {
        public double Low { get; set; }
        public double Medium { get; set; }
        public double High { get; set; }
    }
}
