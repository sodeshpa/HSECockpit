using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace D4HSE.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sites",
                columns: table => new
                {
                    site_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    site_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    region = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sites", x => x.site_id);
                });

            migrationBuilder.CreateTable(
                name: "assets",
                columns: table => new
                {
                    asset_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    site_id = table.Column<Guid>(type: "uuid", nullable: false),
                    asset_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    asset_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_assets", x => x.asset_id);
                    table.ForeignKey(
                        name: "fk_assets_sites_site_id",
                        column: x => x.site_id,
                        principalTable: "sites",
                        principalColumn: "site_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "compliance_statuses",
                columns: table => new
                {
                    compliance_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    site_id = table.Column<Guid>(type: "uuid", nullable: false),
                    period_start = table.Column<DateOnly>(type: "date", nullable: false),
                    period_end = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    assessed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_compliance_statuses", x => x.compliance_id);
                    table.CheckConstraint("ck_compliance_status_status", "status IN ('COMPLIANT','NON_COMPLIANT','UNKNOWN')");
                    table.ForeignKey(
                        name: "fk_compliance_statuses_sites_site_id",
                        column: x => x.site_id,
                        principalTable: "sites",
                        principalColumn: "site_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "critical_barriers",
                columns: table => new
                {
                    barrier_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    site_id = table.Column<Guid>(type: "uuid", nullable: false),
                    asset_id = table.Column<Guid>(type: "uuid", nullable: true),
                    barrier_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    barrier_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    criticality_rank = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_critical_barriers", x => x.barrier_id);
                    table.ForeignKey(
                        name: "fk_critical_barriers_assets_asset_id",
                        column: x => x.asset_id,
                        principalTable: "assets",
                        principalColumn: "asset_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_critical_barriers_sites_site_id",
                        column: x => x.site_id,
                        principalTable: "sites",
                        principalColumn: "site_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "incidents",
                columns: table => new
                {
                    incident_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    site_id = table.Column<Guid>(type: "uuid", nullable: false),
                    asset_id = table.Column<Guid>(type: "uuid", nullable: true),
                    incident_date = table.Column<DateOnly>(type: "date", nullable: false),
                    severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    incident_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    source_category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    data_quality_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "VALID"),
                    ingested_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_incidents", x => x.incident_id);
                    table.CheckConstraint("ck_incidents_data_quality_status", "data_quality_status IN ('VALID','FLAGGED')");
                    table.CheckConstraint("ck_incidents_severity", "severity IN ('LOW','MEDIUM','HIGH','CRITICAL')");
                    table.ForeignKey(
                        name: "fk_incidents_assets_asset_id",
                        column: x => x.asset_id,
                        principalTable: "assets",
                        principalColumn: "asset_id");
                    table.ForeignKey(
                        name: "fk_incidents_sites_site_id",
                        column: x => x.site_id,
                        principalTable: "sites",
                        principalColumn: "site_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "maintenance_records",
                columns: table => new
                {
                    maintenance_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    asset_id = table.Column<Guid>(type: "uuid", nullable: false),
                    site_id = table.Column<Guid>(type: "uuid", nullable: false),
                    maintenance_date = table.Column<DateOnly>(type: "date", nullable: false),
                    activity_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    outcome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    source_category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    data_quality_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "VALID"),
                    ingested_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_maintenance_records", x => x.maintenance_id);
                    table.CheckConstraint("ck_maintenance_records_data_quality_status", "data_quality_status IN ('VALID','FLAGGED')");
                    table.ForeignKey(
                        name: "fk_maintenance_records_assets_asset_id",
                        column: x => x.asset_id,
                        principalTable: "assets",
                        principalColumn: "asset_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_maintenance_records_sites_site_id",
                        column: x => x.site_id,
                        principalTable: "sites",
                        principalColumn: "site_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "near_misses",
                columns: table => new
                {
                    near_miss_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    site_id = table.Column<Guid>(type: "uuid", nullable: false),
                    asset_id = table.Column<Guid>(type: "uuid", nullable: true),
                    event_date = table.Column<DateOnly>(type: "date", nullable: false),
                    potential_severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    source_category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    data_quality_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "VALID"),
                    ingested_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_near_misses", x => x.near_miss_id);
                    table.CheckConstraint("ck_near_misses_data_quality_status", "data_quality_status IN ('VALID','FLAGGED')");
                    table.CheckConstraint("ck_near_misses_potential_severity", "potential_severity IN ('LOW','MEDIUM','HIGH','CRITICAL')");
                    table.ForeignKey(
                        name: "fk_near_misses_assets_asset_id",
                        column: x => x.asset_id,
                        principalTable: "assets",
                        principalColumn: "asset_id");
                    table.ForeignKey(
                        name: "fk_near_misses_sites_site_id",
                        column: x => x.site_id,
                        principalTable: "sites",
                        principalColumn: "site_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "barrier_health_observations",
                columns: table => new
                {
                    observation_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    barrier_id = table.Column<Guid>(type: "uuid", nullable: false),
                    observed_at = table.Column<DateOnly>(type: "date", nullable: false),
                    rag_status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    condition_score = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    source_category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    data_quality_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "VALID"),
                    ingested_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_barrier_health_observations", x => x.observation_id);
                    table.CheckConstraint("ck_barrier_health_observations_data_quality_status", "data_quality_status IN ('VALID','FLAGGED','CONFLICT')");
                    table.CheckConstraint("ck_barrier_health_observations_rag_status", "rag_status IN ('GREEN','AMBER','RED')");
                    table.ForeignKey(
                        name: "fk_barrier_health_observations_critical_barriers_barrier_id",
                        column: x => x.barrier_id,
                        principalTable: "critical_barriers",
                        principalColumn: "barrier_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "risk_items",
                columns: table => new
                {
                    risk_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    site_id = table.Column<Guid>(type: "uuid", nullable: false),
                    asset_id = table.Column<Guid>(type: "uuid", nullable: true),
                    barrier_id = table.Column<Guid>(type: "uuid", nullable: true),
                    risk_description = table.Column<string>(type: "text", nullable: false),
                    severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "OPEN"),
                    identified_at = table.Column<DateOnly>(type: "date", nullable: true),
                    resolved_at = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_risk_items", x => x.risk_id);
                    table.CheckConstraint("ck_risk_items_severity", "severity IN ('LOW','MEDIUM','HIGH','CRITICAL')");
                    table.CheckConstraint("ck_risk_items_status", "status IN ('OPEN','CLOSED','MONITORING')");
                    table.ForeignKey(
                        name: "fk_risk_items_assets_asset_id",
                        column: x => x.asset_id,
                        principalTable: "assets",
                        principalColumn: "asset_id");
                    table.ForeignKey(
                        name: "fk_risk_items_critical_barriers_barrier_id",
                        column: x => x.barrier_id,
                        principalTable: "critical_barriers",
                        principalColumn: "barrier_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_risk_items_sites_site_id",
                        column: x => x.site_id,
                        principalTable: "sites",
                        principalColumn: "site_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_assets_site_id",
                table: "assets",
                column: "site_id");

            migrationBuilder.CreateIndex(
                name: "ix_barrier_health_observations_barrier_id_observed_at",
                table: "barrier_health_observations",
                columns: new[] { "barrier_id", "observed_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_compliance_statuses_site_id_period_end",
                table: "compliance_statuses",
                columns: new[] { "site_id", "period_end" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_critical_barriers_asset_id",
                table: "critical_barriers",
                column: "asset_id");

            migrationBuilder.CreateIndex(
                name: "ix_critical_barriers_site_id",
                table: "critical_barriers",
                column: "site_id");

            migrationBuilder.CreateIndex(
                name: "ix_incidents_asset_id",
                table: "incidents",
                column: "asset_id");

            migrationBuilder.CreateIndex(
                name: "ix_incidents_site_id_incident_date",
                table: "incidents",
                columns: new[] { "site_id", "incident_date" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_records_asset_id_maintenance_date",
                table: "maintenance_records",
                columns: new[] { "asset_id", "maintenance_date" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_records_site_id",
                table: "maintenance_records",
                column: "site_id");

            migrationBuilder.CreateIndex(
                name: "ix_near_misses_asset_id",
                table: "near_misses",
                column: "asset_id");

            migrationBuilder.CreateIndex(
                name: "ix_near_misses_site_id_event_date",
                table: "near_misses",
                columns: new[] { "site_id", "event_date" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_risk_items_asset_id",
                table: "risk_items",
                column: "asset_id");

            migrationBuilder.CreateIndex(
                name: "ix_risk_items_barrier_id",
                table: "risk_items",
                column: "barrier_id");

            migrationBuilder.CreateIndex(
                name: "ix_risk_items_site_id_status",
                table: "risk_items",
                columns: new[] { "site_id", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "barrier_health_observations");

            migrationBuilder.DropTable(
                name: "compliance_statuses");

            migrationBuilder.DropTable(
                name: "incidents");

            migrationBuilder.DropTable(
                name: "maintenance_records");

            migrationBuilder.DropTable(
                name: "near_misses");

            migrationBuilder.DropTable(
                name: "risk_items");

            migrationBuilder.DropTable(
                name: "critical_barriers");

            migrationBuilder.DropTable(
                name: "assets");

            migrationBuilder.DropTable(
                name: "sites");
        }
    }
}
