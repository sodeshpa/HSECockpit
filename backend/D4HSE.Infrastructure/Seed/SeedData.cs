using D4HSE.Core.Entities;

namespace D4HSE.Infrastructure.Seed;

/// <summary>
/// Static class containing deterministic seed data for the pilot site and additional sites.
/// GUIDs are hardcoded to ensure idempotent seeding across environments.
/// </summary>
public static class SeedData
{
    // Site IDs
    public static readonly Guid NorthSeaPlatformAlphaSiteId =
        new("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

    public static readonly Guid GulfOfMexicoPlatformBetaSiteId =
        new("11111111-1111-1111-1111-111111111111");

    public static readonly Guid PermianBasinRefineryGammaSiteId =
        new("22222222-2222-2222-2222-222222222222");

    // Asset IDs (North Sea)
    public static readonly Guid GasCompressionModuleAId =
        new("b2c3d4e5-f6a7-8901-bcde-f12345678901");

    public static readonly Guid FireGasDetectionSystemId =
        new("c3d4e5f6-a7b8-9012-cdef-123456789012");

    public static readonly Guid EmergencyShutdownSystemId =
        new("d4e5f6a7-b8c9-0123-defa-234567890123");

    // Asset IDs (Gulf of Mexico)
    public static readonly Guid SubseaWellheadSystemId =
        new("33333333-3333-3333-3333-333333333331");

    public static readonly Guid RiserIntegritySysId =
        new("33333333-3333-3333-3333-333333333332");

    // Asset IDs (Permian Basin)
    public static readonly Guid FlareSystemId =
        new("44444444-4444-4444-4444-444444444441");

    public static readonly Guid CatalyticCrackerUnitId =
        new("44444444-4444-4444-4444-444444444442");

    public static readonly Guid StorageTankFarmId =
        new("44444444-4444-4444-4444-444444444443");

    // Barrier IDs (North Sea)
    public static readonly Guid ProcessSafetyValvePsv001Id =
        new("e5f6a7b8-c9d0-1234-efab-345678901234");

    public static readonly Guid FireDetectionZoneAId =
        new("f6a7b8c9-d0e1-2345-fabc-456789012345");

    public static readonly Guid EsdLogicSolverId =
        new("a7b8c9d0-e1f2-3456-abcd-567890123456");

    public static readonly Guid GasDetectionArrayBId =
        new("b8c9d0e1-f2a3-4567-bcde-678901234567");

    public static readonly Guid BlowdownSystemId =
        new("c9d0e1f2-a3b4-5678-cdef-789012345678");

    // Barrier IDs (Gulf of Mexico)
    public static readonly Guid SubseaBopId =
        new("55555555-5555-5555-5555-555555555551");

    public static readonly Guid RiserGasDetectionId =
        new("55555555-5555-5555-5555-555555555552");

    public static readonly Guid SubseaIsolationValveId =
        new("55555555-5555-5555-5555-555555555553");

    // Barrier IDs (Permian Basin)
    public static readonly Guid FlarePilotSystemId =
        new("66666666-6666-6666-6666-666666666661");

    public static readonly Guid CrackerOverpressureId =
        new("66666666-6666-6666-6666-666666666662");

    public static readonly Guid TankLevelAlarmId =
        new("66666666-6666-6666-6666-666666666663");

    public static readonly Guid H2sDetectorArrayId =
        new("66666666-6666-6666-6666-666666666664");

    /// <summary>
    /// Returns the pilot site seed record.
    /// </summary>
    public static Site GetPilotSite() => new()
    {
        SiteId = NorthSeaPlatformAlphaSiteId,
        SiteName = "North Sea Platform Alpha",
        Region = "North Sea",
        CreatedAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero)
    };

