using D4HSE.Core.Interfaces;
using D4HSE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace D4HSE.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IRiskItemRepository.
/// Queries RiskItems table for open risk counts by severity.
/// </summary>
public class RiskItemRepository : IRiskItemRepository
{
    private readonly HseCockpitDbContext _dbContext;

    public RiskItemRepository(HseCockpitDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<RiskItemCounts> GetOpenRiskCountsAsync(Guid siteId, CancellationToken ct)
    {
        var severityCounts = await _dbContext.RiskItems
            .AsNoTracking()
            .Where(r => r.SiteId == siteId && r.Status == "OPEN")
            .GroupBy(r => r.Severity)
            .Select(g => new { Severity = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        return new RiskItemCounts
        {
            OpenCriticalCount = severityCounts
                .Where(s => s.Severity.Equals("CRITICAL", StringComparison.OrdinalIgnoreCase))
                .Sum(s => s.Count),
            OpenHighCount = severityCounts
                .Where(s => s.Severity.Equals("HIGH", StringComparison.OrdinalIgnoreCase))
                .Sum(s => s.Count),
            OpenMediumCount = severityCounts
                .Where(s => s.Severity.Equals("MEDIUM", StringComparison.OrdinalIgnoreCase))
                .Sum(s => s.Count),
            OpenLowCount = severityCounts
                .Where(s => s.Severity.Equals("LOW", StringComparison.OrdinalIgnoreCase))
                .Sum(s => s.Count),
            TotalOpenCount = severityCounts.Sum(s => s.Count)
        };
    }
}
