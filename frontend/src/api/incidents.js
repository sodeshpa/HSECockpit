import apiClient from "./client";

const MOCK_INCIDENT_SUMMARY = { totalCount: 5, lowCount: 1, mediumCount: 2, highCount: 1, criticalCount: 1, severityBreakdown: { low: 1, medium: 2, high: 1, critical: 1 } };
const MOCK_NEAR_MISS_SUMMARY = { totalCount: 3, priorPeriodCount: 2, trendDirection: "UP", percentageChange: 50.0 };
const MOCK_RISK_SCORE = { score: 47.5, riskBand: "MEDIUM", dataQualityStatus: "PARTIAL", factors: [{ name: "Incident Severity", value: "0.6", weight: 0.25 }, { name: "Near-Miss Frequency", value: "0.4", weight: 0.25 }, { name: "Open Risks", value: "0.3", weight: 0.25 }, { name: "Barrier Health", value: "0.6", weight: 0.25 }] };
const MOCK_HEATMAP = [{ siteId: "a1b2c3d4-e5f6-7890-abcd-ef1234567890", siteName: "North Sea Platform Alpha", riskScore: 47.5, riskBand: "MEDIUM" }];

async function tryOrMock(apiCall, mockData) {
  try { return await apiCall(); } catch { console.warn("[Demo Mode] Using mock data"); return mockData; }
}

export async function getIncidentSummary(siteId, assetId, fromDate, toDate) {
  return tryOrMock(async () => {
    const params = {};
    if (siteId) params.siteId = siteId;
    if (assetId) params.assetId = assetId;
    if (fromDate) params.fromDate = fromDate;
    if (toDate) params.toDate = toDate;
    const response = await apiClient.get("/incidents/summary", { params });
    const d = response.data;
    return {
      totalCount: d.totalCount,
      severityBreakdown: {
        low: d.lowCount || 0,
        medium: d.mediumCount || 0,
        high: d.highCount || 0,
        critical: d.criticalCount || 0
      }
    };
  }, MOCK_INCIDENT_SUMMARY);
}

export async function getNearMissSummary(siteId, assetId, fromDate, toDate) {
  return tryOrMock(async () => { const params = {}; if (siteId) params.siteId = siteId; if (assetId) params.assetId = assetId; if (fromDate) params.fromDate = fromDate; if (toDate) params.toDate = toDate; return (await apiClient.get("/near-misses/summary", { params })).data; }, MOCK_NEAR_MISS_SUMMARY);
}

export async function getRiskScore(siteId) {
  return tryOrMock(async () => (await apiClient.get(`/risk/score/${siteId}`)).data, MOCK_RISK_SCORE);
}

export async function getRiskHeatmap(periodDays) {
  return tryOrMock(async () => { const params = {}; if (periodDays) params.periodDays = periodDays; return (await apiClient.get("/risk/heatmap", { params })).data; }, MOCK_HEATMAP);
}
