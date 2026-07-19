using D4HSE.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace D4HSE.Infrastructure.Data;

/// <summary>
/// EF Core DbContext for the HSE Cockpit PostgreSQL database.
/// Uses Npgsql provider with snake_case naming convention.
/// </summary>
public class HseCockpitDbContext : DbContext
{
    public HseCockpitDbContext(DbContextOptions<HseCockpitDbContext> options)
        : base(options)
    {
    }

    public DbSet<Site> Sites => Set<Site>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<CriticalBarrier> CriticalBarriers => Set<CriticalBarrier>();
    public DbSet<BarrierHealthObservation> BarrierHealthObservations => Set<BarrierHealthObservation>();
    public DbSet<Incident> Incidents => Set<Incident>();
    public DbSet<NearMiss> NearMisses => Set<NearMiss>();
    public DbSet<MaintenanceRecord> MaintenanceRecords => Set<MaintenanceRecord>();
    public DbSet<RiskItem> RiskItems => Set<RiskItem>();
    public DbSet<ComplianceStatus> ComplianceStatuses => Set<ComplianceStatus>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureSites(modelBuilder);
        ConfigureAssets(modelBuilder);
        ConfigureCriticalBarriers(modelBuilder);
        ConfigureBarrierHealthObservations(modelBuilder);
        ConfigureIncidents(modelBuilder);
        ConfigureNearMisses(modelBuilder);
        ConfigureMaintenanceRecords(modelBuilder);
        ConfigureRiskItems(modelBuilder);
        ConfigureComplianceStatuses(modelBuilder);
    }

    private static void ConfigureSites(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Site>(entity =>
        {
            entity.HasKey(e => e.SiteId);
            entity.Property(e => e.SiteId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.SiteName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Region).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        });
    }

    private static void ConfigureAssets(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasKey(e => e.AssetId);
            entity.Property(e => e.AssetId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.AssetName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.AssetType).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.Site)
                .WithMany(s => s.Assets)
                .HasForeignKey(e => e.SiteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.SiteId);
        });
    }

    private static void ConfigureCriticalBarriers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CriticalBarrier>(entity =>
        {
            entity.HasKey(e => e.BarrierId);
            entity.Property(e => e.BarrierId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.BarrierName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.BarrierType).HasMaxLength(100);
            entity.Property(e => e.CriticalityRank).IsRequired();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.Site)
                .WithMany(s => s.CriticalBarriers)
                .HasForeignKey(e => e.SiteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Asset)
                .WithMany(a => a.CriticalBarriers)
                .HasForeignKey(e => e.AssetId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.SiteId);
            entity.HasIndex(e => e.AssetId);
        });
    }

    private static void ConfigureBarrierHealthObservations(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BarrierHealthObservation>(entity =>
        {
            entity.HasKey(e => e.ObservationId);
            entity.Property(e => e.ObservationId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.ObservedAt).IsRequired();
            entity.Property(e => e.RagStatus).HasMaxLength(10).IsRequired();
            entity.Property(e => e.ConditionScore).HasPrecision(5, 2);
            entity.Property(e => e.SourceCategory).HasMaxLength(50).IsRequired();
            entity.Property(e => e.DataQualityStatus).HasMaxLength(20).HasDefaultValue("VALID");
            entity.Property(e => e.IngestedAt).HasDefaultValueSql("NOW()");

            // CHECK constraints for enums
            entity.ToTable(t => t.HasCheckConstraint(
                "ck_barrier_health_observations_rag_status",
                "rag_status IN ('GREEN','AMBER','RED')"));

            entity.ToTable(t => t.HasCheckConstraint(
                "ck_barrier_health_observations_data_quality_status",
                "data_quality_status IN ('VALID','FLAGGED','CONFLICT')"));

            entity.HasOne(e => e.CriticalBarrier)
                .WithMany(b => b.HealthObservations)
                .HasForeignKey(e => e.BarrierId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.BarrierId, e.ObservedAt })
                .IsDescending(false, true);
        });
    }

    private static void ConfigureIncidents(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Incident>(entity =>
        {
            entity.HasKey(e => e.IncidentId);
            entity.Property(e => e.IncidentId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.IncidentDate).IsRequired();
            entity.Property(e => e.Severity).HasMaxLength(20).IsRequired();
            entity.Property(e => e.IncidentType).HasMaxLength(100);
            entity.Property(e => e.SourceCategory).HasMaxLength(50).IsRequired();
            entity.Property(e => e.DataQualityStatus).HasMaxLength(20).HasDefaultValue("VALID");
            entity.Property(e => e.IngestedAt).HasDefaultValueSql("NOW()");

            // CHECK constraints
            entity.ToTable(t => t.HasCheckConstraint(
                "ck_incidents_severity",
                "severity IN ('LOW','MEDIUM','HIGH','CRITICAL')"));

            entity.ToTable(t => t.HasCheckConstraint(
                "ck_incidents_data_quality_status",
                "data_quality_status IN ('VALID','FLAGGED')"));

            entity.HasOne(e => e.Site)
                .WithMany(s => s.Incidents)
                .HasForeignKey(e => e.SiteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.SiteId, e.IncidentDate })
                .IsDescending(false, true);
        });
    }

    private static void ConfigureNearMisses(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NearMiss>(entity =>
        {
            entity.HasKey(e => e.NearMissId);
            entity.Property(e => e.NearMissId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.EventDate).IsRequired();
            entity.Property(e => e.PotentialSeverity).HasMaxLength(20);
            entity.Property(e => e.SourceCategory).HasMaxLength(50).IsRequired();
            entity.Property(e => e.DataQualityStatus).HasMaxLength(20).HasDefaultValue("VALID");
            entity.Property(e => e.IngestedAt).HasDefaultValueSql("NOW()");

            // CHECK constraints
            entity.ToTable(t => t.HasCheckConstraint(
                "ck_near_misses_potential_severity",
                "potential_severity IN ('LOW','MEDIUM','HIGH','CRITICAL')"));

            entity.ToTable(t => t.HasCheckConstraint(
                "ck_near_misses_data_quality_status",
                "data_quality_status IN ('VALID','FLAGGED')"));

            entity.HasOne(e => e.Site)
                .WithMany(s => s.NearMisses)
                .HasForeignKey(e => e.SiteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.SiteId, e.EventDate })
                .IsDescending(false, true);
        });
    }

    private static void ConfigureMaintenanceRecords(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MaintenanceRecord>(entity =>
        {
            entity.HasKey(e => e.MaintenanceId);
            entity.Property(e => e.MaintenanceId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.MaintenanceDate).IsRequired();
            entity.Property(e => e.ActivityType).HasMaxLength(100);
            entity.Property(e => e.Outcome).HasMaxLength(100);
            entity.Property(e => e.SourceCategory).HasMaxLength(50).IsRequired();
            entity.Property(e => e.DataQualityStatus).HasMaxLength(20).HasDefaultValue("VALID");
            entity.Property(e => e.IngestedAt).HasDefaultValueSql("NOW()");

            // CHECK constraint
            entity.ToTable(t => t.HasCheckConstraint(
                "ck_maintenance_records_data_quality_status",
                "data_quality_status IN ('VALID','FLAGGED')"));

            entity.HasOne(e => e.Asset)
                .WithMany(a => a.MaintenanceRecords)
                .HasForeignKey(e => e.AssetId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Site)
                .WithMany(s => s.MaintenanceRecords)
                .HasForeignKey(e => e.SiteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.AssetId, e.MaintenanceDate })
                .IsDescending(false, true);
        });
    }

    private static void ConfigureRiskItems(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RiskItem>(entity =>
        {
            entity.HasKey(e => e.RiskId);
            entity.Property(e => e.RiskId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.RiskDescription).IsRequired();
            entity.Property(e => e.Severity).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("OPEN");

            // CHECK constraints
            entity.ToTable(t => t.HasCheckConstraint(
                "ck_risk_items_severity",
                "severity IN ('LOW','MEDIUM','HIGH','CRITICAL')"));

            entity.ToTable(t => t.HasCheckConstraint(
                "ck_risk_items_status",
                "status IN ('OPEN','CLOSED','MONITORING')"));

            entity.HasOne(e => e.Site)
                .WithMany(s => s.RiskItems)
                .HasForeignKey(e => e.SiteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CriticalBarrier)
                .WithMany(b => b.RiskItems)
                .HasForeignKey(e => e.BarrierId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => new { e.SiteId, e.Status });
        });
    }

    private static void ConfigureComplianceStatuses(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ComplianceStatus>(entity =>
        {
            entity.HasKey(e => e.ComplianceId);
            entity.Property(e => e.ComplianceId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.PeriodStart).IsRequired();
            entity.Property(e => e.PeriodEnd).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
            entity.Property(e => e.AssessedAt).HasDefaultValueSql("NOW()");

            // CHECK constraint
            entity.ToTable(t => t.HasCheckConstraint(
                "ck_compliance_status_status",
                "status IN ('COMPLIANT','NON_COMPLIANT','UNKNOWN')"));

            entity.HasOne(e => e.Site)
                .WithMany(s => s.ComplianceStatuses)
                .HasForeignKey(e => e.SiteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.SiteId, e.PeriodEnd })
                .IsDescending(false, true);
        });
    }
}
