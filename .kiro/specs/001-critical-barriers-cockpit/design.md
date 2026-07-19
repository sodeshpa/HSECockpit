# Design: Critical Barriers Cockpit

**Feature Branch**: `001-critical-barriers-cockpit`
**Status**: Draft
**Last Updated**: 2026-07-19

---

## 1. Architecture Overview

The D4HSE MVP follows a cloud-native layered architecture hosted entirely on AWS, with clear separation between data ingestion, domain logic, API, and presentation layers. All components are web-accessible; no native mobile client is in scope.

```
┌─────────────────────────────────────────────────────────────────┐
│                    Presentation Layer (AWS)                      │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │  React SPA → S3 Static Hosting + CloudFront CDN           │ │
│  │  ┌──────────────┐ ┌──────────────┐ ┌───────────────────┐  │ │
│  │  │  Barrier     │ │  Incident &  │ │  Executive        │  │ │
│  │  │  Cockpit UI  │ │  Risk UI     │ │  Cockpit UI       │  │ │
│  │  └──────────────┘ └──────────────┘ └───────────────────┘  │ │
│  │  ┌─────────────────────────────────────────────────────┐   │ │
│  │  │             AI Copilot Chat UI                      │   │ │
│  │  └─────────────────────────────────────────────────────┘   │ │
│  └────────────────────────────────────────────────────────────┘ │
└───────────────────────────────┬─────────────────────────────────┘
                                │ HTTPS (API Gateway)
┌───────────────────────────────▼─────────────────────────────────┐
│              API Layer (AWS ECS Fargate + API Gateway)           │
│  ┌──────────────┐  ┌──────────────┐  ┌───────────────────┐     │
│  │  Barrier API │  │  Risk API    │  │  Executive API    │     │
│  └──────────────┘  └──────────────┘  └───────────────────┘     │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │               AI Copilot API                            │    │
│  └─────────────────────────────────────────────────────────┘    │
│  (.NET 8 Web API running on ECS Fargate)                        │
└───────────────────────────────┬─────────────────────────────────┘
                                │
┌───────────────────────────────▼─────────────────────────────────┐
│                  Domain / Service Layer                          │
│  ┌──────────────┐  ┌──────────────┐  ┌───────────────────┐     │
│  │  Barrier     │  │  Risk Score  │  │  Compliance       │     │
│  │  Service     │  │  Service     │  │  Service          │     │
│  └──────────────┘  └──────────────┘  └───────────────────┘     │
│  ┌──────────────┐  ┌─────────────────────────────────────┐     │
│  │  Incident    │  │  RAG / Recommendation Engine        │     │
│  │  Service     │  │  (Amazon Bedrock + OpenSearch)      │     │
│  └──────────────┘  └─────────────────────────────────────┘     │
└───────────────────────────────┬─────────────────────────────────┘
                                │
┌───────────────────────────────▼─────────────────────────────────┐
│                      Data Layer (AWS)                            │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  Amazon RDS (PostgreSQL) — Unified HSE Relational Store  │   │
│  └──────────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  Amazon OpenSearch Serverless — Vector Index (RAG)        │   │
│  └──────────────────────────────────────────────────────────┘   │
│  ┌──────────────┐  ┌──────────────┐  ┌───────────────────┐     │
│  │  Ingestion   │  │  Data        │  │  Audit / Quality  │     │
│  │  (Lambda +   │  │  Validator   │  │  Log (DynamoDB)   │     │
│  │  EventBridge)│  │  (Lambda)    │  │                   │     │
│  └──────────────┘  └──────────────┘  └───────────────────┘     │
└───────────────────────────────┬─────────────────────────────────┘
                                │ Batch / Scheduled Import (EventBridge Scheduler)
┌───────────────────────────────▼─────────────────────────────────┐
│                    Source Systems (S3 Landing Zone)              │
│  ┌──────────────┐  ┌──────────────┐  ┌───────────────────┐     │
│  │  Barrier     │  │  Incident /  │  │  Asset            │     │
│  │  Inspections │  │  Near Miss   │  │  Maintenance      │     │
│  └──────────────┘  └──────────────┘  └───────────────────┘     │
└─────────────────────────────────────────────────────────────────┘
```

---

## 1b. Technology Stack

