using D4HSE.Core.Interfaces;

namespace D4HSE.Services.Services;

/// <summary>
/// Business logic layer for executive cockpit KPI aggregation.
/// Computes portfolio-level metrics across all sites by orchestrating
/// calls to barrier, incident, risk, and compliance data sources.
/// </summary>
public class ExecutiveService
{
    private readonly IBarrierRepository _barrierRepository;
    private readonly IIncidentRepository _incidentRepository;
    private readonly IRiskItemRepository _riskItemRepository;
    private readonly ISiteRepository _siteRepository;
    private readonly IComplianceRepository _complianceRepository;

    public ExecutiveService(
        IBarrierRepository barrierRepository,
        IIncidentRepository incidentRepository,
        IRiskItemRepository riskItemRepository,
        ISiteRepository siteRepository,
        IComplianceRepository complianceRepository)
    {
        _barrierRepository = barrierRepository;
        _incidentRepository = incidentRepository;
        _riskItemRepository = riskItemRepository;
        _siteRepository = siteRepository;
        _complianceRepository = complianceRepository;
    }

    /// <summary>
    /// Computes executive-level KPIs:
    /// - Barrier Health Score: portfolio-wide weighted percentage of GREEN barriers
    /// - Open Critical Risks: count of CRITICAL + OPEN risk items across all sites
    /// - Incident Count MTD: total incidents from first of current month to today
    /// - Compliance Rate: percentage of sites with COMPLIANT status in latest assessment period
    /// </summary>
    public async Task<ExecutiveKPIs> GetExecutiveKPIsAsync(CancellationToken ct)
    {
        var sites = await _siteRepository.GetAllSitesAsync(ct);
        var hasDataQualityIssues = false;

        // Compute Barrier Health Score: portfolio-wide percentage of GREEN barriers
        var barrierHealthScore = await ComputeBarrierHealthScoreAsync(sites, ct);
        if (barrierHealthScore.hasQualityIssues)
        {
            hasDataQualityIssues = true;
        }

        // Compute Open Critical Risks: count across all sites
        var openCriticalRisks = await ComputeOpenCriticalRisksAsync(sites, ct);

        // Compute Incident Count MTD: from first of current month to today
        var incidentCountMTD = await ComputeIncidentCountMTDAsync(ct);

        // Compute Compliance Rate: percentage of sites with COMPLIANT status
        var complianceRate = await ComputeComplianceRateAsync(ct);

        return new ExecutiveKPIs
        {
            BarrierHealthScore = barrierHealthScore.score,
            OpenCriticalRisks = openCriticalRisks,
            IncidentCountMTD = incidentCountMTD,
            ComplianceRate = complianceRate,
            DataQualityStatus = hasDataQualityIssues ? "PARTIAL" : "VALID"
        };
    }

    private async Task<(double score, bool hasQualityIssues)> ComputeBarrierHealthScoreAsync(
        IReadOnlyList<SiteSummary> sites, CancellationToken ct)
    {
        var totalBarriers = 0;
        var greenBarriers = 0;
        var hasQualityIssues = false;

        foreach (var site in sites)
        {
            var barriers = await _barrierRepository.GetBarriersByContextAsync(site.SiteId, null, ct);

            foreach (var barrier in barriers)
            {
                totalBarriers++;

                if (string.Equals(barrier.CurrentRagStatus, "GREEN", StringComparison.OrdinalIgnoreCase))
                {
                    greenBarriers++;
                }

                if (!string.Equals(barrier.DataQualityStatus, "VALID", StringComparison.OrdinalIgnoreCase))
                {
                    hasQualityIssues = true;
                }
            }
        }

        var score = totalBarriers > 0
            ? Math.Round((double)greenBarriers / totalBarriers * 100, 2)
            : 0.0;

        return (score, hasQualityIssues);
    }

    private async Task<int> ComputeOpenCriticalRisksAsync(
        IReadOnlyList<SiteSummary> sites, CancellationToken ct)
    {
        var totalCritical = 0;

        foreach (var site in sites)
        {
            var counts = await _riskItemRepository.GetOpenRiskCountsAsync(site.SiteId, ct);
            totalCritical += counts.OpenCriticalCount;
        }

        return totalCritical;
    }

    private async Task<int> ComputeIncidentCountMTDAsync(CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var firstOfMonth = new DateOnly(today.Year, today.Month, 1);

        // Get incident summary across all sites (null siteId = all sites)
        var summary = await _incidentRepository.GetIncidentSummaryAsync(null, null, firstOfMonth, today, ct);

        return summary.TotalCount;
    }

    private async Task<double> ComputeComplianceRateAsync(CancellationToken ct)
    {
        var complianceSummary = await _complianceRepository.GetLatestComplianceSummaryAsync(ct);

        var totalSites = complianceSummary.TotalSites;
        var compliantSites = complianceSummary.CompliantCount;

        if (totalSites == 0)
        {
            return 0.0;
        }

        return Math.Round((double)compliantSites / totalSites * 100, 2);
    }
}
