---
inclusion: always
---

# Technology Guidelines: Digital HSE Cockpit

## Backend — .NET 8 Web API

### Framework & Patterns

- Target: .NET 8 (LTS), C# 12
- API style: Controller-based (not minimal API) for this project
- Dependency injection: use built-in Microsoft DI container
- Configuration: Options pattern (`IOptions<T>`) for strongly-typed settings
- Async/await: all I/O-bound operations must be async; never use `.Result` or `.Wait()`
- Cancellation tokens: propagate `CancellationToken` through all async controller actions and service methods

### Entity Framework Core 8

- Provider: Npgsql (PostgreSQL)
- Approach: Code-first with migrations checked into source control
- DbContext lifetime: scoped (one per request)
- Queries: always use LINQ or parameterised raw SQL via `FromSqlInterpolated`; never concatenate user input into SQL
- No tracking: use `.AsNoTracking()` for read-only queries
- Migrations: generate with `dotnet ef migrations add <Name>` — never edit migration files manually after generation

### FluentValidation

- One validator class per request DTO (e.g., `GetBarriersRequestValidator`)
- Register validators via `AddValidatorsFromAssemblyContaining<T>()`
- Validation messages must be business-readable (displayed to HSE users in data quality views)
- Severity levels: Error (blocks persistence), Warning (flags record but allows persistence)

### API Conventions

- Route prefix: `/api/v1/`
- HTTP verbs: GET for reads, POST for AI Copilot queries, PUT for updates
- Response format: always return typed response objects; never return anonymous types
- Error responses: use `ProblemDetails` (RFC 7807) for all error responses
- Pagination: use `?page=1&pageSize=20` with response wrapper `{ data: [], totalCount, page, pageSize }`
- Date format: ISO 8601 (`yyyy-MM-ddTHH:mm:ssZ`) in all API responses
- Null handling: omit null fields from JSON responses (configure in `Program.cs`)

### Logging

- Use Serilog with structured JSON output
- Log to CloudWatch via Serilog.Sinks.AwsCloudWatch
- Include correlation ID in all log entries (set via middleware)
- Log levels: Information for requests, Warning for data quality issues, Error for failures
- Never log sensitive data (secrets, tokens, PII)

### Testing

- Unit tests: xUnit + Moq + FluentAssertions
- Test project naming: `D4HSE.*.Tests` (e.g., `D4HSE.Services.Tests`)
- Test naming: `MethodName_Scenario_ExpectedResult` pattern
- Integration tests: use WebApplicationFactory with in-memory PostgreSQL (Testcontainers)

---

## Frontend — React 18

### Framework & Tooling

- Build tool: Vite (latest)
- Language: JavaScript (JSX) — no TypeScript in MVP (can adopt post-MVP)
- Package manager: npm
- Node version: 20 LTS

### State Management

- Server state: TanStack Query (React Query v5) for all API data
- Client state: React Context only for auth and global UI state (theme, sidebar)
- No Redux, Zustand, or other state libraries in MVP
- Query keys: structured as arrays (`['barriers', siteId, assetId]`)
- Stale time: 30 seconds for operational data, 5 minutes for reference data

### UI Components

- Component library: shadcn/ui (copy-paste primitives, not a dependency)
- Styling: Tailwind CSS — no inline styles, no CSS modules, no styled-components
- Icons: Lucide React (comes with shadcn/ui)
- Charts: Recharts — use composable chart components (LineChart, BarChart, RadialBarChart)
- Do not add additional UI libraries without explicit approval

### Routing

- React Router v6 with data router (`createBrowserRouter`)
- Route structure mirrors `src/pages/` folder layout
- Protected routes wrapped with auth guard component
- Lazy load page components with `React.lazy()` + Suspense

### HTTP & API Integration

- HTTP client: Axios with a configured instance (`src/api/client.js`)
- Base URL: loaded from environment variable (`VITE_API_BASE_URL`)
- Auth: Axios interceptor attaches Cognito JWT to `Authorization: Bearer` header
- Error handling: global Axios interceptor for 401 (redirect to login) and 5xx (toast notification)

### Accessibility (WCAG 2.1 AA)

