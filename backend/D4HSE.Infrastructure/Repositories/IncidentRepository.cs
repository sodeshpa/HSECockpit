using D4HSE.Core.Interfaces;
using D4HSE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace D4HSE.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IIncidentRepository.
/// Queries Incidents and NearMisses tables with optional site/asset filtering and date range.
/// </summary>
public class IncidentRepository : IIncidentRepository
{
    private readonly HseCockpitDbContext _dbContext;

    public IncidentRepository(HseCockpitDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IncidentSummary> GetIncidentSummaryAsync(
        Guid? siteId, Guid? assetId, DateOnly fromDate, DateOnly toDate, CancellationToken ct)
    {
        var query = _dbContext.Incidents
            .AsNoTracking()
            .Where(i => i.IncidentDate >= fromDate && i.IncidentDate <= toDate);

        if (siteId.HasValue)
        {
            query = query.Where(i => i.SiteId == siteId.Value);
        }

        if (assetId.HasValue)
        {
            query = query.Where(i => i.AssetId == assetId.Value);
        }

        var severityCounts = await query
            .GroupBy(i => i.Severity)
            .Select(g => new { Severity = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var summary = new IncidentSummary
        {
            PeriodStart = fromDate,
            PeriodEnd = toDate,
            TotalCount = severityCounts.Sum(s => s.Count),
            LowCount = severityCounts
                .Where(s => s.Severity.Equals("LOW", StringComparison.OrdinalIgnoreCase))
                .Sum(s => s.Count),
            MediumCount = severityCounts
                .Where(s => s.Severity.Equals("MEDIUM", StringComparison.OrdinalIgnoreCase))
                .Sum(s => s.Count),
            HighCount = severityCounts
                .Where(s => s.Severity.Equals("HIGH", StringComparison.OrdinalIgnoreCase))
                .Sum(s => s.Count),
            CriticalCount = severityCounts
                .Where(s => s.Severity.Equals("CRITICAL", StringComparison.OrdinalIgnoreCase))
                .Sum(s => s.Count)
        };

        return summary;
    }

    /// <inheritdoc />
    public async Task<NearMissSummary> GetNearMissSummaryAsync(
        Guid? siteId, Guid? assetId, DateOnly fromDate, DateOnly toDate, CancellationToken ct)
    {
        // Current period query
        var currentQuery = _dbContext.NearMisses
            .AsNoTracking()
            .Where(n => n.EventDate >= fromDate && n.EventDate <= toDate);

        if (siteId.HasValue)
        {
            currentQuery = currentQuery.Where(n => n.SiteId == siteId.Value);
        }

        if (assetId.HasValue)
        {
            currentQuery = currentQuery.Where(n => n.AssetId == assetId.Value);
        }

        var currentCount = await currentQuery.CountAsync(ct);

        // Calculate the prior period of the same length for trend comparison
        var periodLength = toDate.DayNumber - fromDate.DayNumber;
        var priorFromDate = fromDate.AddDays(-(periodLength + 1));
        var priorToDate = fromDate.AddDays(-1);

        var priorQuery = _dbContext.NearMisses
            .AsNoTracking()
            .Where(n => n.EventDate >= priorFromDate && n.EventDate <= priorToDate);

        if (siteId.HasValue)
        {
            priorQuery = priorQuery.Where(n => n.SiteId == siteId.Value);
        }

        if (assetId.HasValue)
        {
            priorQuery = priorQuery.Where(n => n.AssetId == assetId.Value);
        }

        var priorCount = await priorQuery.CountAsync(ct);

        // Determine trend direction
        string trendDirection;
        if (currentCount > priorCount)
        {
            trendDirection = "UP";
        }
        else if (currentCount < priorCount)
        {
            trendDirection = "DOWN";
        }
        else
        {
            trendDirection = "STABLE";
        }

        return new NearMissSummary
        {
            TotalCount = currentCount,
            PriorPeriodCount = priorCount,
            TrendDirection = trendDirection,
            PeriodStart = fromDate,
            PeriodEnd = toDate
        };
    }
}