| Layer | Technology | AWS Service | Notes |
|---|---|---|---|
| **Frontend** | React 18 + Vite | S3 + CloudFront | Static SPA hosting with global CDN |
| **UI Components** | Tailwind CSS + shadcn/ui | — | Utility-first styling, accessible component primitives |
| **Charts** | Recharts | — | Composable charts for trend sparklines, heatmap, gauges |
| **State / Data Fetching** | TanStack Query (React Query) | — | Server-state caching, loading/error states |
| **Routing** | React Router v6 | — | Client-side navigation between cockpit views |
| **HTTP Client** | Axios | — | Typed API service layer |
| **API Gateway** | — | Amazon API Gateway (HTTP API) | Request routing, throttling, CORS, auth integration |
| **Backend** | .NET 8 Web API | ECS Fargate | Containerised RESTful API, auto-scaling |
| **ORM** | Entity Framework Core 8 | — | Code-first migrations, LINQ queries |
| **Database** | PostgreSQL 16 | Amazon RDS (Multi-AZ) | Managed relational DB with automated backups |
| **Vector Store** | OpenSearch k-NN | Amazon OpenSearch Serverless | Vector embeddings for RAG semantic search |
| **AI/LLM** | Claude / Titan | Amazon Bedrock | Managed LLM for natural language HSE queries |
| **Embeddings** | Titan Embeddings v2 | Amazon Bedrock | Text embedding generation for vector index |
| **Ingestion** | .NET Lambda functions | AWS Lambda | Scheduled batch processing of source files |
| **Scheduler** | — | Amazon EventBridge Scheduler | Cron-based trigger for ingestion pipeline |
| **Source Files** | — | Amazon S3 | Landing zone for source system exports |
| **Audit Log** | — | Amazon DynamoDB | High-throughput write for data quality events |
| **Secrets** | — | AWS Secrets Manager | Connection strings, API keys, LLM credentials |
| **Auth** | — | Amazon Cognito | User pools, JWT tokens, role-based access |
| **Monitoring** | — | Amazon CloudWatch | Logs, metrics, alarms, dashboards |
| **IaC** | — | AWS CDK (C#) | Infrastructure as code, same language as backend |
| **CI/CD** | — | AWS CodePipeline + CodeBuild | Automated build, test, deploy |
| **Validation** | FluentValidation | — | Business-readable validation messages for data quality |
| **API Docs** | Swagger / Swashbuckle | — | Auto-generated OpenAPI spec |

### AWS Architecture Diagram (Logical)

```
┌─────────────────────────────────────────────────────────────────────┐
│                          AWS Account                                 │
│                                                                     │
│  ┌─────────────┐      ┌──────────────┐      ┌────────────────┐     │
│  │ CloudFront  │─────▶│  S3 Bucket   │      │   Cognito      │     │
│  │ (CDN)       │      │  (React SPA) │      │   User Pool    │     │
│  └──────┬──────┘      └──────────────┘      └───────┬────────┘     │
│         │                                           │ JWT           │
│         │ /api/*                                    │               │
│  ┌──────▼──────────────────────────────────────────▼──────────┐    │
│  │              Amazon API Gateway (HTTP API)                   │    │
│  │              - Route: /api/v1/* → ECS                       │    │
│  │              - Cognito JWT Authorizer                        │    │
│  └──────────────────────────┬──────────────────────────────────┘    │
│                             │                                       │
│  ┌──────────────────────────▼──────────────────────────────────┐    │
│  │          ECS Fargate Cluster (Private Subnet)               │    │
│  │   ┌─────────────────────────────────────────────────────┐   │    │
│  │   │  .NET 8 Web API Container (auto-scaling 1–4 tasks)  │   │    │
│  │   └────────┬────────────────┬───────────────┬───────────┘   │    │
│  └────────────┼────────────────┼───────────────┼───────────────┘    │
│               │                │               │                    │
│  ┌────────────▼────┐  ┌───────▼────────┐  ┌──▼──────────────┐     │
│  │  Amazon RDS     │  │  OpenSearch    │  │  Amazon Bedrock  │     │
│  │  (PostgreSQL)   │  │  Serverless   │  │  (Claude/Titan)  │     │
│  │  Multi-AZ       │  │  (k-NN index) │  │                  │     │
│  └─────────────────┘  └───────────────┘  └──────────────────┘     │
│                                                                     │
│  ┌─────────────────────────────────────────────────────────────┐    │
│  │            Ingestion Pipeline                                │    │
│  │  EventBridge ──▶ Lambda (Ingestion) ──▶ Lambda (Validator)  │    │
│  │       │                                      │               │    │
│  │       ▼                                      ▼               │    │
│  │  S3 (Landing Zone)              DynamoDB (Quality Log)       │    │
│  └─────────────────────────────────────────────────────────────┘    │
│                                                                     │
│  ┌───────────────────┐  ┌───────────────────┐                      │
│  │  Secrets Manager  │  │  CloudWatch       │                      │
│  │  (credentials)    │  │  (logs, metrics)  │                      │
│  └───────────────────┘  └───────────────────┘                      │
└─────────────────────────────────────────────────────────────────────┘
```

### Folder Structure

```
HSECockpit/
├── infrastructure/
│   ├── HSECockpit.Infra/            # AWS CDK (C#) infrastructure project
│   │   ├── Stacks/
│   │   │   ├── NetworkStack.cs      # VPC, subnets, security groups
│   │   │   ├── DatabaseStack.cs     # RDS PostgreSQL, DynamoDB
│   │   │   ├── ComputeStack.cs      # ECS Fargate, Lambda functions
│   │   │   ├── FrontendStack.cs     # S3, CloudFront
│   │   │   ├── ApiGatewayStack.cs   # API Gateway, Cognito
│   │   │   └── ObservabilityStack.cs # CloudWatch, alarms
│   │   └── Program.cs
│   └── HSECockpit.Infra.sln
├── backend/
│   ├── D4HSE.sln
│   ├── D4HSE.Api/                   # .NET 8 Web API (ECS Fargate)
│   │   ├── Controllers/
│   │   ├── Models/                  # DTOs / response models
│   │   ├── Middleware/              # Auth, error handling, correlation
│   │   ├── Dockerfile
│   │   └── Program.cs
│   ├── D4HSE.Core/                  # Domain models, interfaces
│   │   ├── Entities/
│   │   └── Interfaces/
│   ├── D4HSE.Infrastructure/        # EF Core, repositories, AWS SDK
│   │   ├── Data/
│   │   ├── Repositories/
│   │   ├── Seed/
│   │   └── AwsServices/            # Bedrock, OpenSearch clients
│   ├── D4HSE.Services/              # Business logic, risk scoring
│   │   └── Services/
│   └── D4HSE.Ingestion/             # Lambda functions for data ingestion
│       ├── Functions/
│       │   ├── BarrierIngestionFunction.cs
│       │   ├── IncidentIngestionFunction.cs
│       │   └── MaintenanceIngestionFunction.cs
│       ├── Validators/
│       └── aws-lambda-tools-defaults.json
└── frontend/
    ├── src/
    │   ├── api/                     # Axios service layer
    │   ├── components/              # Shared UI components
    │   ├── pages/                   # Route-level page components
    │   ├── hooks/                   # Custom React Query hooks
    │   ├── auth/                    # Cognito auth context & guards
    │   └── main.jsx
    ├── index.html
    ├── vite.config.js
    └── package.json
```

---

## 2. Component Design

### 2.1 Unified HSE Data Layer (Epic 2 — foundation)

**Ingestion Pipeline (AWS Lambda + EventBridge)**
- Amazon EventBridge Scheduler triggers Lambda functions on a configurable cron schedule (e.g., every 6 hours)
- Lambda functions read source files from the S3 landing zone bucket
- Three separate Lambda functions for each source category: barrier inspections, incidents/near misses, asset maintenance records
- Each function normalises source records into the canonical HSE data model
- Passes each record through the Data Validator before persisting to RDS
- Dead-letter queue (SQS) captures failed Lambda invocations for retry

**Data Validator (embedded in Lambda)**
- Checks required fields per record type (see field tables below)
- Validates site/asset references exist in the RDS reference tables
- Checks date consistency (observation date ≤ today, no future-dated records)
- Checks status values against allowed enumerations
- On failure: writes a quality issue record to DynamoDB (Audit/Quality Log); does not persist the raw record to RDS
- Returns a business-readable quality message for each failed check

**HSE Unified Data Store (Amazon RDS PostgreSQL)**
- Relational tables for structured querying (barriers, incidents, near misses, maintenance, risk items, compliance)
- All records carry `source_category`, `site_id`, `asset_id`, `ingested_at`, and `data_quality_status` columns
- Multi-AZ deployment for high availability; automated daily backups with 7-day retention
- Read replica available for reporting queries if load warrants it

**Vector Index (Amazon OpenSearch Serverless)**
- k-NN vector index over ingested text fields to support RAG queries in the AI Copilot
- Embeddings generated via Amazon Bedrock Titan Embeddings v2 model
- Re-indexed after each batch ingestion run via a post-ingestion Lambda trigger
- Collection configured with vector search pipeline for cosine similarity

**Audit / Quality Log (Amazon DynamoDB)**
- Stores every validation failure with: record identifier, field name, issue description, ingestion timestamp
- Partition key: `source_category#ingestion_date`; sort key: `log_id`
- TTL configured (90 days) to auto-expire old quality log entries
- Surfaced in the UI as data quality indicators on affected metrics via API query

---

### 2.2 Critical Barriers Cockpit (Epic 1)

**Barrier Service (.NET 8)**
- `GetBarriersByContext(siteId?, assetId?)` — returns ranked barrier list with current RAG status
- `GetBarrierTrend(barrierId, periodDays)` — returns time-series health observations
- `GetDegradedBarriers(siteId?)` — returns barriers where latest status is worse than previous observation
- RAG status derivation: threshold values (Green/Amber/Red cutoffs) stored in AWS Systems Manager Parameter Store, configurable by HSE stakeholders without redeployment

**Barrier Cockpit UI (React)**
- Filter bar: site selector, asset selector (cascading)
- Barrier list: sorted by criticality (descending), shows name, site/asset, RAG badge, trend arrow (↑ ↓ →), last assessed date
- Degraded barriers row: highlighted with Amber/Red background + degradation flag icon
- Trend sparkline per barrier (last N observations via Recharts)
- Data quality indicator: banner shown when one or more contributing records have quality issues
- Empty states: "No barriers found for this selection" vs "Data unavailable for this period" — distinct messages

**RAG Status Rules**
- Green: barrier condition meets or exceeds threshold; no open degradation flags
- Amber: barrier condition below threshold OR one open degradation flag within assessment window
- Red: barrier condition critically below threshold OR multiple open degradation flags OR overdue inspection
- Status conflicts across records: conservative rule applied (highest severity status wins); conflict flagged in UI

---

### 2.3 Incident & Risk Dashboard (Epic 3)

**Incident Service (.NET 8)**
- `GetIncidentSummary(siteId?, assetId?, fromDate, toDate)` — counts by severity, type
- `GetNearMissSummary(siteId?, assetId?, fromDate, toDate)` — counts, patterns
- `GetRiskHeatmap(period)` — returns site-level risk exposure grid
- `GetSiteRiskScore(siteId)` — composite score from incident severity, near-miss frequency, open risk items, barrier health

**Risk Score Formula (MVP)**
```
RiskScore(site) =
  w1 × NormalisedIncidentSeverity
  + w2 × NormalisedNearMissFrequency
  + w3 × NormalisedOpenRiskCount
  + w4 × (1 − NormalisedBarrierHealthScore)
```
- Weights (w1–w4) stored in AWS Systems Manager Parameter Store; default equal weighting for MVP
- Score range: 0 (lowest risk) to 100 (highest risk)
- Score labelled as partial/data-quality constrained when any contributing data category has quality issues

**Risk Dashboard UI (React)**
- Date range picker, site/asset filter
- Incident summary card: total count, breakdown by severity
- Near-miss summary card: total count, trend vs. prior period
- Risk heatmap: grid of sites × risk level (colour-coded Low/Medium/High/Critical via Recharts)
- Site risk score card: score value, contributing factor breakdown, data quality label where applicable
- Drill-down: selecting a site risk score shows the supporting incidents, near misses, open risks, and barrier signals

---

### 2.4 AI Copilot for HSE (Epic 4)

**RAG / Recommendation Engine (Amazon Bedrock + OpenSearch)**
- Query flow:
  1. User submits natural language question via AI Copilot UI
  2. Question embedded using Amazon Bedrock Titan Embeddings v2
  3. k-NN search against OpenSearch Serverless vector index retrieves top-k relevant HSE records
  4. Prompt constructed with retrieved context + predefined recommendation rules
  5. Amazon Bedrock Claude model generates response grounded in retrieved records
  6. Response returned with cited source records and any applicable recommended actions

- Recommended actions are rule-based (predefined in configuration stored in S3); the LLM does not invent actions
- Responses clearly label: **Observed** (factual from records) vs **Recommended** (rule-based action)
- When queried data is outside MVP scope: response states the limitation and suggests the closest supported question
- Bedrock model invocation via AWS SDK; API key managed in Secrets Manager

**AI Copilot UI (React)**
- Chat-style input panel available from the cockpit sidebar
- Response area shows: answer text, cited record references (source category, site, date), recommended actions (if applicable)
- "Observed / Recommended" badge on each response section
- Unsupported query: friendly message stating what is not available and what can be asked instead
- Loading state with streaming response indicator

**Sample Supported Queries**
| Query | Expected behaviour |
|---|---|
| "Show barriers at risk in Refinery X" | Lists at-risk barriers for that site with status and evidence |
| "What incidents occurred this month?" | Summarises incidents for current calendar month, highlights severity patterns |
| "Are there near misses linked to Barrier Y?" | Retrieves near misses referencing the same asset as Barrier Y |
| "What actions are recommended for degraded barriers?" | Returns rule-based recommended actions for Amber/Red barriers |

---

### 2.5 Executive Cockpit (Epic 5)

**Executive API / Service (.NET 8)**
- Aggregates from Barrier Service, Incident Service, Risk Score Service, and Compliance Service
- `GetExecutiveKPIs()` — returns portfolio-level KPI snapshot
- `GetOpenCriticalRisks()` — risks with severity = Critical that are unresolved
- `GetBarrierHealthScore()` — portfolio-wide weighted barrier health percentage
- `GetComplianceSummary()` — counts of sites in Compliant / Non-Compliant / Unknown status

**Executive Cockpit UI (React)**
- KPI tiles: Barrier Health Score, Open Critical Risks, Incident Count (MTD), Compliance Rate
- Open Critical Risks list: site, risk description, age (days open)
- Barrier health score gauge/indicator with colour band (Recharts gauge)
- Compliance summary: Compliant / Non-Compliant / Unknown counts with site drill-down
- Designed for 3-minute review: no deep drill-down required on this view
- All KPIs labelled when impacted by data quality constraints

---

## 3. Data Model

### 3.1 Core Tables (Amazon RDS PostgreSQL)

**sites**
```sql
CREATE TABLE sites (
    site_id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_name        VARCHAR(255) NOT NULL,
    region           VARCHAR(100),
    created_at       TIMESTAMPTZ DEFAULT NOW()
);
```

**assets**
```sql
CREATE TABLE assets (
    asset_id         UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id          UUID NOT NULL REFERENCES sites(site_id),
    asset_name       VARCHAR(255) NOT NULL,
    asset_type       VARCHAR(100),
    created_at       TIMESTAMPTZ DEFAULT NOW()
);
CREATE INDEX idx_assets_site ON assets(site_id);
```

**critical_barriers**
```sql
CREATE TABLE critical_barriers (
    barrier_id       UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id          UUID NOT NULL REFERENCES sites(site_id),
    asset_id         UUID REFERENCES assets(asset_id),
    barrier_name     VARCHAR(255) NOT NULL,
    barrier_type     VARCHAR(100),
    criticality_rank INTEGER NOT NULL,
    is_active        BOOLEAN DEFAULT TRUE,
    created_at       TIMESTAMPTZ DEFAULT NOW()
);
CREATE INDEX idx_barriers_site ON critical_barriers(site_id);
CREATE INDEX idx_barriers_asset ON critical_barriers(asset_id);
```

**barrier_health_observations**
```sql
CREATE TABLE barrier_health_observations (
    observation_id      UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    barrier_id          UUID NOT NULL REFERENCES critical_barriers(barrier_id),
    observed_at         DATE NOT NULL,
    rag_status          VARCHAR(10) NOT NULL CHECK (rag_status IN ('GREEN','AMBER','RED')),
    condition_score     DECIMAL(5,2),
    notes               TEXT,
    source_category     VARCHAR(50) NOT NULL,
    data_quality_status VARCHAR(20) DEFAULT 'VALID' CHECK (data_quality_status IN ('VALID','FLAGGED','CONFLICT')),
    ingested_at         TIMESTAMPTZ DEFAULT NOW()
);
CREATE INDEX idx_observations_barrier ON barrier_health_observations(barrier_id, observed_at DESC);
```

**incidents**
```sql
CREATE TABLE incidents (
    incident_id         UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id             UUID NOT NULL REFERENCES sites(site_id),
    asset_id            UUID REFERENCES assets(asset_id),
    incident_date       DATE NOT NULL,
    severity            VARCHAR(20) NOT NULL CHECK (severity IN ('LOW','MEDIUM','HIGH','CRITICAL')),
    incident_type       VARCHAR(100),
    description         TEXT,
    source_category     VARCHAR(50) NOT NULL,
    data_quality_status VARCHAR(20) DEFAULT 'VALID' CHECK (data_quality_status IN ('VALID','FLAGGED')),
    ingested_at         TIMESTAMPTZ DEFAULT NOW()
);
CREATE INDEX idx_incidents_site_date ON incidents(site_id, incident_date DESC);
```

**near_misses**
```sql
CREATE TABLE near_misses (
    near_miss_id        UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id             UUID NOT NULL REFERENCES sites(site_id),
    asset_id            UUID REFERENCES assets(asset_id),
    event_date          DATE NOT NULL,
    potential_severity  VARCHAR(20) CHECK (potential_severity IN ('LOW','MEDIUM','HIGH','CRITICAL')),
    description         TEXT,
    source_category     VARCHAR(50) NOT NULL,
    data_quality_status VARCHAR(20) DEFAULT 'VALID' CHECK (data_quality_status IN ('VALID','FLAGGED')),
    ingested_at         TIMESTAMPTZ DEFAULT NOW()
);
CREATE INDEX idx_near_misses_site_date ON near_misses(site_id, event_date DESC);
```

**maintenance_records**
```sql
CREATE TABLE maintenance_records (
    maintenance_id      UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    asset_id            UUID NOT NULL REFERENCES assets(asset_id),
    site_id             UUID NOT NULL REFERENCES sites(site_id),
    maintenance_date    DATE NOT NULL,
    activity_type       VARCHAR(100),
    outcome             VARCHAR(100),
    notes               TEXT,
    source_category     VARCHAR(50) NOT NULL,
    data_quality_status VARCHAR(20) DEFAULT 'VALID' CHECK (data_quality_status IN ('VALID','FLAGGED')),
    ingested_at         TIMESTAMPTZ DEFAULT NOW()
);
CREATE INDEX idx_maintenance_asset_date ON maintenance_records(asset_id, maintenance_date DESC);
```

**risk_items**
```sql
CREATE TABLE risk_items (
    risk_id             UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id             UUID NOT NULL REFERENCES sites(site_id),
    asset_id            UUID REFERENCES assets(asset_id),
    barrier_id          UUID REFERENCES critical_barriers(barrier_id),
    risk_description    TEXT NOT NULL,
    severity            VARCHAR(20) NOT NULL CHECK (severity IN ('LOW','MEDIUM','HIGH','CRITICAL')),
    status              VARCHAR(20) DEFAULT 'OPEN' CHECK (status IN ('OPEN','CLOSED','MONITORING')),
    identified_at       DATE,
    resolved_at         DATE
);
CREATE INDEX idx_risks_site_status ON risk_items(site_id, status);
```

**compliance_status**
```sql
CREATE TABLE compliance_status (
    compliance_id       UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    site_id             UUID NOT NULL REFERENCES sites(site_id),
    period_start        DATE NOT NULL,
    period_end          DATE NOT NULL,
    status              VARCHAR(20) NOT NULL CHECK (status IN ('COMPLIANT','NON_COMPLIANT','UNKNOWN')),
    notes               TEXT,
    assessed_at         TIMESTAMPTZ DEFAULT NOW()
);
CREATE INDEX idx_compliance_site ON compliance_status(site_id, period_end DESC);
```

### 3.2 Audit Log (Amazon DynamoDB)

**data_quality_log**
```
Table: DataQualityLog
Partition Key: source_category#ingestion_date (String)
Sort Key: log_id (String — UUID)

Attributes:
  - source_record_ref   (String)
  - field_name          (String)
  - issue_description   (String)
  - ingestion_timestamp (String — ISO 8601)
  - ttl                 (Number — epoch seconds, 90-day expiry)
```

### 3.3 Vector Index (Amazon OpenSearch Serverless)

**hse_embeddings collection**
```json
{
  "mappings": {
    "properties": {
      "record_id": { "type": "keyword" },
      "record_type": { "type": "keyword" },
      "site_id": { "type": "keyword" },
      "asset_id": { "type": "keyword" },
      "text_content": { "type": "text" },
      "embedding": {
        "type": "knn_vector",
        "dimension": 1024,
        "method": {
          "name": "hnsw",
          "engine": "nmslib",
          "space_type": "cosinesimil"
        }
      },
      "ingested_at": { "type": "date" }
    }
  }
}
```

---

## 4. API Design

All API endpoints are exposed via Amazon API Gateway (HTTP API) with Cognito JWT authorizer. The .NET 8 Web API runs on ECS Fargate behind a VPC Link.

### 4.1 Barrier Endpoints

```
GET  /api/v1/barriers
     ?siteId=&assetId=&status=
     → BarrierSummary[]

GET  /api/v1/barriers/{barrierId}/trend
     ?periodDays=30
     → BarrierTrendPoint[]

GET  /api/v1/barriers/degraded
     ?siteId=
     → DegradedBarrier[]
```

### 4.2 Incident & Risk Endpoints

```
GET  /api/v1/incidents/summary
     ?siteId=&assetId=&from=&to=
     → IncidentSummary

GET  /api/v1/near-misses/summary
     ?siteId=&assetId=&from=&to=
     → NearMissSummary

GET  /api/v1/risk/heatmap
     ?period=30d
     → RiskHeatmapRow[]

GET  /api/v1/risk/score/{siteId}
     → SiteRiskScore
```

### 4.3 AI Copilot Endpoint

```
POST /api/v1/copilot/query
     Body: { "question": "string" }
     → CopilotResponse {
         answer: string,
         citations: SourceRecord[],
         recommendedActions: RecommendedAction[],
         dataScope: "in_scope" | "out_of_scope" | "partial"
       }
```

### 4.4 Executive Endpoints

```
GET  /api/v1/executive/kpis           → ExecutiveKPIs
GET  /api/v1/executive/critical-risks → CriticalRisk[]
GET  /api/v1/executive/barrier-health → BarrierHealthScore
GET  /api/v1/executive/compliance     → ComplianceSummary
```

### 4.5 Data Quality Endpoint

```
GET  /api/v1/data-quality/summary
     ?siteId=&category=
     → DataQualitySummary {
         totalRecords: number,
         validRecords: number,
         flaggedRecords: number,
         lastIngestionAt: string,
         issuesByCategory: CategoryIssue[]
       }
```

### 4.6 Authentication & Authorization

| Role | Access |
|---|---|
| `hse-manager` | Full access to operational cockpits, AI Copilot, and risk dashboard |
| `hse-data-owner` | Access to data quality views, ingestion status, and operational cockpits |
| `executive` | Access to executive cockpit and high-level KPIs only |
| `admin` | Full access including configuration management |

- Cognito User Pool with custom attributes for role assignment
- API Gateway JWT authorizer validates tokens on every request
- Backend enforces role-based filtering on sensitive endpoints

---

## 5. Non-Functional Considerations

| Concern | AWS Approach |
|---|---|
| **Accessibility** | WCAG 2.1 AA minimum; keyboard navigation, sufficient colour contrast, screen-reader labels on all RAG indicators |
| **Security** | Cognito authentication; API Gateway authorization; VPC isolation for ECS and RDS; Secrets Manager for credentials; parameterised queries via EF Core; WAF on CloudFront |
| **Data freshness** | EventBridge scheduled batch import; UI shows last ingestion timestamp; stale data labelled when beyond configured threshold (Parameter Store) |
| **Scalability** | ECS Fargate auto-scaling (1–4 tasks based on CPU/memory); RDS read replica for reporting; OpenSearch Serverless auto-scales; Lambda concurrency for ingestion spikes |
| **High Availability** | Multi-AZ RDS; ECS tasks across multiple AZs; CloudFront global edge caching; S3 99.999999999% durability |
| **Resilience** | Lambda DLQ (SQS) for failed ingestions; ECS health checks with auto-restart; RDS automated failover; partial data surfaces quality labels rather than silent gaps |
| **Observability** | CloudWatch Logs (structured JSON) on all Lambda and ECS; CloudWatch Metrics for API latency, error rates, ingestion counts; CloudWatch Alarms for anomaly detection; X-Ray tracing for request correlation |
| **Cost Optimisation** | Fargate Spot for non-critical workloads; RDS reserved instance for production; S3 Intelligent-Tiering for source files; DynamoDB on-demand capacity |
| **Disaster Recovery** | RDS automated backups (7-day retention) + cross-region snapshot copy; S3 versioning on source bucket; CDK IaC enables full environment rebuild |
| **Compliance** | AWS CloudTrail for API audit logging; encryption at rest (RDS, S3, DynamoDB, OpenSearch) via AWS KMS; encryption in transit (TLS 1.2+) |

---

## 6. Deployment Architecture

### 6.1 Environments

| Environment | Purpose | AWS Account Strategy |
|---|---|---|
| `dev` | Local development + shared dev services | Shared dev account |
| `staging` | Integration testing, UAT | Separate staging account |
| `production` | Live pilot deployment | Isolated production account |

### 6.2 CI/CD Pipeline (AWS CodePipeline)

```
Source (CodeCommit/GitHub)
    │
    ▼
CodeBuild — Build Stage
    ├── Backend: dotnet build + test → Docker image → ECR
    ├── Frontend: npm ci + build → S3 deploy package
    └── Infrastructure: CDK synth → CloudFormation templates
    │
    ▼
CodeBuild — Test Stage
    ├── Unit tests (.NET + React)
    ├── Integration tests (against staging RDS)
    └── WCAG accessibility scan (axe-core)
    │
    ▼
CodeDeploy — Staging
    ├── CDK deploy (staging)
    ├── ECS rolling deployment
    ├── CloudFront invalidation
    └── Smoke tests
    │
    ▼ (Manual approval gate)
CodeDeploy — Production
    ├── CDK deploy (production)
    ├── ECS blue/green deployment
    ├── CloudFront invalidation
    └── Canary health checks
```

### 6.3 Networking

- **VPC**: 2 public subnets (NAT Gateway, ALB) + 2 private subnets (ECS, RDS, Lambda)
- **Security Groups**: ECS → RDS (port 5432 only); Lambda → RDS; ECS → OpenSearch (port 443)
- **VPC Endpoints**: S3 Gateway endpoint, Secrets Manager interface endpoint, Bedrock interface endpoint
- **CloudFront → API Gateway**: Origin request policy with custom headers for backend routing

---

## 7. Constraints & Decisions

| Decision | Rationale |
|---|---|
| ECS Fargate over Lambda for API | API requires persistent connections to RDS and consistent cold-start performance; Fargate provides predictable latency |
| Lambda for ingestion pipeline | Batch jobs are short-lived, event-driven; Lambda provides cost-effective execution without idle compute |
| RDS PostgreSQL over Aurora Serverless | Predictable workload for MVP pilot; PostgreSQL compatibility with EF Core; Aurora Serverless can be adopted post-MVP for auto-scaling |
| OpenSearch Serverless over self-managed | Eliminates cluster management; auto-scales for vector search; pay-per-use for MVP scale |
| Amazon Bedrock over self-hosted LLM | Managed service reduces operational burden; no GPU infrastructure to manage; model selection flexibility (Claude, Titan) |
| DynamoDB for audit log over RDS | High write throughput for quality events; TTL auto-cleanup; avoids bloating the relational store |
| CDK (C#) over Terraform | Same language as backend team; type-safe constructs; first-class AWS support |
| Cognito over custom auth | Managed user directory; built-in JWT flow; integrates natively with API Gateway |
| Batch ingestion only | Streaming is out of scope for MVP; simplifies data layer and reduces delivery risk |
| Configuration-driven RAG thresholds | Thresholds are business decisions; Parameter Store avoids code changes when HSE stakeholders adjust values |
| Conservative status on conflict | Safety-critical context requires the most cautious interpretation when data signals conflict |
| Predefined recommendation rules only | Autonomous rule generation is out of scope; rules are authored by HSE SMEs and stored in S3 configuration |
