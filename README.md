# WodStrat

An intelligent workout strategy and analytics platform built with .NET and React, designed to help functional fitness athletes analyse workouts and generate personalized strategy breakdowns using structured performance data.

## Project Overview

WodStrat aims to transform how CrossFit and functional fitness athletes approach daily workouts by providing a modern web-based architecture that unifies athlete data, workout descriptions, and intelligent strategy generation. While traditional WOD logging tools focus on recording results, WodStrat provides AI-powered workout strategy insights, pacing recommendations, and stimulus interpretation tailored to the individual.

The platform is built as a mobile-responsive web application, designed to work seamlessly on desktop and mobile browsers, with architecture prepared for future native mobile app development.

Athletes can:

- Connect existing fitness accounts (Strava, Garmin, SugarWOD, CrossFit.com, BTWB) to centralize their data
- Paste in any workout (e.g., daily class WOD, Open workout, competition event)
- Receive AI-powered personalized strategies tailored to their capabilities
- Get rep scheme breakdowns and pacing optimization recommendations
- Visualize training trends and progress
- Compare performance across benchmarks
- Prepare more effectively for Open, HYROX, or competition season

## Problem Definition

### The Challenge

Functional fitness athletes face several pain points when planning or executing workouts:

- **Lack of Personal Strategy:** Workout descriptions don't provide pacing guidance tailored to individual strengths/weaknesses.
- **Fragmented Data:** Personal metrics are scattered across SugarWOD, BTWB, Strava, Garmin, spreadsheets, and memory.
- **No Intelligent Insights:** Most existing tools track results but don't interpret them or turn them into actionable strategy.
- **Inconsistent Pacing:** Athletes often attack workouts inefficiently, leading to blowups, poor performance, or inconsistent training stimuli.
- **Limited Historical Awareness:** Athletes struggle to view long-term improvements or understand where they lose time in workouts.

### The Solution

WodStrat addresses these gaps by providing:

- **Unified Data Integration:** Connect Strava, Garmin, SugarWOD, CrossFit.com, and BTWB to centralize all athlete data
- **AI-Powered Strategy:** Intelligent workout analysis and personalized strategy generation
- **Pacing Optimization:** Rep scheme breakdowns and pacing recommendations based on individual capabilities
- **Mobile-First Design:** Responsive web app accessible on any device, PWA-ready for app-like experience
- **Centralized Data Layer:** PostgreSQL database for athlete profiles, training history, and benchmarks
- **Flexible Workout Input:** Ability to paste any WOD text for analysis
- **Version-Controlled Database:** Flyway migrations ensure predictable, reliable schema evolution
- **Scalable Foundation:** Containerized infrastructure for local and cloud deployment

## MoSCoW Priorities

### Must Have (V1)

- Mobile-responsive web application (PWA-ready)
- User accounts with OAuth integration
- Third-party integrations:
  - Strava (activity/workout sync)
  - Garmin (fitness data sync)
  - SugarWOD (WOD history, benchmarks)
  - CrossFit.com (Open scores, benchmark standards)
  - BTWB (workout logging, benchmark data)
- AI-powered strategy generation
- Rep scheme and pacing optimization
- Workout ingestion (raw text parsing)
- Athlete profile and benchmark recording
- Monorepo structure (backend, frontend, database, infra)
- PostgreSQL database with Flyway migrations
- .NET 8 Web API with environment-variable-based configuration
- React + Vite + TypeScript frontend
- Docker Compose for local development

### Should Have

- Data visualizations and charts
- Historical performance insights
- Training load tracking
- Competition prep modules (Open/HYROX/Throwdowns)
- Advanced workout stimulus classification

### Could Have

- Native mobile apps (iOS/Android)
- Social/team features and shared leaderboards
- Live workout tracking
- Multi-language support

### Won't Have (V1)

- Full coaching logic
- Apple Health integration (requires native app)

## Technical Architecture

### Architecture Overview

WodStrat follows a clean, scalable structure enabling rapid expansion into analytics, AI, and athlete performance modeling.

