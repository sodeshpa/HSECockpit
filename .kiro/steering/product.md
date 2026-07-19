---
inclusion: always
---

# Product: Digital HSE Cockpit (D4HSE)

## Project Overview

The Digital HSE Cockpit is a cloud-native application that provides HSE managers with a unified view of critical barrier health, incident and risk dashboards, AI-assisted natural language queries, and executive KPI summaries. The MVP targets 1–3 pilot sites within an 8–12 week delivery window.

## Architecture & Technology Stack

- **Frontend**: React 18 + Vite, Tailwind CSS + shadcn/ui, Recharts, TanStack Query, React Router v6, Axios
- **Backend**: .NET 8 Web API on AWS ECS Fargate, Entity Framework Core 8 (PostgreSQL)
- **Database**: Amazon RDS PostgreSQL (Multi-AZ), Amazon DynamoDB (audit logs)
- **AI/RAG**: Amazon Bedrock (Claude + Titan Embeddings v2), Amazon OpenSearch Serverless (vector index)
- **Ingestion**: AWS Lambda + Amazon EventBridge Scheduler, S3 landing zone
- **Auth**: Amazon Cognito (JWT, role-based access)
- **Infrastructure**: AWS CDK (C#), CodePipeline + CodeBuild CI/CD
- **Monitoring**: Amazon CloudWatch, X-Ray tracing

## Key Domain Concepts

- **Critical Barrier**: A safety control monitored with Green/Amber/Red (RAG) health status
- **Barrier Degradation**: When a barrier's status worsens compared to prior observation
- **Site Risk Score**: Composite of incident severity, near-miss frequency, open risks, and barrier health (0–100 range)
- **Data Quality Status**: Every record carries VALID, FLAGGED, or CONFLICT status; metrics are labelled when impacted
- **Recommended Action**: Rule-based suggestion (never AI-generated) linked to observed conditions

## Coding Standards

### Backend (.NET 8)

- Follow clean architecture: D4HSE.Api → D4HSE.Services → D4HSE.Core ← D4HSE.Infrastructure
- Use EF Core with parameterised queries only; never use raw SQL string interpolation
- Load configuration (RAG thresholds, risk weights, schedules) from AWS Systems Manager Parameter Store
- Load secrets (connection strings, API keys) from AWS Secrets Manager
- Use FluentValidation for all request validation with business-readable messages
- Return data_quality_status on all entity API responses
- Apply conservative conflict rule: highest severity RAG status wins when observations conflict

### Frontend (React)

- Use TanStack Query for all server-state; no manual fetch/useEffect patterns for API calls
- Use shadcn/ui components as the base; do not introduce additional UI libraries
- All colour-coded indicators (RAG badges, heatmap, compliance) must have text equivalents for accessibility
- Implement distinct empty states: "No data found" vs "Data unavailable" (per spec)
- All views must support keyboard navigation and WCAG 2.1 AA compliance

### AWS Infrastructure

- All infrastructure defined in CDK (C#) — no manual console changes
- Lambda functions for ingestion; ECS Fargate for API (never Lambda for the API layer)
- DynamoDB for audit/quality logs; never store quality events in RDS
- OpenSearch Serverless for vector search; embeddings via Bedrock Titan Embeddings v2
- S3 landing zone for source files; EventBridge Scheduler for ingestion triggers

## Data Ingestion Rules

- Batch/scheduled only — no real-time streaming in MVP
- Three source categories: barrier inspections, incidents/near misses, asset maintenance records
- Invalid records go to DynamoDB quality log with business-readable messages; never persist invalid data to RDS
- SQS dead-letter queues on all Lambda functions for retry handling
- Update last-ingestion timestamp in Parameter Store after each successful run

## Security Rules

- No secrets, API keys, or connection strings in source code or Docker images
- Cognito JWT required on all API endpoints via API Gateway authorizer
- Four roles: hse-manager, hse-data-owner, executive, admin
- WAF enabled on CloudFront distribution
- Encryption at rest (KMS) for RDS, S3, DynamoDB, OpenSearch
- TLS 1.2+ for all data in transit

## AI Copilot Rules

- LLM responses must be grounded in retrieved HSE records only
- Recommended actions come from predefined rules in S3 config — the LLM must never invent actions
- Responses must clearly label "Observed" (factual) vs "Recommended" (rule-based)
- Out-of-scope queries must return a limitation message and suggest supported alternatives
- Prompt injection prevention: sanitise all user input before inclusion in prompts

## Spec References

- Requirements: #[[file:.kiro/specs/001-critical-barriers-cockpit/requirements.md]]
- Design: #[[file:.kiro/specs/001-critical-barriers-cockpit/design.md]]
- Tasks: #[[file:.kiro/specs/001-critical-barriers-cockpit/tasks.md]]
---
inclusion: always
---

# FleetMate — Product & Engineering Guidelines

## Product Overview



## Tech Stack

-
- **Database**: PostgreSQL
- **AI Integration**: LLM provider abstracted behind an interface (provider-agnostic)
- **Testing**: fast-check for property-based tests, example-based unit and integration tests

## Architecture Principles

- Stateless API layer — no server-side session state between requests
- AI provider abstraction — all LLM calls go through a common interface so providers can be swapped without changing business logic
- Non-blocking storage — persistence failures never block the customer response
- Fail-fast validation — reject invalid input immediately with clear error messages
- Structured diagnosis output — AI responses are always parsed into `probableIssue` and `repairRecommendation` fields

## Code Style

- Use strict TypeScript with explicit interface definitions for all data contracts
- Prefer `interface` over `type` for object shapes
- Use descriptive error codes as string literal unions (e.g., `'TOO_SHORT' | 'TOO_LONG'`)
- All timestamps in ISO 8601 UTC format
- UUIDs (v4) for interaction identifiers
- Keep functions small and single-purpose; orchestration logic lives in dedicated service layers
- Use async/await for all asynchronous operations — no raw Promise chains

## API Conventions

- REST endpoints under `/api/` prefix
- Validation errors return 422 with a `code` and `suggestion` field
- AI/service failures return 503 with a user-friendly `suggestion`
- Retrieval errors return 400 (bad request) or 404 (not found) as appropriate
- All responses include a `timestamp` field in ISO 8601 UTC

## Error Handling

- Log all errors with correlation IDs
- Retry storage failures once within 5 seconds; on second failure, log and continue
- AI timeouts (>10 seconds) are treated as failures — never hang indefinitely
- Never expose internal error details to customers

## Testing

- Property-based tests use fast-check with a minimum of 100 iterations per property
- Tag property tests with feature and property name for traceability
- Unit tests cover edge cases and timing behavior
- Integration tests verify end-to-end flows including database interactions
- All validation boundaries must have corresponding property tests

## Database

- PostgreSQL with indexed `created_at` for efficient date-range queries
- Interaction records retained for a minimum of 12 months


