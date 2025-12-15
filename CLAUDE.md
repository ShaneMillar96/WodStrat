# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

WodStrat is a functional fitness workout strategy platform with a .NET 8 backend, React 18 frontend, and PostgreSQL database. The project uses an AI-driven JIRA workflow with specialized planning and execution agents.

## Development Commands

### Infrastructure
```bash
# Start PostgreSQL database
cd infra && docker compose up -d postgres

# Run Flyway migrations
docker compose --profile migrate run --rm flyway migrate

# View migration status
docker compose --profile migrate run --rm flyway info
```

### Backend (.NET 8)
```bash
# Build solution
cd backend && dotnet build

# Run API (requires DB_CONNECTION_STRING env var)
DB_CONNECTION_STRING="Host=localhost;Port=5432;Database=wodstrat;Username=wodstrat;Password=wodstrat_dev" dotnet run --project src/WodStrat.Api

# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "FullyQualifiedName~AthleteServiceTests"

# Run single test
dotnet test --filter "FullyQualifiedName~AthleteServiceTests.CreateAsync_ValidDto_ReturnsCreatedAthlete"
```

### Frontend (React + Vite)
```bash
cd frontend
npm install
npm run dev      # Dev server at localhost:5173
npm run build    # Production build
```

## Architecture

### Backend Structure (Clean Architecture)
```
backend/src/
├── WodStrat.Api/           # Controllers, ViewModels, Validators
├── WodStrat.Services/      # Business logic, DTOs, Interfaces
└── WodStrat.Dal/           # EF Core DbContext, Models, Enums
```

### Frontend Structure
```
frontend/src/
├── pages/          # Route components
├── components/     # UI components (ui/, forms/)
├── hooks/          # Custom React hooks
├── services/       # API client layer
├── types/          # TypeScript definitions
├── schemas/        # Zod validation schemas
└── lib/            # TanStack Query config
```

### Database
- PostgreSQL 16 with custom ENUM types (`experience_level`, `athlete_goal`, `benchmark_category`, `benchmark_metric_type`)
- Flyway migrations in `db/migrations/V{n}__description.sql`
- EF Core enums require `[PgName("...")]` attributes and `NpgsqlDataSourceBuilder.MapEnum<T>()` registration

## Key Patterns

### Backend
- FluentValidation for request validation
- Extension methods for DTO/ViewModel mapping (no AutoMapper)
- Soft delete via `IsDeleted` flag on entities
- xUnit + NSubstitute + AutoFixture for testing

### Frontend
- TanStack Query for server state
- React Hook Form + Zod for forms
- Tailwind CSS for styling
- API proxy in Vite config routes `/api/*` to backend

## AI Workflow Commands

The repository includes an AI-driven development workflow via slash commands:

| Command | Purpose |
|---------|---------|
| `/jira-plan WOD-XX` | Create implementation plans (spawns 4 parallel planner agents) |
| `/jira-execute WOD-XX` | Execute implementation (sequential executor agents) |
| `/implementation-reviewer WOD-XX` | Review implementation quality |
| `/test-implementer WOD-XX` | Generate test coverage |
| `/finalize WOD-XX` | Create PR and update JIRA |

Plans and reports are stored in `.work/WOD-XX/` directory.

## API Endpoints

| Endpoint | Description |
|----------|-------------|
| `GET /health` | Health check |
| `GET/POST /api/athletes` | Athlete CRUD |
| `GET/PUT /api/athletes/{id}` | Single athlete operations |
| `GET /api/benchmark-definitions` | List benchmark types |
| `GET/POST /api/athletes/{id}/benchmarks` | Athlete benchmark results |
