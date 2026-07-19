import { useQuery } from "@tanstack/react-query";
import {
  getBarriers,
  getBarrierTrend,
  getDegradedBarriers,
  getSites,
  getAssets,
} from "../api/barriers";

/**
 * Fetch barriers for a given site/asset context.
 * Stale time: 30 seconds (operational data).
 */
export function useBarriers(siteId, assetId) {
  return useQuery({
    queryKey: ["barriers", siteId, assetId],
    queryFn: () => getBarriers(siteId, assetId),
    staleTime: 30 * 1000,
  });
}

/**
 * Fetch trend data for a specific barrier (sparkline).
 * Stale time: 30 seconds (operational data).
 */
export function useBarrierTrend(barrierId, periodDays) {
  return useQuery({
    queryKey: ["barriers", barrierId, "trend"],
    queryFn: () => getBarrierTrend(barrierId, periodDays),
    enabled: !!barrierId,
    staleTime: 30 * 1000,
  });
}

/**
 * Fetch degraded barriers for a site.
 * Stale time: 30 seconds (operational data).
 */
export function useDegradedBarriers(siteId) {
  return useQuery({
    queryKey: ["barriers", "degraded", siteId],
    queryFn: () => getDegradedBarriers(siteId),
    staleTime: 30 * 1000,
  });
}

/**
 * Fetch all available sites (reference data).
 * Stale time: 5 minutes.
 */
export function useSites() {
  return useQuery({
    queryKey: ["sites"],
    queryFn: getSites,
    staleTime: 5 * 60 * 1000,
  });
}

/**
 * Fetch assets for a given site (reference data).
 * Disabled when siteId is not provided.
 * Stale time: 5 minutes.
 */
export function useAssets(siteId) {
  return useQuery({
    queryKey: ["assets", siteId],
    queryFn: () => getAssets(siteId),
    enabled: !!siteId,
    staleTime: 5 * 60 * 1000,
  });
}
