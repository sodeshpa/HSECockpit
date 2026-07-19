# Implementation Tasks: Critical Barriers Cockpit

**Feature Branch**: `001-critical-barriers-cockpit`
**Status**: Draft
**Last Updated**: 2026-07-19

---

## Delivery Phases

Tasks are sequenced across four delivery phases within the 8–12 week MVP window:

- **Phase 1 (Weeks 1–3)**: Foundation — AWS infrastructure, data model, ingestion pipeline, validation
- **Phase 2 (Weeks 4–7)**: Core cockpits — barrier monitoring, incident & risk dashboard
- **Phase 3 (Weeks 8–10)**: Advanced capabilities — AI Copilot, executive cockpit
- **Phase 4 (Weeks 11–12)**: Integration, hardening, and pilot validation

---

## Phase 1: AWS Infrastructure & Unified HSE Data Foundation (Epic 2)

### TASK-001 — AWS Infrastructure Setup (CDK)
**Epic**: 2 | **FRs**: FR-020 | **Priority**: P1

- [x] Initialise AWS CDK project (C#) in `infrastructure/HSECockpit.Infra/`
- [x] Create `NetworkStack`: VPC with 2 public + 2 private subnets, NAT Gateway, security groups
- [x] Create `DatabaseStack`: RDS PostgreSQL (Multi-AZ), DynamoDB table (DataQualityLog with TTL)
- [x] Create `ComputeStack`: ECS Fargate cluster, task definition, ECR repository, Lambda functions for ingestion
- [x] Create `FrontendStack`: S3 bucket (static hosting), CloudFront distribution with OAI
- [x] Create `ApiGatewayStack`: HTTP API Gateway, VPC Link to ECS, Cognito User Pool + JWT authorizer
- [x] Create `ObservabilityStack`: CloudWatch log groups, metric alarms, X-Ray tracing config
- [x] Configure VPC endpoints: S3 Gateway, Secrets Manager interface, Bedrock interface
- [x] Configure EventBridge Scheduler for ingestion Lambda triggers
- [x] Set up Secrets Manager entries (placeholder): RDS connection string, Bedrock API config
- [x] Set up Systems Manager Parameter Store: RAG thresholds, risk score weights, ingestion schedule
- [x] Deploy to `dev` environment and validate all resources provisioned

**Done when**: `cdk deploy --all` succeeds in dev account; all stacks create without errors; ECS, RDS, S3, CloudFront reachable.

---

### TASK-002 — CI/CD Pipeline Setup
**Epic**: 2 | **FRs**: FR-020 | **Priority**: P1

- [x] Create CodePipeline with source stage (GitHub/CodeCommit connection)
- [x] Create CodeBuild project for backend: `dotnet build` + `dotnet test` → Docker image → push to ECR
- [x] Create CodeBuild project for frontend: `npm ci` + `npm run build` → S3 sync + CloudFront invalidation
- [x] Create CodeBuild project for infrastructure: `cdk synth` → CloudFormation templates
- [x] Configure staging deployment stage with ECS rolling update
- [x] Configure production deployment stage with manual approval gate and blue/green ECS deploy
- [x] Add WCAG accessibility scan (axe-core) to test stage
- [x] Configure pipeline notifications (SNS) for build failures

**Done when**: Push to main branch triggers pipeline; backend deploys to ECS; frontend deploys to S3/CloudFront; staging environment accessible.

---

### TASK-003 — Data Model & Database Setup (RDS PostgreSQL)
**Epic**: 2 | **FRs**: FR-005, FR-006 | **Priority**: P1

- [x] Create EF Core DbContext with PostgreSQL provider (Npgsql)
- [x] Define entity classes: `Site`, `Asset`, `CriticalBarrier`, `BarrierHealthObservation`, `Incident`, `NearMiss`, `MaintenanceRecord`, `RiskItem`, `ComplianceStatus`
- [x] Apply CHECK constraints for status/severity enums per entity
- [x] Create foreign key relationships and indexes on `site_id`, `asset_id`, `barrier_id`, `observed_at`/`incident_date`
- [x] Generate EF Core migration scripts (version-controlled)
- [x] Create DynamoDB table schema for `DataQualityLog` (partition key: `source_category#ingestion_date`, sort key: `log_id`, TTL: 90 days)
- [x] Seed reference data: at least one pilot site and associated assets
- [x] Configure connection string retrieval from AWS Secrets Manager

**Done when**: EF Core migration applies cleanly to RDS; seed data loads; DynamoDB table created; connection via Secrets Manager verified.

---

### TASK-004 — Ingestion Pipeline — Barrier Inspections (Lambda)
**Epic**: 2 | **FRs**: FR-005, FR-006, FR-007 | **Priority**: P1

- [x] Create `BarrierIngestionFunction` Lambda (.NET 8) in `D4HSE.Ingestion` project
- [x] Configure EventBridge Scheduler trigger (configurable cron, default every 6 hours)
- [x] Read source JSON/CSV files from S3 landing zone bucket (`s3://hse-landing-zone/barriers/`)
- [x] Map source fields to `BarrierHealthObservation` canonical model
- [x] Integrate Data Validator: required fields, site/asset reference check (query RDS), date consistency, RAG status enum check
- [x] Write valid records to RDS `barrier_health_observations` table
- [x] Write validation failures to DynamoDB `DataQualityLog` with business-readable messages
- [x] Configure SQS dead-letter queue for failed Lambda invocations
- [x] Store last-ingestion timestamp in Parameter Store for UI display
- [x] Trigger post-ingestion embedding generation (invoke embedding Lambda)

**Done when**: Upload file to S3 → Lambda fires → valid records in RDS, invalid in DynamoDB; DLQ captures failures; no valid records silently dropped.

---

### TASK-005 — Ingestion Pipeline — Incidents & Near Misses (Lambda)
**Epic**: 2 | **FRs**: FR-005, FR-006, FR-007 | **Priority**: P1

- [x] Create `IncidentIngestionFunction` Lambda (.NET 8)
- [x] Configure EventBridge Scheduler trigger
- [x] Read source files from S3 (`s3://hse-landing-zone/incidents/`)
- [x] Map to `Incident` and `NearMiss` entities with correct severity enum mapping
- [x] Apply Data Validator: required fields (incident_date, site_id, severity), date consistency, status enums
- [x] Write failures to DynamoDB `DataQualityLog`
- [x] Ensure near misses are stored and queryable separately from incidents in RDS
- [x] Configure SQS dead-letter queue

**Done when**: Incidents and near misses ingest into separate RDS tables; validation failures logged in DynamoDB; combined risk queries return both categories.

---

### TASK-006 — Ingestion Pipeline — Asset Maintenance Records (Lambda)
**Epic**: 2 | **FRs**: FR-005, FR-006, FR-007 | **Priority**: P1

- [x] Create `MaintenanceIngestionFunction` Lambda (.NET 8)
- [ ] Configure EventBridge Scheduler trigger
- [x] Read source files from S3 (`s3://hse-landing-zone/maintenance/`)
- [x] Map to `MaintenanceRecord` canonical model
- [x] Apply Data Validator: required fields (asset_id, maintenance_date, activity_type), asset reference check
- [ ] Write failures to DynamoDB `DataQualityLog`
- [ ] Configure SQS dead-letter queue

**Done when**: Maintenance records ingest and are queryable by asset and date in RDS; invalid records logged with clear messages in DynamoDB.

---
### TASK-007 — Data Quality API & UI Indicator
**Epic**: 2 | **FRs**: FR-007, FR-018, FR-019 | **Priority**: P1

- [x] Implement `GET /api/v1/data-quality/summary` endpoint querying DynamoDB for counts of valid/flagged records by category and site
- [x] Return `data_quality_status` on all entity API responses so UI can surface quality labels
- [x] Read last-ingestion timestamp from Parameter Store for freshness display
- [x] Implement quality indicator banner React component (shown when one or more contributing records are flagged)
- [x] Implement partial/stale metric label React component (shown when data category is absent or beyond freshness threshold)

**Done when**: A metric sourced from flagged records displays a visible quality label; a metric with missing data shows a "partial" indicator rather than a zero.

---

### TASK-008 — Authentication & Authorization (Cognito)
**Epic**: 2 | **FRs**: FR-020 | **Priority**: P1

- [x] Configure Cognito User Pool with custom attributes: `role` (hse-manager, hse-data-owner, executive, admin)
- [x] Create Cognito App Client for React SPA (PKCE flow)
- [x] Configure API Gateway JWT authorizer with Cognito User Pool
- [x] Implement React auth context using `@aws-amplify/auth` or `amazon-cognito-identity-js`
- [x] Create protected route wrapper component in React
- [x] Implement role-based access control middleware in .NET 8 API (claims-based)
- [x] Create test users for each role in dev environment

**Done when**: Unauthenticated requests to API return 401; role-restricted endpoints enforce access; React login/logout flow works end-to-end.

---

## Phase 2: Critical Barriers Cockpit & Risk Dashboard (Epics 1 & 3)

### TASK-009 — Barrier Service (.NET 8)
**Epic**: 1 | **FRs**: FR-001, FR-002, FR-003, FR-004 | **Priority**: P1

- [x] Implement `GetBarriersByContext(siteId?, assetId?)` — returns barriers sorted by criticality_rank with current RAG status from RDS
- [x] Implement `GetBarrierTrend(barrierId, periodDays)` — returns ordered health observations for sparkline
- [x] Implement `GetDegradedBarriers(siteId?)` — barriers where latest RAG status is worse than prior observation
- [x] Apply conservative conflict rule: when multiple same-period observations conflict, use highest severity RAG status and set `data_quality_status = CONFLICT`
- [x] Load RAG thresholds from AWS Systems Manager Parameter Store (not hardcoded)
- [x] Expose via `GET /api/v1/barriers`, `/api/v1/barriers/{id}/trend`, `/api/v1/barriers/degraded`

**Done when**: Unit tests confirm correct RAG derivation, degradation detection, and conflict resolution; API returns correct data for representative test cases.

---

### TASK-010 — Barrier Cockpit UI (React)
**Epic**: 1 | **FRs**: FR-001, FR-002, FR-003, FR-004, FR-017 | **Priority**: P1

- [x] Build site/asset cascading filter bar (React Query + Axios)
- [x] Build barrier list component: name, site/asset, RAG badge (shadcn/ui Badge), trend arrow (↑ ↓ →), last assessed date
- [x] Highlight degraded barriers: Amber/Red background + degradation flag icon
- [x] Add trend sparkline per barrier (Recharts LineChart, last N observations, configurable N)
- [x] Add data quality indicator banner (from TASK-007 component)
- [x] Implement empty states:
  - "No barriers found for this site/asset selection"
  - "Data unavailable for this period" (distinct from no-activity)
- [x] Ensure all RAG colour indicators have accessible text labels (not colour-only)
- [x] Keyboard navigation through barrier list
- [x] Deploy to S3/CloudFront via CI/CD pipeline

**Done when**: AC-001-1, AC-001-2, AC-001-3 pass in manual walkthrough; WCAG colour contrast checked on RAG badges.

---

### TASK-011 — Incident Service (.NET 8)
**Epic**: 3 | **FRs**: FR-008, FR-009, FR-011 | **Priority**: P2

- [x] Implement `GetIncidentSummary(siteId?, assetId?, fromDate, toDate)` — total count + breakdown by severity from RDS
- [x] Implement `GetNearMissSummary(siteId?, assetId?, fromDate, toDate)` — count + trend vs prior period
- [x] Implement `GetSiteRiskScore(siteId)` — composite score using configurable weights from Parameter Store; return contributing factor breakdown
- [x] Label score as partial when any contributing data category has quality issues
- [x] Expose via `GET /api/v1/incidents/summary`, `/api/v1/near-misses/summary`, `/api/v1/risk/score/{siteId}`

**Done when**: Unit tests confirm score calculation for known inputs; partial label returned when a contributing category is absent.

---

### TASK-012 — Risk Heatmap Service (.NET 8)
**Epic**: 3 | **FRs**: FR-010, FR-011 | **Priority**: P2

- [x] Implement `GetRiskHeatmap(period)` — returns site-level risk exposure rows with Low/Medium/High/Critical banding
- [x] Risk banding thresholds loaded from Parameter Store (configuration-driven)
- [x] Sites with higher incident severity/frequency appear at higher risk levels than lower-exposure sites
- [x] Expose via `GET /api/v1/risk/heatmap`

**Done when**: AC-003-2 test case passes — a site with injected high-severity incidents scores higher than a low-exposure site.

---

### TASK-013 — Incident & Risk Dashboard UI (React)
**Epic**: 3 | **FRs**: FR-008, FR-009, FR-010, FR-011, FR-017, FR-018, FR-019 | **Priority**: P2

- [x] Build date range picker (shadcn/ui DatePicker) and site/asset filter
- [x] Build incident summary card: total count, severity breakdown (Recharts BarChart)
- [x] Build near-miss summary card: total count, trend vs prior period
- [x] Build risk heatmap grid: sites × risk band (colour-coded with accessible text labels, Recharts HeatMap)
- [x] Build site risk score card: score value, contributing factor breakdown, data quality label
- [x] Drill-down: selecting a site risk score shows supporting incidents, near misses, open risks, and barrier signals
- [x] Keyboard navigation and screen-reader accessible on all interactive elements

**Done when**: AC-003-1, AC-003-2, AC-003-3 pass in manual walkthrough.

---

## Phase 3: AI Copilot & Executive Cockpit (Epics 4 & 5)

### TASK-014 — Vector Index Setup (OpenSearch Serverless)
**Epic**: 4 | **FRs**: FR-012, FR-013 | **Priority**: P2

- [x] Create OpenSearch Serverless collection (`hse-embeddings`) with vector search pipeline
- [x] Define index mapping: `record_id`, `record_type`, `site_id`, `asset_id`, `text_content`, `embedding` (knn_vector, dim 1024, cosine similarity)
- [x] Create embedding generation Lambda: invoke Amazon Bedrock Titan Embeddings v2, write to OpenSearch
- [x] Trigger embedding Lambda post-ingestion (EventBridge rule on successful ingestion completion)
- [x] Implement `SemanticSearch(query, topK)` service method: embed query via Bedrock → k-NN search in OpenSearch → return ranked HSE records
- [x] Configure IAM roles for Lambda → Bedrock and Lambda → OpenSearch access

**Done when**: Semantic search returns relevant barrier/incident records for sample queries; index updates after a new ingestion run.

---

### TASK-015 — Recommendation Rules Engine
**Epic**: 4 | **FRs**: FR-014, FR-015 | **Priority**: P2

- [x] Define predefined recommendation rules in S3 configuration file (JSON):
  - "if barrier = RED → recommend immediate inspection"
  - "if near-miss rate increases > 50% → escalate to site manager"
  - "if overdue maintenance on critical asset → flag for priority scheduling"
- [x] Implement rule matcher service: given retrieved HSE context, identify applicable rules
- [x] Return matched rules as `RecommendedAction[]` with rule label, suggested action text, and triggering condition
- [x] Rules loaded from S3 at service startup with caching; reloadable without code deploy

**Done when**: AC-004-3 sample scenario returns at least one recommended action for a RED barrier; recommended actions are clearly distinct from factual observations.

---

### TASK-016 — AI Copilot API (.NET 8 + Amazon Bedrock)
**Epic**: 4 | **FRs**: FR-012, FR-013, FR-014, FR-015 | **Priority**: P2

- [x] Implement `POST /api/v1/copilot/query` controller
- [x] Query flow:
  1. Embed question via Bedrock Titan Embeddings v2
  2. Semantic search via OpenSearch (top-k = 10)
  3. Build prompt with retrieved context + applicable recommendation rules
  4. Invoke Amazon Bedrock Claude model via AWS SDK
  5. Parse LLM response, extract citations and recommendations
- [x] Response schema: `{ answer, citations: SourceRecord[], recommendedActions: RecommendedAction[], dataScope }`
- [x] Set `dataScope = out_of_scope` and return guidance message when question references data outside MVP categories
- [x] Set `dataScope = partial` when retrieved records have quality flags
- [x] Bedrock model ID and config loaded from Secrets Manager / Parameter Store (never hardcoded)
- [x] Implement prompt injection guardrails: sanitise user input, constrain system prompt

**Done when**: AC-004-1 and AC-004-2 acceptance scenarios produce relevant, cited responses; out-of-scope query returns guidance message.

---

### TASK-017 — AI Copilot UI (React)
**Epic**: 4 | **FRs**: FR-012, FR-013, FR-014, FR-015 | **Priority**: P2

- [x] Build chat-style input panel in cockpit sidebar (shadcn/ui components)
- [x] Display response with: answer text, citation list (source category, site, date), recommended actions section
- [x] Apply "Observed" / "Recommended" badge (shadcn/ui Badge) to respective response sections
- [x] Out-of-scope query: show friendly limitation message and suggest supported alternatives
- [x] Loading state with streaming indicator while Bedrock processes
- [x] Keyboard accessible; focus management after response loads

**Done when**: All three AC-004 scenarios pass; "Observed" and "Recommended" sections are visually and semantically distinct.

---

### TASK-018 — Executive Service & API (.NET 8)
**Epic**: 5 | **FRs**: FR-016, FR-017, FR-018, FR-019 | **Priority**: P3

- [x] Implement `GetExecutiveKPIs()` — Barrier Health Score, Open Critical Risks count, Incident Count MTD, Compliance Rate
- [x] Implement `GetOpenCriticalRisks()` — risk items with severity=CRITICAL and status=OPEN from RDS
- [x] Implement `GetBarrierHealthScore()` — portfolio-wide weighted percentage of GREEN barriers
- [x] Implement `GetComplianceSummary()` — Compliant/Non-Compliant/Unknown counts by site
- [x] All KPIs carry data quality constraint labels where applicable
- [x] Expose `GET /api/v1/executive/kpis`, `/critical-risks`, `/barrier-health`, `/compliance`

**Done when**: AC-005-1, AC-005-2, AC-005-3 acceptance scenarios satisfied with representative test data.

---

### TASK-019 — Executive Cockpit UI (React)
**Epic**: 5 | **FRs**: FR-016, FR-019 | **Priority**: P3

- [x] Build KPI tile row: Barrier Health Score, Open Critical Risks, Incident Count MTD, Compliance Rate
- [x] Build Open Critical Risks list: site, description, days open
- [x] Build barrier health score gauge with colour band (Recharts RadialBarChart — Green/Amber/Red)
- [x] Build compliance summary: Compliant/Non-Compliant/Unknown counts with site drill-down
- [x] Apply data quality constraint labels to impacted KPIs
- [x] View designed for 3-minute review; no deep drill-down required
- [x] Accessible: all colour indicators have text equivalents; keyboard navigable

**Done when**: AC-005-1, AC-005-2, AC-005-3 pass in manual walkthrough; executive can review all KPIs without operational support.

---
## Phase 4: Integration, Hardening & Pilot Validation

### TASK-020 — End-to-End Integration & Cross-View Consistency
**Epic**: All | **FRs**: FR-003, FR-016, FR-018 | **Priority**: P1

- [x] Verify barrier degradation is visible in both operational cockpit (TASK-010) and executive cockpit (TASK-019)
- [x] Verify data quality labels propagate consistently from Lambda ingestion (TASK-004–006) through .NET API to React UI
- [x] Verify site filter applied in Barrier Cockpit is consistent with same filter in Risk Dashboard
- [x] Verify Executive KPIs update when new records are ingested via S3 → Lambda pipeline
- [x] Verify Cognito auth flow works across all views with correct role-based access

**Done when**: Degradation scenario shows amber/red in both cockpit views; quality labels appear at every affected metric; auth enforced correctly.

---

### TASK-021 — Accessibility Audit
**Epic**: All | **Priority**: P1

- [x] Run automated accessibility checks (axe-core, WCAG 2.1 AA) on all UI views via CI/CD
- [x] Verify colour contrast ratios on RAG badges, risk heatmap, compliance indicators
- [x] Verify keyboard navigation for: barrier list, filter controls, risk heatmap, AI Copilot chat, executive tiles
- [x] Add `aria-label` attributes on all icon-only controls and colour-coded indicators
- [x] Document known limitations for manual assistive-technology testing

**Done when**: No automated WCAG AA failures on primary views; all interactive elements reachable by keyboard.

---

### TASK-022 — Security Hardening (AWS)
**Epic**: All | **Priority**: P1

- [x] Audit all EF Core queries for parameterisation (no raw SQL string interpolation)
- [x] Confirm no secrets, API keys, or connection strings in source code or Docker images
- [x] Verify all secrets retrieved from AWS Secrets Manager at runtime
- [x] Confirm environment variables documented in `.env.example` (placeholder values only)
- [x] Input validation on all API endpoints via FluentValidation (reject unexpected types, enforce length limits)
- [x] LLM prompt construction reviewed to prevent prompt injection via user input
- [x] Verify WAF rules on CloudFront distribution (rate limiting, SQL injection patterns)
- [x] Verify RDS security group allows only ECS and Lambda access (port 5432)
- [x] Confirm encryption at rest: RDS (KMS), S3 (SSE-S3), DynamoDB (AWS managed), OpenSearch (KMS)
- [x] Confirm encryption in transit: TLS 1.2+ on all endpoints
- [x] Enable CloudTrail for API audit logging

**Done when**: Security review confirms no exposed secrets; all data encrypted at rest and in transit; WAF active; IAM least-privilege verified.

---

### TASK-023 — Observability & Monitoring
**Epic**: All | **Priority**: P1

- [x] Configure structured JSON logging on all Lambda functions (CloudWatch Logs)
- [x] Configure structured logging on ECS .NET API (Serilog → CloudWatch)
- [x] Create CloudWatch dashboard: API latency (p50/p95/p99), error rates, Lambda invocation counts, RDS connections
- [x] Create CloudWatch Alarms: API error rate > 5%, Lambda DLQ depth > 0, RDS CPU > 80%, ingestion failures
- [x] Enable X-Ray tracing on API Gateway → ECS → RDS path
- [x] Track data quality metrics over time (ingestion success rate, flagged record percentage)
- [x] Configure SNS notifications for critical alarms

**Done when**: CloudWatch dashboard shows all key metrics; alarms fire correctly for simulated failure scenarios; X-Ray traces visible.

---

### TASK-024 — Pilot Validation & Success Criteria Check
**Epic**: All | **FRs**: All | **Priority**: P1

- [x] Upload representative pilot data to S3 landing zone for at least one site covering all three source categories
- [x] Verify end-to-end: S3 upload → Lambda ingestion → RDS storage → API serving → React display
- [x] Validate SC-001: HSE managers identify top at-risk barriers in < 2 minutes
- [x] Validate SC-002: ≥ 90% of records accepted or flagged with readable quality issue
- [x] Validate SC-003: ≥ 95% of degraded barriers highlighted in cockpit
- [x] Validate SC-004: AI Copilot answers sample questions in < 1 minute each (Bedrock response time included)
- [x] Validate SC-006: Executive KPIs reviewable in < 3 minutes
- [x] Validate SC-007: Site risk ordering consistent for ≥ 90% of test cases
- [x] Collect pilot user feedback (SC-005 target: ≥ 80% report improved posture understanding)
- [x] Document any gaps vs. success criteria for post-MVP backlog
- [x] Performance test: API response times < 2s under expected pilot load

**Done when**: All measurable success criteria checked and results documented; gaps logged; system stable under pilot load.

---

## Task Summary

| Task | Phase | Epic | Priority | FRs | AWS Services |
|---|---|---|---|---|---|
| TASK-001 AWS Infrastructure (CDK) | 1 | 2 | P1 | FR-020 | VPC, RDS, DynamoDB, ECS, S3, CloudFront, API GW, Cognito |
| TASK-002 CI/CD Pipeline | 1 | 2 | P1 | FR-020 | CodePipeline, CodeBuild, ECR |
| TASK-003 Data Model & DB Setup | 1 | 2 | P1 | FR-005, FR-006 | RDS PostgreSQL, DynamoDB |
| TASK-004 Ingestion — Barrier Inspections | 1 | 2 | P1 | FR-005–007 | Lambda, EventBridge, S3, SQS |
| TASK-005 Ingestion — Incidents & Near Misses | 1 | 2 | P1 | FR-005–007 | Lambda, EventBridge, S3, SQS |
| TASK-006 Ingestion — Maintenance Records | 1 | 2 | P1 | FR-005–007 | Lambda, EventBridge, S3, SQS |
| TASK-007 Data Quality API & UI | 1 | 2 | P1 | FR-007, FR-018, FR-019 | DynamoDB, Parameter Store |
| TASK-008 Authentication & Auth | 1 | 2 | P1 | FR-020 | Cognito, API Gateway |
| TASK-009 Barrier Service | 2 | 1 | P1 | FR-001–004 | RDS, Parameter Store |
| TASK-010 Barrier Cockpit UI | 2 | 1 | P1 | FR-001–004, FR-017 | S3, CloudFront |
| TASK-011 Incident Service | 2 | 3 | P2 | FR-008, FR-009, FR-011 | RDS, Parameter Store |
| TASK-012 Risk Heatmap Service | 2 | 3 | P2 | FR-010, FR-011 | RDS, Parameter Store |
| TASK-013 Incident & Risk Dashboard UI | 2 | 3 | P2 | FR-008–011, FR-017–019 | S3, CloudFront |
| TASK-014 Vector Index (RAG) | 3 | 4 | P2 | FR-012, FR-013 | OpenSearch Serverless, Bedrock, Lambda |
| TASK-015 Recommendation Rules Engine | 3 | 4 | P2 | FR-014, FR-015 | S3 (config) |
| TASK-016 AI Copilot API | 3 | 4 | P2 | FR-012–015 | Bedrock, OpenSearch, Secrets Manager |
| TASK-017 AI Copilot UI | 3 | 4 | P2 | FR-012–015 | S3, CloudFront |
| TASK-018 Executive Service & API | 3 | 5 | P3 | FR-016–019 | RDS |
| TASK-019 Executive Cockpit UI | 3 | 5 | P3 | FR-016, FR-019 | S3, CloudFront |
| TASK-020 End-to-End Integration | 4 | All | P1 | FR-003, FR-016, FR-018 | All |
| TASK-021 Accessibility Audit | 4 | All | P1 | All | — |
| TASK-022 Security Hardening | 4 | All | P1 | All | WAF, KMS, CloudTrail, Secrets Manager |
| TASK-023 Observability & Monitoring | 4 | All | P1 | All | CloudWatch, X-Ray, SNS |
| TASK-024 Pilot Validation | 4 | All | P1 | All | All |
