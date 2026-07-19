using D4HSE.Core.Interfaces;
using D4HSE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace D4HSE.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of ISiteRepository.
/// Provides read-only access to Site entities.
/// </summary>
public class SiteRepository : ISiteRepository
{
    private readonly HseCockpitDbContext _dbContext;

    public SiteRepository(HseCockpitDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SiteSummary>> GetAllSitesAsync(CancellationToken ct)
    {
        return await _dbContext.Sites
            .AsNoTracking()
            .Select(s => new SiteSummary
            {
                SiteId = s.SiteId,
                SiteName = s.SiteName
            })
            .ToListAsync(ct);
    }
}