- All interactive elements must be keyboard focusable and operable
- Colour-coded indicators (RAG badges, heatmap cells) must include text labels or aria-label
- Minimum contrast ratio: 4.5:1 for normal text, 3:1 for large text
- Focus indicators must be visible on all interactive elements
- Use semantic HTML elements (`nav`, `main`, `section`, `article`, `button`)
- Form inputs must have associated `<label>` elements
- Run axe-core in CI/CD pipeline; zero violations required for merge

### Testing

- Unit tests: Vitest + React Testing Library
- Test files co-located with components (`Component.test.jsx`)
- Test user interactions, not implementation details
- Accessibility: include axe-core assertions in component tests

---

## AWS Services & Configuration

### Compute

| Service | Use | Config |
|---|---|---|
| ECS Fargate | .NET 8 API hosting | Auto-scaling 1–4 tasks; 0.5 vCPU / 1 GB RAM per task |
| AWS Lambda | Data ingestion functions | .NET 8 runtime; 512 MB memory; 5-min timeout; SQS DLQ |

### Data

| Service | Use | Config |
|---|---|---|
| Amazon RDS | PostgreSQL 16 relational store | Multi-AZ; db.t3.medium; 7-day backup retention |
| Amazon DynamoDB | Audit/quality log | On-demand capacity; TTL 90 days |
| Amazon OpenSearch Serverless | Vector index for RAG | k-NN collection; 1024-dim vectors; cosine similarity |
| Amazon S3 | Source file landing zone + frontend hosting | Versioning enabled; Intelligent-Tiering for source files |

### AI & ML

| Service | Use | Config |
|---|---|---|
| Amazon Bedrock (Claude) | LLM for natural language queries | Latest Claude model; max tokens 4096 |
| Amazon Bedrock (Titan Embeddings v2) | Text embedding generation | 1024-dimension output |

### Networking & Security

| Service | Use | Config |
|---|---|---|
| Amazon CloudFront | CDN for React SPA + API routing | WAF enabled; custom error pages |
| Amazon API Gateway | HTTP API with JWT auth | Cognito authorizer; rate limiting; CORS |
| Amazon Cognito | User authentication | User Pool with custom role attribute; PKCE flow for SPA |
| AWS Secrets Manager | Credentials storage | RDS connection string, Bedrock config |
| AWS Systems Manager | Application configuration | Parameter Store for RAG thresholds, risk weights |
| AWS KMS | Encryption keys | Customer-managed keys for RDS, S3, DynamoDB, OpenSearch |

### CI/CD & Operations

| Service | Use | Config |
|---|---|---|
| AWS CodePipeline | Deployment orchestration | Source → Build → Test → Staging → Approval → Production |
| AWS CodeBuild | Build & test | .NET build + Docker → ECR; npm build → S3 |
| Amazon ECR | Container registry | Image scanning enabled; lifecycle policy (keep last 10) |
| Amazon CloudWatch | Logging & monitoring | Structured JSON logs; custom metrics; alarms on error rates |
| AWS X-Ray | Distributed tracing | Enabled on API Gateway + ECS |
| AWS CloudTrail | Audit trail | All management + data events logged |

### Infrastructure as Code