```
React Frontend (PWA) → .NET Backend → PostgreSQL → External Integrations
                              ↓
                     AI/LLM Strategy Engine
```

### Core Components

#### Backend Layer

- ASP.NET Core Web API
- OAuth 2.0 authentication (supporting Strava, Garmin, etc.)
- Integration service layer for third-party APIs
- AI/LLM integration for strategy generation
- Environment-variable-driven configuration
- Health-check endpoint for infrastructure readiness
- Dependency injection and layered architecture

#### Frontend Layer

- React 18 + Vite
- TypeScript for type-safe component development
- Mobile-responsive design (Tailwind CSS)
- PWA capabilities for app-like mobile experience
- Routing and state management

#### Database Layer

- PostgreSQL for relational and analytical needs
- Flyway for repeatable migrations
- Schema for athletes, workouts, benchmarks, and integrations

#### Infrastructure Layer

- Docker Compose orchestrating:
  - PostgreSQL
  - Flyway
  - Backend API
  - Frontend dev server
- Local-first workflow optimized for development

#### Integration Layer

- Strava API integration
- Garmin Connect API integration
- SugarWOD API integration
- CrossFit.com data integration
- BTWB API integration

## Tech Stack

### Backend

- .NET 8 Web API
- C# 12
- OAuth 2.0 / OpenID Connect
- AI/LLM integration (strategy generation)
- Environment variable configuration
- Dependency injection architecture

### Frontend

- React 18
- TypeScript
- Vite
- Tailwind CSS (responsive design)
- PWA support

### Data & Infrastructure

- PostgreSQL
- Flyway
- Docker & Docker Compose
- Containerized local environment

## Prerequisites

- .NET 8 SDK
- Node.js 18+
- Docker
- Docker Compose

## Quick Start

### 1. Clone the Repository

```bash
git clone <repository-url>
cd WodStrat
```

### 2. Start Database & Infrastructure

```bash
cd infra
docker compose up -d
```

This launches:
- PostgreSQL
- Flyway container (ready to run migrations)

### 3. Start the Backend

```bash
cd backend
dotnet run
```

Backend API will be available at: http://localhost:5000

### 4. Start the Frontend

```bash
cd frontend
npm install
npm run dev
```

Frontend will be available at: http://localhost:5173

## Docker Commands

```bash
# Start all containers
docker compose up -d

# Stop all containers
docker compose down

# View logs
docker compose logs -f

# Run migrations manually
docker compose run flyway migrate
```

## Testing

Testing will include:

- Unit tests for domain services
- Integration tests for API endpoints
- Integration tests for third-party API connections
- Load testing for workout parsing & AI strategy generation
- E2E tests for critical user flows

## API Endpoints

| Endpoint | Description |
|----------|-------------|
| `GET /health` | Confirms API is running |
| `POST /auth/*` | Authentication endpoints |
| `GET /integrations/*` | Third-party integration endpoints |
| `POST /workouts/analyze` | AI-powered workout analysis |
| `GET /athletes/profile` | Athlete profile and benchmarks |

More endpoints will be documented as features are built.

## Deployment

Production deployment will include:

- Container-based hosting
- Environment variable configuration
- Reverse proxy (NGINX/Traefik)
- CI/CD pipeline
- SSL/TLS certificates

## Project Structure

```
WodStrat/
├── backend/
│   ├── WodStrat.sln
│   └── WodStrat.Api/
│       ├── Program.cs
│       ├── EnvironmentVariables.cs
│       └── WodStrat.Api.csproj
├── frontend/
│   ├── src/
│   │   ├── main.tsx
│   │   └── App.tsx
│   ├── package.json
│   └── vite.config.ts
├── db/
│   └── migrations/
│       └── V1__initial_empty.sql
├── infra/
│   └── docker-compose.yml
└── README.md
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit changes with clear messages
4. Push and open a pull request

Contributions to architecture refinement, DX improvements, and documentation are welcome.

## License

This project will be licensed under MIT unless specified otherwise.

## Support

For support or feature discussions, open a GitHub issue or contact the development team.

---

Built with love for the functional fitness community — empowering athletes to train smarter, strategize better, and unlock their performance potential.
