# Requirements: Critical Barriers Cockpit

**Feature Branch**: `001-critical-barriers-cockpit`
**Status**: Draft
**Last Updated**: 2026-07-12

---

## Overview

The Critical Barriers Cockpit gives HSE managers a single view of barrier health, unified HSE data, incident and risk dashboards, AI-assisted natural language queries, and an executive KPI cockpit — all within an 8–12 week MVP delivery window.

### Out of Scope (MVP)

- Native mobile applications (web/desktop only)
- Data sources beyond the 3 priority categories
- Real-time streaming ingestion (batch/scheduled imports only)

---

## User Stories

### US-001 — Monitor Critical Barrier Health (P1)

> As an HSE manager, I need a single cockpit showing the most critical safety barriers by site or asset so I can quickly understand which barriers are healthy, degraded, or at risk and prioritize operational action.

**Acceptance Scenarios**:

- **AC-001-1**: Given barrier health data exists for a selected site, when an HSE manager opens the cockpit, then the manager sees the highest-priority barriers with current Green/Amber/Red status and trend direction.
- **AC-001-2**: Given a critical barrier has degraded since its previous assessment, when the cockpit refreshes or the manager reviews the barrier list, then the degraded barrier is clearly highlighted as requiring attention.
- **AC-001-3**: Given multiple sites or assets are available, when the manager filters by a specific site or asset, then the cockpit shows only barriers relevant to that selection.

---

### US-002 — Use Unified HSE Operational Data (P1)

> As an HSE data owner, I need barrier inspections, incidents and near misses, and asset maintenance records brought into one trusted HSE view so that decisions are based on consistent and validated information.

**Acceptance Scenarios**:

- **AC-002-1**: Given valid barrier inspection, incident or near-miss, and maintenance records are available, when the data owner reviews the unified HSE view, then all three categories are represented with their relevant site, asset, date, status, and risk context.
- **AC-002-2**: Given incoming records contain missing required fields or inconsistent values, when data quality validation runs, then those records are flagged with understandable quality issues and do not silently distort cockpit results.
- **AC-002-3**: Given a cockpit metric is shown to an HSE manager, when the manager inspects its supporting context, then the manager can identify the data categories contributing to the metric.

---

### US-003 — Assess Incidents and Site Risk (P2)

> As an HSE manager, I need an incident and risk dashboard that summarizes incidents, near misses, heatmap risk, and site-level risk score so that emerging operational risk is visible before escalation.

**Acceptance Scenarios**:

- **AC-003-1**: Given incident and near-miss records exist for a selected period, when the manager opens the risk dashboard, then the manager sees incident counts, near-miss counts, and risk distribution by site.
- **AC-003-2**: Given one site has materially higher recent incident severity or frequency, when the heatmap is displayed, then that site appears at a higher risk level than lower-exposure sites.
- **AC-003-3**: Given a manager selects a site-level risk score, when supporting details are shown, then the score is explained by the relevant incidents, near misses, open risks, and barrier health signals.

---

### US-004 — Ask HSE Questions in Natural Language (P2)

> As an HSE manager, I need to ask natural language questions about barriers, incidents, and risks so that I can get quick answers and recommended actions without manually searching multiple views.

**Acceptance Scenarios**:

- **AC-004-1**: Given ingested HSE data includes barriers for Refinery X, when a manager asks "Show barriers at risk in Refinery X", then the response lists relevant at-risk barriers with status, site context, and supporting evidence.
- **AC-004-2**: Given incident data exists for the current month, when a manager asks "What incidents occurred this month?", then the response summarizes the incidents and identifies significant patterns or severity concerns.
- **AC-004-3**: Given a response includes an at-risk barrier or open risk, when predefined recommendation rules apply, then the response includes practical recommended actions and distinguishes them from factual record summaries.

---

### US-005 — Review Executive HSE Cockpit (P3)

> As an executive or senior HSE stakeholder, I need a concise KPI cockpit showing barrier health, open critical risks, and compliance status so that I can understand portfolio-level HSE posture quickly.

**Acceptance Scenarios**:

- **AC-005-1**: Given HSE data exists across one or more sites, when an executive opens the cockpit, then the executive sees current KPI values, open critical risks, overall barrier health score, and compliance status summary.
- **AC-005-2**: Given a site has open critical risks or degraded barriers, when portfolio KPIs are calculated, then the executive cockpit reflects those issues in the relevant risk and health indicators.
- **AC-005-3**: Given compliance status is available for monitored sites, when the executive reviews the summary, then compliant, non-compliant, and unknown statuses are clearly distinguishable.

---

## Functional Requirements

