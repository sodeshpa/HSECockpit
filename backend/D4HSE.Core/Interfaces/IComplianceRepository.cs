namespace D4HSE.Core.Interfaces;

/// <summary>
/// Repository interface for compliance status read operations against the relational store.
/// </summary>
public interface IComplianceRepository
{
    /// <summary>
    /// Returns a summary of the latest compliance assessment across all sites,
    /// including counts of compliant, non-compliant, and unknown statuses.
    /// Only the most recent assessment per site (by period_end) is considered.
    /// </summary>
    Task<ComplianceSummary> GetLatestComplianceSummaryAsync(CancellationToken ct);
}

/// <summary>
/// Aggregated compliance summary across all assessed sites.
/// </summary>
public class ComplianceSummary
{
    /// <summary>
    /// Total number of sites with a compliance assessment.
    /// </summary>
    public int TotalSites { get; set; }

    /// <summary>
    /// Number of sites with status = COMPLIANT in their latest assessment.
    /// </summary>
    public int CompliantCount { get; set; }

    /// <summary>
    /// Number of sites with status = NON_COMPLIANT in their latest assessment.
    /// </summary>
    public int NonCompliantCount { get; set; }

    /// <summary>
    /// Number of sites with status = UNKNOWN in their latest assessment.
    /// </summary>
    public int UnknownCount { get; set; }
}
