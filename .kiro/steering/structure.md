---
inclusion: always
---

# Project Structure: Digital HSE Cockpit

## Repository Layout

```
HSECockpit/
├── .kiro/
│   ├── specs/                         # Kiro spec files
│   │   └── 001-critical-barriers-cockpit/
│   │       ├── requirements.md
│   │       ├── design.md
│   │       └── tasks.md
│   └── steering/                      # Kiro steering rules
│       ├── product.md
│       └── structure.md
├── infrastructure/                    # AWS CDK (C#)
│   ├── HSECockpit.Infra.sln
│   └── HSECockpit.Infra/
│       ├── Program.cs
│       └── Stacks/
│           ├── NetworkStack.cs        # VPC, subnets, security groups
│           ├── DatabaseStack.cs       # RDS PostgreSQL, DynamoDB
│           ├── ComputeStack.cs        # ECS Fargate, Lambda functions
│           ├── FrontendStack.cs       # S3, CloudFront
│           ├── ApiGatewayStack.cs     # API Gateway, Cognito
│           └── ObservabilityStack.cs  # CloudWatch, alarms
├── backend/
│   ├── D4HSE.sln
│   ├── D4HSE.Api/                     # .NET 8 Web API (ECS Fargate container)
│   │   ├── Controllers/              # API controllers per domain
│   │   ├── Models/                   # DTOs and response models
│   │   ├── Middleware/               # Auth, error handling, correlation ID
│   │   ├── Dockerfile
│   │   └── Program.cs
│   ├── D4HSE.Core/                    # Domain layer (no external dependencies)
│   │   ├── Entities/                 # Domain entities (Site, Asset, CriticalBarrier, etc.)
│   │   └── Interfaces/              # Repository and service interfaces
│   ├── D4HSE.Infrastructure/          # Data access and AWS integrations
│   │   ├── Data/                     # EF Core DbContext, configurations
│   │   ├── Repositories/            # Repository implementations
│   │   ├── Seed/                     # Seed data for pilot site
│   │   └── AwsServices/             # Bedrock, OpenSearch, DynamoDB clients
│   ├── D4HSE.Services/                # Business logic layer
│   │   └── Services/                # BarrierService, IncidentService, RiskScoreService, etc.
│   └── D4HSE.Ingestion/              # Lambda functions for data ingestion
│       ├── Functions/
│       │   ├── BarrierIngestionFunction.cs
│       │   ├── IncidentIngestionFunction.cs
│       │   └── MaintenanceIngestionFunction.cs
│       ├── Validators/               # FluentValidation rules per record type
│       └── aws-lambda-tools-defaults.json
└── frontend/
    ├── package.json
    ├── vite.config.js
    ├── index.html
    └── src/
        ├── main.jsx                   # App entry point
        ├── App.jsx                    # Root component with router
        ├── api/                       # Axios service layer (typed API calls)
        ├── auth/                      # Cognito auth context, guards, login
        ├── components/                # Shared UI components
        │   ├── ui/                   # shadcn/ui primitives
        │   ├── filters/              # Site/asset filter bar
        │   ├── indicators/           # RAG badges, quality banners, trend arrows
        │   └── layout/               # App shell, sidebar, navigation
        ├── pages/                     # Route-level page components
        │   ├── BarrierCockpit/       # Critical barriers view
        │   ├── RiskDashboard/        # Incidents & risk heatmap
        │   ├── ExecutiveCockpit/     # Executive KPI view
        │   └── AICopilot/           # Natural language chat UI
        └── hooks/                     # Custom TanStack Query hooks
```

## Structural Rules

### General

- Keep the three top-level directories (`infrastructure/`, `backend/`, `frontend/`) at the repository root
- Never mix infrastructure code with application code
- Spec and steering files live in `.kiro/` and are not deployed

### Backend (.NET 8)

- Follow clean architecture with dependency flow: Api → Services → Core ← Infrastructure
- `D4HSE.Core` must have zero external package dependencies (domain models and interfaces only)
- `D4HSE.Api` contains only controllers, DTOs, middleware, and startup — no business logic
- `D4HSE.Services` contains all business logic — risk scoring, barrier health derivation, compliance checks
- `D4HSE.Infrastructure` contains all data access (EF Core), AWS SDK clients, and repository implementations
- `D4HSE.Ingestion` is a separate project for Lambda functions — it references Core and Infrastructure but not Api
- One controller per domain area: `BarriersController`, `IncidentsController`, `RiskController`, `ExecutiveController`, `CopilotController`, `DataQualityController`
- Entity classes go in `D4HSE.Core/Entities/`; never create entity classes outside Core

### Frontend (React)

- Pages are route-level components in `src/pages/` — one folder per view
- Shared components go in `src/components/` with subfolders by concern (ui, filters, indicators, layout)
- API service functions go in `src/api/` — one file per backend domain (barriers.js, incidents.js, executive.js, copilot.js)
- TanStack Query hooks go in `src/hooks/` — one file per domain (useBarriers.js, useIncidents.js, etc.)
- Auth-related code (Cognito context, route guards) lives in `src/auth/`
- Do not create utility files at the root of `src/` — group by concern in appropriate subfolder

### Infrastructure (CDK)

- One stack per concern: Network, Database, Compute, Frontend, ApiGateway, Observability
- All stacks defined in `infrastructure/HSECockpit.Infra/Stacks/`
- Stack composition and deployment order managed in `Program.cs`
- Environment-specific configuration (dev/staging/prod) passed via CDK context, not hardcoded

### Naming Conventions

- Backend: PascalCase for classes, methods, properties; camelCase for local variables and parameters
- Frontend: PascalCase for React components; camelCase for functions, hooks, and variables
- Files: PascalCase for C# files matching class names; camelCase for JS/JSX files; kebab-case for CSS modules
- API routes: lowercase kebab-case (`/api/v1/near-misses/summary`)
- Database: snake_case for table and column names
- CDK stacks: PascalCase class names (e.g., `NetworkStack`, `DatabaseStack`)
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