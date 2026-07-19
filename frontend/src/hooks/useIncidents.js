import { useQuery } from "@tanstack/react-query";
import {
  getIncidentSummary,
  getNearMissSummary,
  getRiskScore,
  getRiskHeatmap,
} from "../api/incidents";

/**
 * Fetch incident summary for a site/asset and date range.
 * Stale time: 30 seconds (operational data).
 */
export function useIncidentSummary(siteId, assetId, fromDate, toDate) {
  return useQuery({
    queryKey: ["incidents", "summary", siteId, assetId, fromDate, toDate],
    queryFn: () => getIncidentSummary(siteId, assetId, fromDate, toDate),
    staleTime: 30 * 1000,
  });
}

/**
 * Fetch near-miss summary for a site/asset and date range.
 * Stale time: 30 seconds (operational data).
 */
export function useNearMissSummary(siteId, assetId, fromDate, toDate) {
  return useQuery({
    queryKey: ["nearMisses", "summary", siteId, assetId, fromDate, toDate],
    queryFn: () => getNearMissSummary(siteId, assetId, fromDate, toDate),
    staleTime: 30 * 1000,
  });
}

/**
 * Fetch risk score for a specific site.
 * Stale time: 30 seconds (operational data).
 * Disabled when siteId is not provided.
 */
export function useRiskScore(siteId) {
  return useQuery({
    queryKey: ["risk", "score", siteId],
    queryFn: () => getRiskScore(siteId),
    enabled: !!siteId,
    staleTime: 30 * 1000,
  });
}

/**
 * Fetch risk heatmap data.
 * Stale time: 30 seconds (operational data).
 */
export function useRiskHeatmap(periodDays) {
  return useQuery({
    queryKey: ["risk", "heatmap", periodDays],
    queryFn: () => getRiskHeatmap(periodDays),
    staleTime: 30 * 1000,
  });
}
