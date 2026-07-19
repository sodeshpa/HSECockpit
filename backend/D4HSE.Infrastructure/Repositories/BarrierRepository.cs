using D4HSE.Core.Interfaces;
using D4HSE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace D4HSE.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IBarrierRepository.
/// Queries CriticalBarriers joined with their latest BarrierHealthObservation to derive RAG status.
/// </summary>
public class BarrierRepository : IBarrierRepository
{
    private readonly HseCockpitDbContext _dbContext;

    public BarrierRepository(HseCockpitDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<BarrierWithStatus>> GetBarriersByContextAsync(
        Guid? siteId, Guid? assetId, CancellationToken ct)
    {
        var query = _dbContext.CriticalBarriers
            .AsNoTracking()
            .Where(b => b.IsActive);

        if (siteId.HasValue)
        {
            query = query.Where(b => b.SiteId == siteId.Value);
        }

        if (assetId.HasValue)
        {
            query = query.Where(b => b.AssetId == assetId.Value);
        }

        // For each barrier, get the latest observation date and resolve RAG status.
        // When multiple observations exist on the same (latest) day with different statuses,
        // use the highest severity: RED > AMBER > GREEN (conservative conflict rule).
        var result = await query
            .Select(b => new
            {
                b.BarrierId,
                b.BarrierName,
                b.BarrierType,
                b.SiteId,
                b.AssetId,
                b.CriticalityRank,
                LatestObservationDate = b.HealthObservations
                    .OrderByDescending(o => o.ObservedAt)
                    .Select(o => (DateOnly?)o.ObservedAt)
                    .FirstOrDefault(),
                // Get all observations on the latest date to resolve conflicts
                LatestObservations = b.HealthObservations
                    .Where(o => o.ObservedAt == b.HealthObservations
                        .OrderByDescending(o2 => o2.ObservedAt)
                        .Select(o2 => o2.ObservedAt)
                        .FirstOrDefault())
                    .Select(o => new { o.RagStatus, o.DataQualityStatus })
                    .ToList()
            })
            .OrderBy(b => b.CriticalityRank)
            .ToListAsync(ct);

        return result.Select(b =>
        {
            string? currentRagStatus = null;
            string dataQualityStatus = "VALID";

            if (b.LatestObservations.Count > 0)
            {
                // Apply conservative conflict rule: highest severity wins (RED > AMBER > GREEN)
                currentRagStatus = ResolveRagStatus(b.LatestObservations.Select(o => o.RagStatus));

                // If multiple different statuses exist on the same day, mark as CONFLICT
                var distinctStatuses = b.LatestObservations
                    .Select(o => o.RagStatus)
                    .Distinct()
                    .ToList();

                if (distinctStatuses.Count > 1)
                {
                    dataQualityStatus = "CONFLICT";
                }
                else
                {
                    // Aggregate from observation quality statuses (worst wins)
                    dataQualityStatus = ResolveDataQualityStatus(
                        b.LatestObservations.Select(o => o.DataQualityStatus));
                }
            }

            return new BarrierWithStatus
            {
                BarrierId = b.BarrierId,
                BarrierName = b.BarrierName,
                BarrierType = b.BarrierType,
                SiteId = b.SiteId,
                AssetId = b.AssetId,
                CriticalityRank = b.CriticalityRank,
                CurrentRagStatus = currentRagStatus,
                LastAssessedDate = b.LatestObservationDate,
                DataQualityStatus = dataQualityStatus
            };
        }).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<BarrierTrendPoint>> GetBarrierTrendAsync(
        Guid barrierId, int periodDays, CancellationToken ct)
    {
        var cutoffDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-periodDays));

        return await _dbContext.BarrierHealthObservations
            .AsNoTracking()
            .Where(o => o.BarrierId == barrierId && o.ObservedAt >= cutoffDate)
            .OrderBy(o => o.ObservedAt)
            .Select(o => new BarrierTrendPoint
            {
                ObservedAt = o.ObservedAt,
                RagStatus = o.RagStatus,
                ConditionScore = o.ConditionScore,
                DataQualityStatus = o.DataQualityStatus
            })
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<BarrierWithStatus>> GetDegradedBarriersAsync(
        Guid? siteId, CancellationToken ct)
    {
        var query = _dbContext.CriticalBarriers
            .AsNoTracking()
            .Where(b => b.IsActive);

        if (siteId.HasValue)
        {
            query = query.Where(b => b.SiteId == siteId.Value);
        }

        // For each barrier, get the two latest distinct observation dates and their RAG statuses.
        var barriersWithRecentObservations = await query
            .Select(b => new
            {
                b.BarrierId,
                b.BarrierName,
                b.BarrierType,
                b.SiteId,
                b.AssetId,
                b.CriticalityRank,
                // Get the two most recent observations ordered by date descending
                TopTwoObservations = b.HealthObservations
                    .OrderByDescending(o => o.ObservedAt)
                    .Take(2)
                    .Select(o => new { o.ObservedAt, o.RagStatus, o.DataQualityStatus })
                    .ToList()
            })
            .ToListAsync(ct);

        // Filter to barriers where the latest RAG is worse than the prior
        var degraded = new List<BarrierWithStatus>();

        foreach (var b in barriersWithRecentObservations)
        {
            if (b.TopTwoObservations.Count < 2)
                continue;

            var latestRag = b.TopTwoObservations[0].RagStatus;
            var priorRag = b.TopTwoObservations[1].RagStatus;

            if (GetRagSeverity(latestRag) > GetRagSeverity(priorRag))
            {
                var latestObs = b.TopTwoObservations[0];
                degraded.Add(new BarrierWithStatus
                {
                    BarrierId = b.BarrierId,
                    BarrierName = b.BarrierName,
                    BarrierType = b.BarrierType,
                    SiteId = b.SiteId,
                    AssetId = b.AssetId,
                    CriticalityRank = b.CriticalityRank,
                    CurrentRagStatus = latestRag,
                    LastAssessedDate = latestObs.ObservedAt,
                    DataQualityStatus = latestObs.DataQualityStatus
                });
            }
        }

        return degraded.OrderBy(b => b.CriticalityRank).ToList();
    }

    /// <summary>
    /// Maps RAG status to a numeric severity for comparison. Higher = worse.
    /// </summary>
    private static int GetRagSeverity(string ragStatus)
    {
        return ragStatus.ToUpperInvariant() switch
        {
            "GREEN" => 0,
            "AMBER" => 1,
            "RED" => 2,
            _ => 0
        };
    }

    /// <summary>
    /// Resolves RAG status using conservative rule: RED > AMBER > GREEN.
    /// </summary>
    private static string ResolveRagStatus(IEnumerable<string> statuses)
    {
        var statusList = statuses.ToList();

        if (statusList.Any(s => s.Equals("RED", StringComparison.OrdinalIgnoreCase)))
            return "RED";
        if (statusList.Any(s => s.Equals("AMBER", StringComparison.OrdinalIgnoreCase)))
            return "AMBER";

        return "GREEN";
    }

    /// <summary>
    /// Resolves data quality status: CONFLICT > FLAGGED > VALID (worst wins).
    /// </summary>
    private static string ResolveDataQualityStatus(IEnumerable<string> statuses)
    {
        var statusList = statuses.ToList();

        if (statusList.Any(s => s.Equals("CONFLICT", StringComparison.OrdinalIgnoreCase)))
            return "CONFLICT";
        if (statusList.Any(s => s.Equals("FLAGGED", StringComparison.OrdinalIgnoreCase)))
            return "FLAGGED";

        return "VALID";
    }
}
