using D4HSE.Core.Interfaces;
using D4HSE.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace D4HSE.Api.Controllers;

[ApiController]
[Route("api/v1")]
[Authorize]
public class SitesController : ControllerBase
{
    private readonly ISiteRepository _siteRepository;
    private readonly HseCockpitDbContext _dbContext;

    public SitesController(ISiteRepository siteRepository, HseCockpitDbContext dbContext)
    {
        _siteRepository = siteRepository;
        _dbContext = dbContext;
    }

    [HttpGet("sites")]
    public async Task<IActionResult> GetSites(CancellationToken ct)
    {
        var sites = await _siteRepository.GetAllSitesAsync(ct);
        var response = sites.Select(s => new { id = s.SiteId, name = s.SiteName });
        return Ok(response);
    }

    [HttpGet("assets")]
    public async Task<IActionResult> GetAssets([FromQuery] Guid? siteId, CancellationToken ct)
    {
        var query = _dbContext.Assets.AsNoTracking();

        if (siteId.HasValue)
        {
            query = query.Where(a => a.SiteId == siteId.Value);
        }

        var assets = await query
            .Select(a => new { id = a.AssetId, name = a.AssetName, siteId = a.SiteId })
            .OrderBy(a => a.name)
            .ToListAsync(ct);

        return Ok(assets);
    }
}