| ID | Requirement | User Story |
|---|---|---|
| FR-001 | System MUST provide a Critical Barriers Cockpit that lists priority barriers by selected site or asset. | US-001 |
| FR-002 | System MUST display each monitored barrier with Green, Amber, or Red health status based on available barrier condition and operational risk signals. | US-001 |
| FR-003 | System MUST highlight barrier degradation when a barrier's status worsens or its health trend declines compared with prior available observations. | US-001 |
| FR-004 | System MUST show barrier health trend direction for monitored barriers over the available historical period. | US-001 |
| FR-005 | System MUST unify MVP HSE data from barrier inspections, incidents and near misses, and asset maintenance records. | US-002 |
| FR-006 | System MUST validate incoming HSE records for required fields, recognizable site or asset context, date consistency, and status value consistency. | US-002 |
| FR-007 | System MUST flag data quality issues in a way that business users can understand and factor into decisions. | US-002 |
| FR-008 | System MUST provide incident summaries for selected sites, assets, and time periods. | US-003 |
| FR-009 | System MUST track and summarize near misses separately from incidents while allowing combined risk interpretation. | US-003 |
| FR-010 | System MUST present a risk heatmap that shows relative risk exposure by site or asset. | US-003 |
| FR-011 | System MUST calculate and display a site-level risk score using incident, near-miss, open risk, and barrier health signals available in the MVP data scope. | US-003 |
| FR-012 | Users MUST be able to ask natural language questions about monitored barriers, incidents, near misses, risks, and site-level HSE status. | US-004 |
| FR-013 | System MUST answer natural language questions using only ingested MVP HSE data and must indicate when the requested information is unavailable. | US-004 |
| FR-014 | System MUST provide recommended actions for supported HSE situations based on predefined business rules. | US-004 |
| FR-015 | System MUST distinguish recommended actions from factual summaries so users can tell what is observed versus advised. | US-004 |
| FR-016 | System MUST provide an executive cockpit containing KPI dashboard, open critical risks, barrier health score, and compliance status summary. | US-005 |
| FR-017 | System MUST allow users to filter operational and executive views by site or asset where that context is available. | US-001, US-003, US-005 |
| FR-018 | System MUST preserve enough source context for users to understand which data categories support displayed metrics and answers. | US-002, US-004 |
| FR-019 | System MUST clearly label metrics that are partial, stale, or constrained by unresolved data quality issues. | US-002 |
| FR-020 | System MUST support an MVP delivery scope focused on the Critical Barriers Cockpit, unified data layer, incident and risk dashboard, AI-assisted HSE queries, and executive cockpit. | All |

---

## Key Entities

| Entity | Description |
|---|---|
| **Site** | An operational location or facility where HSE barriers, incidents, near misses, assets, risks, and compliance status are monitored. |
| **Asset** | Equipment, unit, or operational area associated with safety barriers, maintenance records, incidents, and risk exposure. |
| **Critical Barrier** | A safety control or protective measure monitored for health status, degradation, trend, and operational importance. |
| **Barrier Health Observation** | A dated assessment or signal describing the condition, status, trend, or quality of a critical barrier. |
| **Incident** | A recorded HSE event with date, site or asset context, severity, description, and risk relevance. |
| **Near Miss** | A recorded event that did not result in an incident but indicates potential risk exposure. |
| **Maintenance Record** | A record of asset work, condition, or maintenance activity that may influence barrier health or operational risk. |
| **Risk Item** | An identified risk associated with a site, asset, barrier, incident pattern, or compliance concern. |
| **Recommended Action** | A rule-based suggestion linked to an observed barrier, risk, incident, or compliance condition. |
| **Compliance Status** | A summary indicator showing whether monitored sites or assets satisfy expected HSE compliance conditions within the MVP scope. |

---

## Edge Cases

- If one or more priority data categories are unavailable, the cockpit must clearly identify missing data and avoid presenting incomplete metrics as fully reliable.
- If barrier status inputs conflict across records, the cockpit must flag the conflict and use a conservative status for safety-critical summaries.
- If a selected site or asset has no records for the current period, the cockpit must show an empty state that distinguishes "no activity" from "data unavailable".
- If natural language questions request data outside the MVP data scope, the response must state the limitation and offer the closest supported view or question.
- If data quality issues affect KPI or risk calculations, impacted metrics must be labelled as partial or data-quality constrained.
- If a barrier changes status from Green to Amber or Red, the degradation must be visible in both operational and executive summaries.

---

## Success Criteria

| ID | Criterion |
|---|---|
| SC-001 | HSE managers can identify top at-risk barriers for a selected site or asset in under 2 minutes during user validation. |
| SC-002 | At least 90% of representative MVP records from the three priority data categories are either accepted as valid or flagged with a clear business-readable quality issue. |
| SC-003 | At least 95% of degraded barriers in the validation dataset are visibly highlighted in the operational cockpit. |
| SC-004 | HSE managers can answer sample questions "Show barriers at risk in Refinery X" and "What incidents occurred this month?" with relevant results in under 1 minute each. |
| SC-005 | At least 80% of pilot HSE users report the cockpit improves their ability to understand current barrier and risk posture. |
| SC-006 | Executive stakeholders can review barrier health score, open critical risks, compliance status, and HSE KPIs in under 3 minutes without operational support. |
| SC-007 | Risk dashboard validation shows consistent site risk ordering for at least 90% of test cases where incident severity, near-miss frequency, and barrier health differences are known. |
| SC-008 | MVP supports review of barrier, incident, near-miss, maintenance, risk, action recommendation, and compliance information for at least one pilot site or asset group within the 8–12 week delivery scope. |

---

## Assumptions

- MVP focuses on one or more pilot sites or asset groups rather than enterprise-wide rollout.
- The three priority data categories are barrier inspections, incidents and near misses, and asset maintenance records.
- Data freshness expectations are appropriate for operational decision support; visible freshness or quality labelling is required but continuous streaming is not.
- Green, Amber, and Red status thresholds will be defined by HSE business stakeholders before implementation planning completes.
- Recommended actions are based on predefined HSE rules for the MVP; no autonomous decision-making is required.
- Compliance status summary is limited to compliance indicators available in the MVP data scope.
- Representative data samples will be provided from existing source systems or proposal artifacts for validation.