    /// <summary>
    /// Returns the 2 additional sites (Gulf of Mexico and Permian Basin).
    /// </summary>
    public static List<Site> GetAdditionalSites() =>
    [
        new()
        {
            SiteId = GulfOfMexicoPlatformBetaSiteId,
            SiteName = "Gulf of Mexico Platform Beta",
            Region = "Gulf of Mexico",
            CreatedAt = new DateTimeOffset(2024, 3, 15, 0, 0, 0, TimeSpan.Zero)
        },
        new()
        {
            SiteId = PermianBasinRefineryGammaSiteId,
            SiteName = "Permian Basin Refinery Gamma",
            Region = "Permian Basin",
            CreatedAt = new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero)
        }
    ];

    /// <summary>
    /// Returns the seed assets associated with the pilot site.
    /// </summary>
    public static List<Asset> GetPilotAssets() =>
    [
        new()
        {
            AssetId = GasCompressionModuleAId,
            SiteId = NorthSeaPlatformAlphaSiteId,
            AssetName = "Gas Compression Module A",
            AssetType = "Process Equipment",
            CreatedAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero)
        },
        new()
        {
            AssetId = FireGasDetectionSystemId,
            SiteId = NorthSeaPlatformAlphaSiteId,
            AssetName = "Fire & Gas Detection System",
            AssetType = "Safety System",
            CreatedAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero)
        },
        new()
        {
            AssetId = EmergencyShutdownSystemId,
            SiteId = NorthSeaPlatformAlphaSiteId,
            AssetName = "Emergency Shutdown System",
            AssetType = "Safety System",
            CreatedAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero)
        }
    ];

    /// <summary>
    /// Returns assets for the additional sites (Gulf of Mexico and Permian Basin).
    /// </summary>
    public static List<Asset> GetAdditionalAssets() =>
    [
        // Gulf of Mexico assets
        new()
        {
            AssetId = SubseaWellheadSystemId,
            SiteId = GulfOfMexicoPlatformBetaSiteId,
            AssetName = "Subsea Wellhead System",
            AssetType = "Subsea Equipment",
            CreatedAt = new DateTimeOffset(2024, 3, 15, 0, 0, 0, TimeSpan.Zero)
        },
        new()
        {
            AssetId = RiserIntegritySysId,
            SiteId = GulfOfMexicoPlatformBetaSiteId,
            AssetName = "Riser Integrity System",
            AssetType = "Structural System",
            CreatedAt = new DateTimeOffset(2024, 3, 15, 0, 0, 0, TimeSpan.Zero)
        },
        // Permian Basin assets
        new()
        {
            AssetId = FlareSystemId,
            SiteId = PermianBasinRefineryGammaSiteId,
            AssetName = "Flare System",
            AssetType = "Emission Control",
            CreatedAt = new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero)
        },
        new()
        {
            AssetId = CatalyticCrackerUnitId,
            SiteId = PermianBasinRefineryGammaSiteId,
            AssetName = "Catalytic Cracker Unit",
            AssetType = "Process Equipment",
            CreatedAt = new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero)
        },
        new()
        {
            AssetId = StorageTankFarmId,
            SiteId = PermianBasinRefineryGammaSiteId,
            AssetName = "Storage Tank Farm",
            AssetType = "Storage",
            CreatedAt = new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero)
        }
    ];

    /// <summary>
    /// Returns the seed critical barriers for the pilot site.
    /// </summary>
    public static List<CriticalBarrier> GetPilotBarriers() =>
    [
        new()
        {
            BarrierId = ProcessSafetyValvePsv001Id,
            SiteId = NorthSeaPlatformAlphaSiteId,
            AssetId = GasCompressionModuleAId,
            BarrierName = "Process Safety Valve PSV-001",
            BarrierType = "Pressure Relief",
            CriticalityRank = 1,
            IsActive = true,
            CreatedAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero)
        },
        new()
        {
            BarrierId = FireDetectionZoneAId,
            SiteId = NorthSeaPlatformAlphaSiteId,
            AssetId = FireGasDetectionSystemId,
            BarrierName = "Fire Detection Zone A",
            BarrierType = "Fire Detection",
            CriticalityRank = 2,
            IsActive = true,
            CreatedAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero)
        },
        new()
        {
            BarrierId = EsdLogicSolverId,
            SiteId = NorthSeaPlatformAlphaSiteId,
            AssetId = EmergencyShutdownSystemId,
            BarrierName = "ESD Logic Solver",
            BarrierType = "Emergency Shutdown",
            CriticalityRank = 3,
            IsActive = true,
            CreatedAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero)
        },
        new()
        {
            BarrierId = GasDetectionArrayBId,
            SiteId = NorthSeaPlatformAlphaSiteId,
            AssetId = FireGasDetectionSystemId,
            BarrierName = "Gas Detection Array B",
            BarrierType = "Gas Detection",
            CriticalityRank = 4,
            IsActive = true,
            CreatedAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero)
        },
        new()
        {
            BarrierId = BlowdownSystemId,
            SiteId = NorthSeaPlatformAlphaSiteId,
            AssetId = GasCompressionModuleAId,
            BarrierName = "Blowdown System",
            BarrierType = "Pressure Relief",
            CriticalityRank = 5,
            IsActive = true,
            CreatedAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero)
        }
    ];

    /// <summary>
    /// Returns barriers for the additional sites (Gulf of Mexico and Permian Basin).
    /// </summary>
    public static List<CriticalBarrier> GetAdditionalBarriers() =>
    [
        // Gulf of Mexico barriers
        new()
        {
            BarrierId = SubseaBopId,
            SiteId = GulfOfMexicoPlatformBetaSiteId,
            AssetId = SubseaWellheadSystemId,
            BarrierName = "Subsea BOP",
            BarrierType = "Well Control",
            CriticalityRank = 1,
            IsActive = true,
            CreatedAt = new DateTimeOffset(2024, 3, 15, 0, 0, 0, TimeSpan.Zero)
        },
        new()
        {
            BarrierId = RiserGasDetectionId,
            SiteId = GulfOfMexicoPlatformBetaSiteId,
            AssetId = RiserIntegritySysId,
            BarrierName = "Riser Gas Detection",
            BarrierType = "Gas Detection",
            CriticalityRank = 2,
            IsActive = true,
            CreatedAt = new DateTimeOffset(2024, 3, 15, 0, 0, 0, TimeSpan.Zero)
        },
        new()
        {
            BarrierId = SubseaIsolationValveId,
            SiteId = GulfOfMexicoPlatformBetaSiteId,
            AssetId = SubseaWellheadSystemId,
            BarrierName = "Subsea Isolation Valve",
            BarrierType = "Isolation",
            CriticalityRank = 3,
            IsActive = true,
            CreatedAt = new DateTimeOffset(2024, 3, 15, 0, 0, 0, TimeSpan.Zero)
        },
        // Permian Basin barriers
        new()
        {
            BarrierId = FlarePilotSystemId,
            SiteId = PermianBasinRefineryGammaSiteId,
            AssetId = FlareSystemId,
            BarrierName = "Flare Pilot System",
            BarrierType = "Emission Control",
            CriticalityRank = 1,
            IsActive = true,
            CreatedAt = new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero)
        },
        new()
        {
            BarrierId = CrackerOverpressureId,
            SiteId = PermianBasinRefineryGammaSiteId,
            AssetId = CatalyticCrackerUnitId,
            BarrierName = "Cracker Overpressure Protection",
            BarrierType = "Pressure Relief",
            CriticalityRank = 2,
            IsActive = true,
            CreatedAt = new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero)
        },
        new()
        {
            BarrierId = TankLevelAlarmId,
            SiteId = PermianBasinRefineryGammaSiteId,
            AssetId = StorageTankFarmId,
            BarrierName = "Tank Level Alarm",
            BarrierType = "Level Monitoring",
            CriticalityRank = 3,
            IsActive = true,
            CreatedAt = new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero)
        },
        new()
        {
            BarrierId = H2sDetectorArrayId,
            SiteId = PermianBasinRefineryGammaSiteId,
            AssetId = CatalyticCrackerUnitId,
            BarrierName = "H2S Detector Array",
            BarrierType = "Gas Detection",
            CriticalityRank = 4,
            IsActive = true,
            CreatedAt = new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero)
        }
    ];

    /// <summary>
    /// Returns 10 incidents across all 3 sites (mix of severities).
    /// </summary>
    public static List<Incident> GetIncidents() =>
    [
        // North Sea — 3 incidents (HIGH, MEDIUM, LOW)
        new()
        {
            IncidentId = new Guid("77777777-7777-7777-7777-777777777701"),
            SiteId = NorthSeaPlatformAlphaSiteId,
            AssetId = GasCompressionModuleAId,
            IncidentDate = new DateOnly(2026, 7, 3),
            Severity = "HIGH",
            IncidentType = "Gas Leak",
            Description = "Minor gas leak detected at compression module flange joint during routine operations.",
            SourceCategory = "incident_report",
            DataQualityStatus = "VALID",
            IngestedAt = new DateTimeOffset(2026, 7, 3, 14, 0, 0, TimeSpan.Zero)
        },
        new()
        {
            IncidentId = new Guid("77777777-7777-7777-7777-777777777702"),
            SiteId = NorthSeaPlatformAlphaSiteId,
            AssetId = FireGasDetectionSystemId,
            IncidentDate = new DateOnly(2026, 7, 8),
            Severity = "MEDIUM",
            IncidentType = "Equipment Failure",
            Description = "Fire detection sensor in Zone A returned intermittent false readings.",
            SourceCategory = "incident_report",
            DataQualityStatus = "VALID",
            IngestedAt = new DateTimeOffset(2026, 7, 8, 10, 0, 0, TimeSpan.Zero)
        },
        new()
        {
            IncidentId = new Guid("77777777-7777-7777-7777-777777777703"),
            SiteId = NorthSeaPlatformAlphaSiteId,
            AssetId = null,
            IncidentDate = new DateOnly(2026, 7, 12),
            Severity = "LOW",
            IncidentType = "Slip/Trip/Fall",
            Description = "Personnel slipped on wet deck surface near helideck access route.",
            SourceCategory = "incident_report",
            DataQualityStatus = "VALID",
            IngestedAt = new DateTimeOffset(2026, 7, 12, 8, 0, 0, TimeSpan.Zero)
        },
        // Gulf of Mexico — 4 incidents (CRITICAL, HIGH, MEDIUM, MEDIUM)
        new()
        {
            IncidentId = new Guid("77777777-7777-7777-7777-777777777704"),
            SiteId = GulfOfMexicoPlatformBetaSiteId,
            AssetId = SubseaWellheadSystemId,
            IncidentDate = new DateOnly(2026, 6, 18),
            Severity = "CRITICAL",
            IncidentType = "Gas Leak",
            Description = "Subsea gas leak detected at wellhead connector requiring emergency BOP activation.",
            SourceCategory = "incident_report",
            DataQualityStatus = "VALID",
            IngestedAt = new DateTimeOffset(2026, 6, 18, 6, 0, 0, TimeSpan.Zero)
        },
        new()
        {
            IncidentId = new Guid("77777777-7777-7777-7777-777777777705"),
            SiteId = GulfOfMexicoPlatformBetaSiteId,
            AssetId = RiserIntegritySysId,
            IncidentDate = new DateOnly(2026, 6, 25),
            Severity = "HIGH",
            IncidentType = "Equipment Failure",
            Description = "Riser tensioner hydraulic line failure causing partial loss of tension control.",
            SourceCategory = "incident_report",
            DataQualityStatus = "VALID",
            IngestedAt = new DateTimeOffset(2026, 6, 25, 11, 0, 0, TimeSpan.Zero)
        },
        new()
        {
            IncidentId = new Guid("77777777-7777-7777-7777-777777777706"),
            SiteId = GulfOfMexicoPlatformBetaSiteId,
            AssetId = null,
            IncidentDate = new DateOnly(2026, 7, 2),
            Severity = "MEDIUM",
            IncidentType = "Dropped Object",
            Description = "Wrench dropped from drill floor to lower deck during pipe handling operations.",
            SourceCategory = "incident_report",
            DataQualityStatus = "VALID",
            IngestedAt = new DateTimeOffset(2026, 7, 2, 9, 0, 0, TimeSpan.Zero)
        },
        new()
        {
            IncidentId = new Guid("77777777-7777-7777-7777-777777777707"),
            SiteId = GulfOfMexicoPlatformBetaSiteId,
            AssetId = SubseaWellheadSystemId,
            IncidentDate = new DateOnly(2026, 7, 10),
            Severity = "MEDIUM",
            IncidentType = "Confined Space",
            Description = "Inadequate ventilation identified during subsea equipment bay entry procedure.",
            SourceCategory = "incident_report",
            DataQualityStatus = "FLAGGED",
            IngestedAt = new DateTimeOffset(2026, 7, 10, 15, 0, 0, TimeSpan.Zero)
        },
        // Permian Basin — 3 incidents (HIGH, MEDIUM, LOW)
        new()
        {
            IncidentId = new Guid("77777777-7777-7777-7777-777777777708"),
            SiteId = PermianBasinRefineryGammaSiteId,
            AssetId = CatalyticCrackerUnitId,
            IncidentDate = new DateOnly(2026, 7, 5),
            Severity = "HIGH",
            IncidentType = "Chemical Spill",
            Description = "Catalyst fines release from cracker regenerator due to cyclone separator malfunction.",
            SourceCategory = "incident_report",
            DataQualityStatus = "VALID",
            IngestedAt = new DateTimeOffset(2026, 7, 5, 7, 0, 0, TimeSpan.Zero)
        },
        new()
        {
            IncidentId = new Guid("77777777-7777-7777-7777-777777777709"),
            SiteId = PermianBasinRefineryGammaSiteId,
            AssetId = StorageTankFarmId,
            IncidentDate = new DateOnly(2026, 7, 9),
            Severity = "MEDIUM",
            IncidentType = "Equipment Failure",
            Description = "Tank level gauge malfunction on Tank T-104 leading to overfill alarm trigger.",
            SourceCategory = "incident_report",
            DataQualityStatus = "VALID",
            IngestedAt = new DateTimeOffset(2026, 7, 9, 13, 0, 0, TimeSpan.Zero)
        },
        new()
        {
            IncidentId = new Guid("77777777-7777-7777-7777-777777777710"),
            SiteId = PermianBasinRefineryGammaSiteId,
            AssetId = null,
            IncidentDate = new DateOnly(2026, 7, 14),
            Severity = "LOW",
            IncidentType = "Slip/Trip/Fall",
            Description = "Contractor tripped over unsecured cable tray near pipe rack area.",
            SourceCategory = "incident_report",
            DataQualityStatus = "VALID",
            IngestedAt = new DateTimeOffset(2026, 7, 14, 16, 0, 0, TimeSpan.Zero)
        }
    ];

    /// <summary>
    /// Returns 8 near-misses across all 3 sites.
    /// </summary>
    public static List<NearMiss> GetNearMisses() =>
    [
        // North Sea — 2 near-misses
        new()
        {
            NearMissId = new Guid("88888888-8888-8888-8888-888888888801"),
            SiteId = NorthSeaPlatformAlphaSiteId,
            AssetId = GasCompressionModuleAId,
            EventDate = new DateOnly(2026, 6, 20),
            PotentialSeverity = "HIGH",
            Description = "Pressure relief valve nearly exceeded design limits during compressor startup sequence.",
            SourceCategory = "near_miss_report",
            DataQualityStatus = "VALID",
            IngestedAt = new DateTimeOffset(2026, 6, 20, 10, 0, 0, TimeSpan.Zero)
        },
        new()
        {
            NearMissId = new Guid("88888888-8888-8888-8888-888888888802"),
            SiteId = NorthSeaPlatformAlphaSiteId,
            AssetId = EmergencyShutdownSystemId,
            EventDate = new DateOnly(2026, 7, 6),
            PotentialSeverity = "MEDIUM",
            Description = "ESD trip signal delayed by 2 seconds due to logic solver communication timeout.",
            SourceCategory = "near_miss_report",
            DataQualityStatus = "VALID",
            IngestedAt = new DateTimeOffset(2026, 7, 6, 14, 0, 0, TimeSpan.Zero)
        },
        // Gulf of Mexico — 3 near-misses
        new()
        {
            NearMissId = new Guid("88888888-8888-8888-8888-888888888803"),
            SiteId = GulfOfMexicoPlatformBetaSiteId,
            AssetId = SubseaWellheadSystemId,
            EventDate = new DateOnly(2026, 6, 12),
            PotentialSeverity = "HIGH",
            Description = "BOP test revealed slow response time on annular ram close function.",
            SourceCategory = "near_miss_report",
            DataQualityStatus = "VALID",
            IngestedAt = new DateTimeOffset(2026, 6, 12, 8, 0, 0, TimeSpan.Zero)
        },
        new()
        {
            NearMissId = new Guid("88888888-8888-8888-8888-888888888804"),
            SiteId = GulfOfMexicoPlatformBetaSiteId,
            AssetId = RiserIntegritySysId,
            EventDate = new DateOnly(2026, 7, 1),
            PotentialSeverity = "MEDIUM",
            Description = "Riser flex joint angle approached operational limit during heavy weather.",
            SourceCategory = "near_miss_report",
            DataQualityStatus = "VALID",
            IngestedAt = new DateTimeOffset(2026, 7, 1, 6, 0, 0, TimeSpan.Zero)
        },
        new()
        {
            NearMissId = new Guid("88888888-8888-8888-8888-888888888805"),
            SiteId = GulfOfMexicoPlatformBetaSiteId,
            AssetId = null,
            EventDate = new DateOnly(2026, 7, 11),
            PotentialSeverity = "HIGH",
            Description = "Crane load exceeded safe working limit by 3% before operator abort.",
            SourceCategory = "near_miss_report",
            DataQualityStatus = "FLAGGED",
            IngestedAt = new DateTimeOffset(2026, 7, 11, 12, 0, 0, TimeSpan.Zero)
        },
        // Permian Basin — 3 near-misses
        new()
        {
            NearMissId = new Guid("88888888-8888-8888-8888-888888888806"),
            SiteId = PermianBasinRefineryGammaSiteId,
            AssetId = FlareSystemId,
            EventDate = new DateOnly(2026, 6, 28),
            PotentialSeverity = "HIGH",
            Description = "Flare pilot flame extinguished momentarily during high crosswind event.",
            SourceCategory = "near_miss_report",
            DataQualityStatus = "VALID",
            IngestedAt = new DateTimeOffset(2026, 6, 28, 18, 0, 0, TimeSpan.Zero)
        },
        new()
        {
            NearMissId = new Guid("88888888-8888-8888-8888-888888888807"),
            SiteId = PermianBasinRefineryGammaSiteId,
            AssetId = CatalyticCrackerUnitId,
            EventDate = new DateOnly(2026, 7, 7),
            PotentialSeverity = "MEDIUM",
            Description = "H2S concentration briefly spiked above action level near cracker unit before dilution.",
            SourceCategory = "near_miss_report",
            DataQualityStatus = "VALID",
            IngestedAt = new DateTimeOffset(2026, 7, 7, 9, 0, 0, TimeSpan.Zero)
        },
        new()
        {
            NearMissId = new Guid("88888888-8888-8888-8888-888888888808"),
            SiteId = PermianBasinRefineryGammaSiteId,
            AssetId = StorageTankFarmId,
            EventDate = new DateOnly(2026, 7, 13),
            PotentialSeverity = "MEDIUM",
            Description = "Tank truck overfill prevention valve activated during loading operations.",
            SourceCategory = "near_miss_report",
            DataQualityStatus = "VALID",
            IngestedAt = new DateTimeOffset(2026, 7, 13, 11, 0, 0, TimeSpan.Zero)
        }
    ];

    /// <summary>
    /// Returns 4 barrier health observations per barrier for all barriers (existing + new),
    /// spaced ~3 weeks apart to show trend history over the last 90 days.
    /// </summary>
    public static List<BarrierHealthObservation> GetBarrierHealthObservations() =>
    [
        // North Sea — PSV-001 (stable GREEN)
        Obs("99999999-0001-0001-0001-000000000001", ProcessSafetyValvePsv001Id, 2026, 6, 1, "GREEN", 92m, "VALID"),
        Obs("99999999-0001-0001-0001-000000000002", ProcessSafetyValvePsv001Id, 2026, 6, 22, "GREEN", 89m, "VALID"),
        Obs("99999999-0001-0001-0001-000000000003", ProcessSafetyValvePsv001Id, 2026, 7, 7, "GREEN", 85m, "VALID"),
        Obs("99999999-0001-0001-0001-000000000004", ProcessSafetyValvePsv001Id, 2026, 7, 15, "GREEN", 88m, "VALID"),

        // North Sea — Fire Detection Zone A (degrading GREEN → AMBER)
        Obs("99999999-0001-0002-0001-000000000001", FireDetectionZoneAId, 2026, 6, 1, "GREEN", 78m, "VALID"),
        Obs("99999999-0001-0002-0001-000000000002", FireDetectionZoneAId, 2026, 6, 22, "GREEN", 72m, "VALID"),
        Obs("99999999-0001-0002-0001-000000000003", FireDetectionZoneAId, 2026, 7, 7, "AMBER", 58m, "VALID"),
        Obs("99999999-0001-0002-0001-000000000004", FireDetectionZoneAId, 2026, 7, 15, "AMBER", 52m, "FLAGGED"),

        // North Sea — ESD Logic Solver (stable GREEN)
        Obs("99999999-0001-0003-0001-000000000001", EsdLogicSolverId, 2026, 6, 1, "GREEN", 95m, "VALID"),
        Obs("99999999-0001-0003-0001-000000000002", EsdLogicSolverId, 2026, 6, 22, "GREEN", 93m, "VALID"),
        Obs("99999999-0001-0003-0001-000000000003", EsdLogicSolverId, 2026, 7, 7, "GREEN", 91m, "VALID"),
        Obs("99999999-0001-0003-0001-000000000004", EsdLogicSolverId, 2026, 7, 15, "GREEN", 94m, "VALID"),

        // North Sea — Gas Detection Array B (degrading GREEN → AMBER → RED)
        Obs("99999999-0001-0004-0001-000000000001", GasDetectionArrayBId, 2026, 6, 1, "GREEN", 75m, "VALID"),
        Obs("99999999-0001-0004-0001-000000000002", GasDetectionArrayBId, 2026, 6, 22, "AMBER", 55m, "VALID"),
        Obs("99999999-0001-0004-0001-000000000003", GasDetectionArrayBId, 2026, 7, 7, "AMBER", 42m, "VALID"),
        Obs("99999999-0001-0004-0001-000000000004", GasDetectionArrayBId, 2026, 7, 15, "RED", 28m, "VALID"),

        // North Sea — Blowdown System (improving AMBER → GREEN)
        Obs("99999999-0001-0005-0001-000000000001", BlowdownSystemId, 2026, 6, 1, "AMBER", 48m, "VALID"),
        Obs("99999999-0001-0005-0001-000000000002", BlowdownSystemId, 2026, 6, 22, "AMBER", 55m, "VALID"),
        Obs("99999999-0001-0005-0001-000000000003", BlowdownSystemId, 2026, 7, 7, "GREEN", 72m, "VALID"),
        Obs("99999999-0001-0005-0001-000000000004", BlowdownSystemId, 2026, 7, 15, "GREEN", 78m, "VALID"),

        // Gulf of Mexico — Subsea BOP (degrading GREEN → RED)
        Obs("99999999-0002-0001-0001-000000000001", SubseaBopId, 2026, 6, 1, "GREEN", 82m, "VALID"),
        Obs("99999999-0002-0001-0001-000000000002", SubseaBopId, 2026, 6, 22, "AMBER", 60m, "VALID"),
        Obs("99999999-0002-0001-0001-000000000003", SubseaBopId, 2026, 7, 7, "AMBER", 45m, "VALID"),
        Obs("99999999-0002-0001-0001-000000000004", SubseaBopId, 2026, 7, 15, "RED", 32m, "VALID"),

        // Gulf of Mexico — Riser Gas Detection (stable AMBER)
        Obs("99999999-0002-0002-0001-000000000001", RiserGasDetectionId, 2026, 6, 1, "AMBER", 55m, "VALID"),
        Obs("99999999-0002-0002-0001-000000000002", RiserGasDetectionId, 2026, 6, 22, "AMBER", 52m, "VALID"),
        Obs("99999999-0002-0002-0001-000000000003", RiserGasDetectionId, 2026, 7, 7, "AMBER", 50m, "FLAGGED"),
        Obs("99999999-0002-0002-0001-000000000004", RiserGasDetectionId, 2026, 7, 15, "AMBER", 48m, "VALID"),

        // Gulf of Mexico — Subsea Isolation Valve (stable GREEN)
        Obs("99999999-0002-0003-0001-000000000001", SubseaIsolationValveId, 2026, 6, 1, "GREEN", 88m, "VALID"),
        Obs("99999999-0002-0003-0001-000000000002", SubseaIsolationValveId, 2026, 6, 22, "GREEN", 86m, "VALID"),
        Obs("99999999-0002-0003-0001-000000000003", SubseaIsolationValveId, 2026, 7, 7, "GREEN", 84m, "VALID"),
        Obs("99999999-0002-0003-0001-000000000004", SubseaIsolationValveId, 2026, 7, 15, "GREEN", 87m, "VALID"),

        // Permian Basin — Flare Pilot System (degrading GREEN → AMBER)
        Obs("99999999-0003-0001-0001-000000000001", FlarePilotSystemId, 2026, 6, 1, "GREEN", 80m, "VALID"),
        Obs("99999999-0003-0001-0001-000000000002", FlarePilotSystemId, 2026, 6, 22, "GREEN", 74m, "VALID"),
        Obs("99999999-0003-0001-0001-000000000003", FlarePilotSystemId, 2026, 7, 7, "AMBER", 62m, "VALID"),
        Obs("99999999-0003-0001-0001-000000000004", FlarePilotSystemId, 2026, 7, 15, "AMBER", 56m, "VALID"),

        // Permian Basin — Cracker Overpressure (stable GREEN)
        Obs("99999999-0003-0002-0001-000000000001", CrackerOverpressureId, 2026, 6, 1, "GREEN", 90m, "VALID"),
        Obs("99999999-0003-0002-0001-000000000002", CrackerOverpressureId, 2026, 6, 22, "GREEN", 88m, "VALID"),
        Obs("99999999-0003-0002-0001-000000000003", CrackerOverpressureId, 2026, 7, 7, "GREEN", 91m, "VALID"),
        Obs("99999999-0003-0002-0001-000000000004", CrackerOverpressureId, 2026, 7, 15, "GREEN", 89m, "VALID"),

        // Permian Basin — Tank Level Alarm (improving RED → AMBER → GREEN)
        Obs("99999999-0003-0003-0001-000000000001", TankLevelAlarmId, 2026, 6, 1, "RED", 25m, "VALID"),
        Obs("99999999-0003-0003-0001-000000000002", TankLevelAlarmId, 2026, 6, 22, "AMBER", 45m, "VALID"),
        Obs("99999999-0003-0003-0001-000000000003", TankLevelAlarmId, 2026, 7, 7, "AMBER", 60m, "VALID"),
        Obs("99999999-0003-0003-0001-000000000004", TankLevelAlarmId, 2026, 7, 15, "GREEN", 75m, "VALID"),

        // Permian Basin — H2S Detector Array (stable AMBER with one FLAGGED)
        Obs("99999999-0003-0004-0001-000000000001", H2sDetectorArrayId, 2026, 6, 1, "AMBER", 50m, "VALID"),
        Obs("99999999-0003-0004-0001-000000000002", H2sDetectorArrayId, 2026, 6, 22, "AMBER", 47m, "VALID"),
        Obs("99999999-0003-0004-0001-000000000003", H2sDetectorArrayId, 2026, 7, 7, "AMBER", 44m, "FLAGGED"),
        Obs("99999999-0003-0004-0001-000000000004", H2sDetectorArrayId, 2026, 7, 15, "AMBER", 46m, "VALID")
    ];

    /// <summary>
    /// Helper to create a BarrierHealthObservation with less boilerplate.
    /// </summary>
    private static BarrierHealthObservation Obs(
        string id, Guid barrierId, int year, int month, int day,
        string rag, decimal score, string qualityStatus) => new()
    {
        ObservationId = new Guid(id),
        BarrierId = barrierId,
        ObservedAt = new DateOnly(year, month, day),
        RagStatus = rag,
        ConditionScore = score,
        Notes = null,
        SourceCategory = "inspection",
        DataQualityStatus = qualityStatus,
        IngestedAt = new DateTimeOffset(year, month, day, 12, 0, 0, TimeSpan.Zero)
    };
}
