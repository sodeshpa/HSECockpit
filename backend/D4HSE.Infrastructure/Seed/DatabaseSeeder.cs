using D4HSE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace D4HSE.Infrastructure.Seed;

/// <summary>
/// Seeds the database with pilot reference data.
/// Checks for existing data before inserting to ensure idempotency.
/// </summary>
public class DatabaseSeeder
{
    /// <summary>
    /// Seeds all sites, assets, barriers, observations, incidents, and near-misses.
    /// </summary>
    public async Task SeedAsync(HseCockpitDbContext context, CancellationToken cancellationToken = default)
    {
        await SeedSiteAsync(context, cancellationToken);
        await SeedAdditionalSitesAsync(context, cancellationToken);
        await SeedAssetsAsync(context, cancellationToken);
        await SeedAdditionalAssetsAsync(context, cancellationToken);
        await SeedBarriersAsync(context, cancellationToken);
        await SeedAdditionalBarriersAsync(context, cancellationToken);
        await SeedObservationsAsync(context, cancellationToken);
        await SeedIncidentsAsync(context, cancellationToken);
        await SeedNearMissesAsync(context, cancellationToken);
    }

    private static async Task SeedSiteAsync(HseCockpitDbContext context, CancellationToken cancellationToken)
    {
        var siteExists = await context.Sites
            .AnyAsync(s => s.SiteId == SeedData.NorthSeaPlatformAlphaSiteId, cancellationToken);

        if (!siteExists)
        {
            context.Sites.Add(SeedData.GetPilotSite());
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task SeedAdditionalSitesAsync(HseCockpitDbContext context, CancellationToken cancellationToken)
    {
        var additionalSites = SeedData.GetAdditionalSites();

        foreach (var site in additionalSites)
        {
            var exists = await context.Sites
                .AnyAsync(s => s.SiteId == site.SiteId, cancellationToken);

            if (!exists)
            {
                context.Sites.Add(site);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedAssetsAsync(HseCockpitDbContext context, CancellationToken cancellationToken)
    {
        var pilotAssets = SeedData.GetPilotAssets();

        foreach (var asset in pilotAssets)
        {
            var exists = await context.Assets
                .AnyAsync(a => a.AssetId == asset.AssetId, cancellationToken);

            if (!exists)
            {
                context.Assets.Add(asset);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedAdditionalAssetsAsync(HseCockpitDbContext context, CancellationToken cancellationToken)
    {
        var additionalAssets = SeedData.GetAdditionalAssets();

        foreach (var asset in additionalAssets)
        {
            var exists = await context.Assets
                .AnyAsync(a => a.AssetId == asset.AssetId, cancellationToken);

            if (!exists)
            {
                context.Assets.Add(asset);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedBarriersAsync(HseCockpitDbContext context, CancellationToken cancellationToken)
    {
        var pilotBarriers = SeedData.GetPilotBarriers();

        foreach (var barrier in pilotBarriers)
        {
            var exists = await context.CriticalBarriers
                .AnyAsync(b => b.BarrierId == barrier.BarrierId, cancellationToken);

            if (!exists)
            {
                context.CriticalBarriers.Add(barrier);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedAdditionalBarriersAsync(HseCockpitDbContext context, CancellationToken cancellationToken)
    {
        var additionalBarriers = SeedData.GetAdditionalBarriers();

        foreach (var barrier in additionalBarriers)
        {
            var exists = await context.CriticalBarriers
                .AnyAsync(b => b.BarrierId == barrier.BarrierId, cancellationToken);

            if (!exists)
            {
                context.CriticalBarriers.Add(barrier);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedObservationsAsync(HseCockpitDbContext context, CancellationToken cancellationToken)
    {
        var observations = SeedData.GetBarrierHealthObservations();

        foreach (var obs in observations)
        {
            var exists = await context.BarrierHealthObservations
                .AnyAsync(o => o.ObservationId == obs.ObservationId, cancellationToken);

            if (!exists)
            {
                context.BarrierHealthObservations.Add(obs);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedIncidentsAsync(HseCockpitDbContext context, CancellationToken cancellationToken)
    {
        var incidents = SeedData.GetIncidents();

        foreach (var incident in incidents)
        {
            var exists = await context.Incidents
                .AnyAsync(i => i.IncidentId == incident.IncidentId, cancellationToken);

            if (!exists)
            {
                context.Incidents.Add(incident);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedNearMissesAsync(HseCockpitDbContext context, CancellationToken cancellationToken)
    {
        var nearMisses = SeedData.GetNearMisses();

        foreach (var nearMiss in nearMisses)
        {
            var exists = await context.NearMisses
                .AnyAsync(n => n.NearMissId == nearMiss.NearMissId, cancellationToken);

            if (!exists)
            {
                context.NearMisses.Add(nearMiss);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
