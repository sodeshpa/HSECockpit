import { useQuery } from "@tanstack/react-query";
import { getExecutiveKPIs, getCriticalRisks, getComplianceSummary } from "../api/executive";

export function useExecutiveKPIs() {
  return useQuery({
    queryKey: ["executive", "kpis"],
    queryFn: getExecutiveKPIs,
    staleTime: 30 * 1000,
  });
}

export function useCriticalRisks() {
  return useQuery({
    queryKey: ["executive", "critical-risks"],
    queryFn: getCriticalRisks,
    staleTime: 30 * 1000,
  });
}

export function useComplianceSummary() {
  return useQuery({
    queryKey: ["executive", "compliance"],
    queryFn: getComplianceSummary,
    staleTime: 5 * 60 * 1000,
  });
}
