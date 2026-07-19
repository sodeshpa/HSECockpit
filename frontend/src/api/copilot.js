import apiClient from "./client";

const MOCK_RESPONSE = {
  answer: "Based on the available HSE data, there are 2 barriers currently at risk across your sites:\n\n1. **Gas Detection Array B** (North Sea Platform Alpha) — RED status, showing progressive degradation from GREEN over the past 6 weeks.\n2. **Subsea BOP** (Gulf of Mexico Platform Beta) — RED status, degraded from GREEN with condition score dropping to 32%.\n\nBoth barriers have recent observations confirming the degradation trend.",
  citations: [
    { recordId: "obs-001", recordType: "barrier_observation", siteId: "a1b2c3d4-e5f6-7890-abcd-ef1234567890", textContent: "Gas Detection Array B observation: RED status, condition score 28", relevanceScore: 0.92 },
    { recordId: "obs-002", recordType: "barrier_observation", siteId: "11111111-1111-1111-1111-111111111111", textContent: "Subsea BOP observation: RED status, condition score 32", relevanceScore: 0.89 }
  ],
  recommendedActions: [
    { ruleId: "rule-001", label: "Immediate Inspection", action: "Schedule immediate physical inspection for RED-status barriers within 48 hours", triggeringCondition: "Barrier status is RED" },
    { ruleId: "rule-002", label: "Escalation Required", action: "Escalate to site manager for review and action planning", triggeringCondition: "Multiple barriers in degraded state at same site" }
  ],
  dataScope: "full"
};

async function tryOrMock(apiCall, mockData) {
  try { return await apiCall(); } catch (err) { console.warn("[Demo Mode] Using mock copilot data"); return mockData; }
}

export async function queryCopilot(query) {
  return tryOrMock(async () => {
    const response = await apiClient.post("/copilot/query", { query });
    return response.data;
  }, MOCK_RESPONSE);
}