- Tool: AWS CDK v2 (C#)
- One stack per concern (Network, Database, Compute, Frontend, ApiGateway, Observability)
- Environment config via CDK context (`cdk.json`) — never hardcode account IDs or regions
- Tag all resources: `Project=HSECockpit`, `Environment=dev|staging|prod`, `ManagedBy=CDK`
- Use `RemovalPolicy.RETAIN` for RDS and S3 in production; `DESTROY` in dev/staging

---

## Dependency Management

### Backend (NuGet)

- Pin exact versions in `.csproj` files (no floating versions)
- Key packages: `Microsoft.EntityFrameworkCore` 8.x, `Npgsql.EntityFrameworkCore.PostgreSQL` 8.x, `FluentValidation.AspNetCore` 11.x, `AWSSDK.*` latest stable, `Serilog.*` latest stable, `Swashbuckle.AspNetCore` 6.x

### Frontend (npm)

- Use exact versions in `package.json` (no `^` or `~` prefixes)
- Key packages: `react` 18.x, `react-dom` 18.x, `react-router-dom` 6.x, `@tanstack/react-query` 5.x, `axios` 1.x, `recharts` 2.x, `tailwindcss` 3.x, `lucide-react` latest

### Infrastructure (NuGet)

- CDK packages: `Amazon.CDK.Lib` latest v2, `Constructs` latest

---

## Environment Variables

### Backend (ECS Task Definition)

```
ASPNETCORE_ENVIRONMENT=Development|Staging|Production
AWS_REGION=<region>
DATABASE__SECRETARN=<secrets-manager-arn>
BEDROCK__MODELID=<model-id>
OPENSEARCH__ENDPOINT=<opensearch-url>
OPENSEARCH__COLLECTIONNAME=hse-embeddings
```

### Frontend (Vite — .env files)

```
VITE_API_BASE_URL=https://<api-gateway-url>/api/v1
VITE_COGNITO_USER_POOL_ID=<pool-id>
VITE_COGNITO_CLIENT_ID=<client-id>
VITE_COGNITO_DOMAIN=<cognito-domain>
VITE_AWS_REGION=<region>
```

### Lambda (Environment)

```
DATABASE__CONNECTIONSTRING=<from-secrets-manager-at-runtime>
S3__LANDINGZONE_BUCKET=hse-landing-zone
DYNAMODB__QUALITYLOG_TABLE=DataQualityLog
PARAMETERSTORE__INGESTION_TIMESTAMP_KEY=/hse/ingestion/last-run
```

---

## Build & Run Commands

### Backend

```bash
# Restore & build
dotnet restore backend/D4HSE.sln
dotnet build backend/D4HSE.sln

# Run API locally
dotnet run --project backend/D4HSE.Api

# Run tests
dotnet test backend/D4HSE.sln

# EF Core migrations
dotnet ef migrations add <Name> --project backend/D4HSE.Infrastructure --startup-project backend/D4HSE.Api
dotnet ef database update --project backend/D4HSE.Infrastructure --startup-project backend/D4HSE.Api

# Docker build
docker build -t d4hse-api -f backend/D4HSE.Api/Dockerfile backend/
```

### Frontend

```bash
# Install dependencies
npm ci --prefix frontend

# Dev server (localhost:5173)
npm run dev --prefix frontend

# Production build
npm run build --prefix frontend

# Run tests
npm run test --prefix frontend

# Lint
npm run lint --prefix frontend
```

### Infrastructure

```bash
# CDK synth (validate templates)
dotnet build infrastructure/HSECockpit.Infra.sln
cd infrastructure/HSECockpit.Infra && cdk synth

# Deploy all stacks
cdk deploy --all

# Deploy specific stack
cdk deploy NetworkStack
```
---
inclusion: always
---
# Project Structure

```text
vehicle-diagnostic-assistant/
│
├── .kiro/
│   ├── specs/
│   │   └── vehicle-diagnostic-assistant/
│   │       ├── requirements.md
│   │       ├── design.md
│   │       └── tasks.md
│   │
│   └── steering/
│       ├── product.md
│       ├── tech.md
│       └── structure.md
│
├── frontend/
│   └── src/
│       ├── components/
│       ├── pages/
│       ├── services/
│       └── utils/
│
├── backend/
│   ├── src/
│   │   ├── handlers/              # Lambda entry points
│   │   ├── diagnostics/           # AI diagnosis domain
│   │   │   ├── services/
│   │   │   ├── repositories/
│   │   │   ├── models/
│   │   │   ├── prompts/
│   │   │   └── events/
│   │   │
│   │   ├── shared/
│   │   │   ├── auth/
│   │   │   ├── logging/
│   │   │   ├── validation/
│   │   │   └── utils/
│   │   │
│   │   └── tests/
│   │
│   ├── requirements.txt
│   └── template.yaml
│
├── infrastructure/
│   ├── cloudformation/
│   └── environments/
│       ├── dev/
│       ├── qa/
│       └── prod/
│
├── tests/
│   ├── integration/
│   └── e2e/
│
├── docs/
├── README.md
└── .gitignore
```

---

# Conventions

- Group code by domain (`diagnostics`, `audit`, `escalation`)
- Keep prompts separate from business logic
- Keep Lambda handlers thin
- Store shared utilities in `shared/`
- Use repositories for DynamoDB access
- Use CloudFormation for infrastructure
- Keep environments isolated (`dev`, `qa`, `prod`)
- Never hardcode secrets or credentials

---