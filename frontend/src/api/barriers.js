import apiClient from "./client";

const MOCK_SITES = [
  { id: "a1b2c3d4-e5f6-7890-abcd-ef1234567890", name: "North Sea Platform Alpha" }
];

const MOCK_ASSETS = [
  { id: "b2c3d4e5-f6a7-8901-bcde-f12345678901", name: "Gas Compression Module A" },
  { id: "c3d4e5f6-a7b8-9012-cdef-123456789012", name: "Fire & Gas Detection System" },
  { id: "d4e5f6a7-b8c9-0123-defa-234567890123", name: "Emergency Shutdown System" }
];

const MOCK_BARRIERS = [
  { id: "e5f6a7b8-c9d0-1234-efab-345678901234", name: "Process Safety Valve PSV-001", type: "Pressure Relief", currentStatus: "GREEN", criticalityRank: 1, lastAssessedAt: "2026-07-15", siteName: "North Sea Platform Alpha", assetName: "Gas Compression Module A", dataQualityStatus: "VALID" },
  { id: "f6a7b8c9-d0e1-2345-fabc-456789012345", name: "Fire Detection Zone A", type: "Fire Detection", currentStatus: "AMBER", criticalityRank: 2, lastAssessedAt: "2026-07-15", siteName: "North Sea Platform Alpha", assetName: "Fire & Gas Detection System", dataQualityStatus: "VALID" },
  { id: "a7b8c9d0-e1f2-3456-abcd-567890123456", name: "ESD Logic Solver", type: "Emergency Shutdown", currentStatus: "RED", criticalityRank: 3, lastAssessedAt: "2026-07-15", siteName: "North Sea Platform Alpha", assetName: "Emergency Shutdown System", dataQualityStatus: "FLAGGED" },
  { id: "b8c9d0e1-f2a3-4567-bcde-678901234567", name: "Gas Detection Array B", type: "Gas Detection", currentStatus: "GREEN", criticalityRank: 4, lastAssessedAt: "2026-07-14", siteName: "North Sea Platform Alpha", assetName: "Fire & Gas Detection System", dataQualityStatus: "VALID" },
  { id: "c9d0e1f2-a3b4-5678-cdef-789012345678", name: "Blowdown System", type: "Pressure Relief", currentStatus: "AMBER", criticalityRank: 5, lastAssessedAt: "2026-07-13", siteName: "North Sea Platform Alpha", assetName: "Gas Compression Module A", dataQualityStatus: "VALID" }
];

const MOCK_TREND = [
  { observedAt: "2026-06-01", ragStatus: "GREEN", conditionScore: 90 },
  { observedAt: "2026-06-15", ragStatus: "GREEN", conditionScore: 85 },
  { observedAt: "2026-07-01", ragStatus: "AMBER", conditionScore: 70 },
  { observedAt: "2026-07-10", ragStatus: "AMBER", conditionScore: 62 },
  { observedAt: "2026-07-15", ragStatus: "RED", conditionScore: 28 }
];

function mapBarrier(b) {
  return {
    id: b.barrierId || b.id,
    name: b.barrierName || b.name,
    type: b.barrierType || b.type,
    siteId: b.siteId,
    assetId: b.assetId,
    siteName: b.siteName,
    assetName: b.assetName,
    criticalityRank: b.criticalityRank,
    currentStatus: b.currentRagStatus || b.currentStatus,
    lastAssessedAt: b.lastAssessedDate || b.lastAssessedAt,
    dataQualityStatus: b.dataQualityStatus,
    observations: b.observations
  };
}

async function tryOrMock(apiCall, mockData) {
  try {
    return await apiCall();
  } catch {
    console.warn("[Demo Mode] Using mock data - API not available");
    return mockData;
  }
}

export async function getBarriers(siteId, assetId) {
  return tryOrMock(async () => {
    const params = {};
    if (siteId) params.siteId = siteId;
    if (assetId) params.assetId = assetId;
    const response = await apiClient.get("/barriers", { params });
    return (response.data || []).map(mapBarrier);
  }, MOCK_BARRIERS);
}

export async function getBarrierTrend(barrierId, periodDays) {
  return tryOrMock(async () => {
    const params = {};
    if (periodDays) params.periodDays = periodDays;
    const response = await apiClient.get(`/barriers/${barrierId}/trend`, { params });
    return response.data;
  }, MOCK_TREND);
}

export async function getDegradedBarriers(siteId) {
  return tryOrMock(async () => {
    const params = {};
    if (siteId) params.siteId = siteId;
    const response = await apiClient.get("/barriers/degraded", { params });
    return (response.data || []).map(mapBarrier);
  }, [MOCK_BARRIERS[2]]);
}

export async function getSites() {
  return tryOrMock(async () => {
    const response = await apiClient.get("/sites");
    return response.data;
  }, MOCK_SITES);
}

export async function getAssets(siteId) {
  return tryOrMock(async () => {
    const params = {};
    if (siteId) params.siteId = siteId;
    const response = await apiClient.get("/assets", { params });
    return response.data;
  }, MOCK_ASSETS);
}
