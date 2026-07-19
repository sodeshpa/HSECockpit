using D4HSE.Core.Interfaces;
using D4HSE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace D4HSE.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IComplianceRepository.
/// Queries ComplianceStatuses table for the latest assessment per site.
/// </summary>
public class ComplianceRepository : IComplianceRepository
{
    private readonly HseCockpitDbContext _dbContext;

    public ComplianceRepository(HseCockpitDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<ComplianceSummary> GetLatestComplianceSummaryAsync(CancellationToken ct)
    {
        // Get the latest compliance assessment per site (most recent period_end)
        var latestPerSite = await _dbContext.ComplianceStatuses
            .AsNoTracking()
            .GroupBy(c => c.SiteId)
            .Select(g => g.OrderByDescending(c => c.PeriodEnd).First())
            .ToListAsync(ct);

        var compliantCount = latestPerSite.Count(c =>
            c.Status.Equals("COMPLIANT", StringComparison.OrdinalIgnoreCase));
        var nonCompliantCount = latestPerSite.Count(c =>
            c.Status.Equals("NON_COMPLIANT", StringComparison.OrdinalIgnoreCase));
        var unknownCount = latestPerSite.Count(c =>
            c.Status.Equals("UNKNOWN", StringComparison.OrdinalIgnoreCase));

        return new ComplianceSummary
        {
            TotalSites = latestPerSite.Count,
            CompliantCount = compliantCount,
            NonCompliantCount = nonCompliantCount,
            UnknownCount = unknownCount
        };
    }
}
