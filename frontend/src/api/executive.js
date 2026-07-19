import apiClient from "./client";

const MOCK_KPIS = {
  barrierHealthScore: 72.5,
  openCriticalRisks: 3,
  incidentCountMTD: 10,
  complianceRate: 85.0,
  dataQualityStatus: "PARTIAL"
};

const MOCK_CRITICAL_RISKS = [
  { riskId: "r1", siteName: "Gulf of Mexico Platform Beta", description: "Subsea BOP degraded to RED status - immediate inspection required", severity: "CRITICAL", status: "OPEN", daysOpen: 12 },
  { riskId: "r2", siteName: "North Sea Platform Alpha", description: "Gas Detection Array B showing progressive degradation trend", severity: "CRITICAL", status: "OPEN", daysOpen: 8 },
  { riskId: "r3", siteName: "Permian Basin Refinery Gamma", description: "H2S detector calibration overdue on Catalytic Cracker Unit", severity: "CRITICAL", status: "OPEN", daysOpen: 5 }
];

const MOCK_COMPLIANCE = [
  { siteId: "a1b2c3d4-e5f6-7890-abcd-ef1234567890", siteName: "North Sea Platform Alpha", status: "COMPLIANT" },
  { siteId: "11111111-1111-1111-1111-111111111111", siteName: "Gulf of Mexico Platform Beta", status: "NON_COMPLIANT" },
  { siteId: "22222222-2222-2222-2222-222222222222", siteName: "Permian Basin Refinery Gamma", status: "COMPLIANT" }
];

async function tryOrMock(apiCall, mockData) {
  try { return await apiCall(); } catch { console.warn("[Demo Mode] Using mock data"); return mockData; }
}

export async function getExecutiveKPIs() {
  return tryOrMock(async () => (await apiClient.get("/executive/kpis")).data, MOCK_KPIS);
}

export async function getCriticalRisks() {
  return tryOrMock(async () => (await apiClient.get("/executive/critical-risks")).data, MOCK_CRITICAL_RISKS);
}

export async function getComplianceSummary() {
  return tryOrMock(async () => (await apiClient.get("/executive/compliance")).data, MOCK_COMPLIANCE);
}
