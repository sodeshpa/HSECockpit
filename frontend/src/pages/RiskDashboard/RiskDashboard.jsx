import { useState, useCallback } from "react";
import { SiteAssetFilter } from "../../components/filters/SiteAssetFilter";
import { DateRangePicker } from "../../components/filters/DateRangePicker";
import { IncidentSummaryCard } from "./IncidentSummaryCard";
import { NearMissSummaryCard } from "./NearMissSummaryCard";
import { SiteRiskScoreCard } from "./SiteRiskScoreCard";
import { RiskHeatmapGrid } from "./RiskHeatmapGrid";
import {
  useIncidentSummary,
  useNearMissSummary,
  useRiskScore,
  useRiskHeatmap,
} from "../../hooks/useIncidents";

/**
 * Risk Dashboard page container.
 * Combines filters (SiteAssetFilter + DateRangePicker) with summary cards
 * and the risk heatmap grid.
 */
export function RiskDashboard() {
  const [filters, setFilters] = useState({
    siteId: null,
    assetId: null,
  });
  const [dateRange, setDateRange] = useState({
    fromDate: null,
    toDate: null,
  });

  const handleFilterChange = useCallback(({ siteId, assetId }) => {
    setFilters({ siteId, assetId });
  }, []);

  const handleDateChange = useCallback(({ fromDate, toDate }) => {
    setDateRange({ fromDate, toDate });
  }, []);

  const {
    data: incidentData,
    isLoading: incidentLoading,
    isError: incidentError,
  } = useIncidentSummary(
    filters.siteId,
    filters.assetId,
    dateRange.fromDate,
    dateRange.toDate
  );

  const {
    data: nearMissData,
    isLoading: nearMissLoading,
    isError: nearMissError,
  } = useNearMissSummary(
    filters.siteId,
    filters.assetId,
    dateRange.fromDate,
    dateRange.toDate
  );

  const {
    data: riskScoreData,
    isLoading: riskScoreLoading,
    isError: riskScoreError,
  } = useRiskScore(filters.siteId);

  const {
    data: heatmapData,
    isLoading: heatmapLoading,
    isError: heatmapError,
  } = useRiskHeatmap(30);

  return (
    <section className="space-y-6 p-6" aria-label="Risk Dashboard">
      <h1 className="text-2xl font-bold text-gray-900">Risk Dashboard</h1>

      {/* Filters */}
      <div className="flex flex-wrap gap-4">
        <SiteAssetFilter onFilterChange={handleFilterChange} />
        <DateRangePicker
          fromDate={dateRange.fromDate || ""}
          toDate={dateRange.toDate || ""}
          onDateChange={handleDateChange}
        />
      </div>

      {/* Summary Cards Grid */}
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
        <IncidentSummaryCard
          data={incidentData}
          isLoading={incidentLoading}
          isError={incidentError}
        />
        <NearMissSummaryCard
          data={nearMissData}
          isLoading={nearMissLoading}
          isError={nearMissError}
        />
        <SiteRiskScoreCard
          data={riskScoreData}
          isLoading={riskScoreLoading}
          isError={riskScoreError}
          siteId={filters.siteId}
        />
      </div>

      {/* Heatmap */}
      <RiskHeatmapGrid
        data={heatmapData}
        isLoading={heatmapLoading}
        isError={heatmapError}
      />
    </section>
  );
}
