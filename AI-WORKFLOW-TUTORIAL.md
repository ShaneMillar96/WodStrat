# AI-Driven Development Workflow Tutorial

This document provides a comprehensive guide to the AI-powered development workflow configured in this repository. The workflow automates the entire software development lifecycle from JIRA ticket to production-ready pull request.

## Table of Contents

1. [Overview](#overview)
2. [Workflow Architecture](#workflow-architecture)
3. [Commands Reference](#commands-reference)
4. [Agents Reference](#agents-reference)
5. [Step-by-Step Workflow Guide](#step-by-step-workflow-guide)
6. [Directory Structure](#directory-structure)
7. [Confidence Assessment System](#confidence-assessment-system)
8. [Best Practices](#best-practices)

---

## Overview

### What is this Workflow?

This is a JIRA-driven AI development workflow that uses specialized agents to plan, implement, review, test, and finalize features. The workflow is designed to:

- **Reduce context switching** - AI maintains context across the entire feature lifecycle
- **Ensure consistency** - Enforces architectural patterns and coding standards
- **Provide traceability** - Creates comprehensive documentation from requirements to code
- **Accelerate development** - Automates repetitive planning and implementation tasks

### When to Use This Workflow

Use this workflow when:
- Implementing new features that span multiple layers (database, backend, frontend)
- The feature has a JIRA ticket with clear requirements
- You want comprehensive planning and documentation
- The feature follows Clean Architecture patterns

### Key Components

| Component | Count | Purpose |
|-----------|-------|---------|
| **Commands** | 7 | User-invoked slash commands for workflow stages |
| **Planner Agents** | 4 | Create implementation plans for each layer |
| **Executor Agents** | 3 | Implement changes based on plans |

---

## Workflow Architecture

### Complete Workflow Diagram

```
                        ┌─────────────────────────────────────────────────────────────┐
                        │                    AI DEVELOPMENT WORKFLOW                   │
                        └─────────────────────────────────────────────────────────────┘

    ┌─────────────────┐
    │ JIRA Ticket     │
    │ (Requirements)  │
    └────────┬────────┘
             │
             ▼
    ┌─────────────────┐     Creates technical description
    │ /jira-description│────────────────────────────────────► JIRA Updated
    └────────┬────────┘
             │
             ▼
    ┌─────────────────┐     ┌─────────────────────────────────────────────────────┐
    │   /jira-plan    │────►│              PLANNING PHASE (Parallel)              │
    └────────┬────────┘     │  ┌──────────────┐  ┌──────────────┐                 │
             │              │  │   Database   │  │   Service    │                 │
             │              │  │   Schema     │  │   Layer      │                 │
             │              │  │   Planner    │  │   Planner    │                 │
             │              │  └──────┬───────┘  └──────┬───────┘                 │
             │              │         │                 │                         │
             │              │         ▼                 ▼                         │
             │              │  schema_changes.md  services_changes.md             │
             │              │                                                     │
             │              │  ┌──────────────┐  ┌──────────────┐                 │
             │              │  │     API      │  │      UI      │                 │
             │              │  │   Planner    │  │   Planner    │                 │
             │              │  └──────┬───────┘  └──────┬───────┘                 │
             │              │         │                 │                         │
             │              │         ▼                 ▼                         │
             │              │  api_changes.md     ui_changes.md                   │
             │              └─────────────────────────────────────────────────────┘
             │                                    │
             │                                    ▼
             │                          ┌─────────────────┐
             │                          │ implementation.md│ (Master Plan)
             │                          └────────┬────────┘
             │                                   │
             ▼                                   ▼
    ┌─────────────────┐     ┌─────────────────────────────────────────────────────┐
    │  /jira-execute  │────►│            EXECUTION PHASE (Sequential)             │
    └────────┬────────┘     │                                                     │
             │              │  Phase 1: Database Executor                         │
             │              │     └──► Migrations, Models, Context                │
             │              │                    │                                │
             │              │  Phase 2: Server Executor (Services)                │
             │              │     └──► Service Classes, DTOs, Specs               │
             │              │                    │                                │
             │              │  Phase 3: Server Executor (API)                     │
             │              │     └──► Controllers, ViewModels, Mappings          │
             │              │                    │                                │
             │              │  Phase 4: UI Executor                               │
             │              │     └──► Components, Hooks, Types, Styling          │
             │              └─────────────────────────────────────────────────────┘
             │                                    │
             │                                    ▼
             │                          ┌─────────────────┐
             │                          │execution_report.md│
             │                          └────────┬────────┘
             ▼                                   │
    ┌─────────────────────┐                      │
    │/implementation-reviewer│◄─────────────────┘
    └────────┬────────────┘
             │              Creates: implementation_review.md
             ▼
    ┌─────────────────┐
    │ /test-implementer│     Creates: test_implementation.md
    └────────┬────────┘      + Test Files
             │
             ▼
    ┌─────────────────┐     Creates: FINAL_REPORT.md
    │   /finalize     │────────────────────────────────────► Pull Request
    └────────┬────────┘                                      JIRA Updated
             │
             ▼
    ┌─────────────────┐     Handles reviewer feedback
    │  /pr-comments   │────────────────────────────────────► Code Updates
    └─────────────────┘
```

### Agent Types

#### Planner Agents (Green - Planning Phase)
Create detailed implementation plans. Run in parallel during `/jira-plan`.

| Agent | Output File | Purpose |
|-------|-------------|---------|
| `database-schema-planner` | `schema_changes.md` | Database migrations, entity models |
| `service-layer-planner` | `services_changes.md` | Business logic, DTOs, specifications |
| `api-planner` | `api_changes.md` | Controllers, endpoints, ViewModels |
| `ui-planner` | `ui_changes.md` | React components, hooks, types |

#### Executor Agents (Blue/Red - Execution Phase)
Implement changes based on plans. Run sequentially during `/jira-execute`.

| Agent | Input Files | Purpose |
|-------|-------------|---------|
| `database-executor` | `schema_changes.md` | Execute migrations, create models |
| `server-executor` | `services_changes.md`, `api_changes.md` | Implement backend layers |
| `ui-executor` | `ui_changes.md` | Implement frontend components |

---

## Commands Reference

### 1. `/jira-description`

**Purpose**: Create or update JIRA tickets with comprehensive technical descriptions.

**Usage**:
```
/jira-description              # Create new ticket
/jira-description WOD-123      # Update existing ticket
```

**What it does**:
1. Asks for feature description
2. Assesses understanding with confidence level (target: 80%+)
3. Asks clarifying questions if confidence is low
4. Generates technical description with:
   - Implementation steps
   - Files to modify
   - Dependencies
   - Testing strategy
   - Acceptance criteria
5. Creates/updates JIRA ticket via MCP

---

### 2. `/jira-plan`

**Purpose**: Orchestrate comprehensive planning for a feature.

**Usage**:
```
/jira-plan WOD-123
```

**What it does**:
1. Validates JIRA ticket exists
2. Creates work directory: `.work/WOD-123/`
3. Creates feature branch: `feature/WOD-123`
4. Launches **4 planner agents in parallel**:
   - Database Schema Planner → `schema_changes.md`
   - Service Layer Planner → `services_changes.md`
   - API Planner → `api_changes.md`
   - UI Planner → `ui_changes.md`
5. Assesses overall confidence
6. Generates master `implementation.md`

**Prerequisites**: Valid JIRA ticket with technical description

---

### 3. `/jira-execute`

**Purpose**: Execute the implementation plan created by `/jira-plan`.

**Usage**:
```
/jira-execute WOD-123
```

**What it does**:
1. Validates planning artifacts exist
2. Executes **4 phases sequentially**:
   - **Phase 1**: Database Executor (migrations, models)
   - **Phase 2**: Server Executor (services)
   - **Phase 3**: Server Executor (API)
   - **Phase 4**: UI Executor (frontend)
3. Validates each phase before proceeding
4. Creates `execution_report.md`
5. Updates JIRA ticket status

**Prerequisites**: Completed `/jira-plan` with all planning documents

---

### 4. `/implementation-reviewer`

**Purpose**: Review implementation quality against planning documents.

**Usage**:
```
/implementation-reviewer WOD-123
```

**What it does**:
1. Reads all planning documents
2. Analyzes git changes
3. Validates plan-to-code compliance for each layer
4. Tests API endpoints manually
5. Calculates implementation score (target: 85%+)
6. Creates `review/implementation_review.md`

**Output includes**:
- Per-layer compliance scores
- Issues found with recommendations
- Code quality assessment
- Next steps based on score

---

### 5. `/test-implementer`

**Purpose**: Generate comprehensive test coverage.

**Usage**:
```
/test-implementer WOD-123
```

**What it does**:
1. Analyzes implemented code
2. Generates backend tests:
   - Service layer unit tests (xUnit, NSubstitute, AutoFixture)
   - Controller tests
3. Generates frontend tests:
   - Component tests (React Testing Library)
   - Hook tests
4. Creates integration test scenarios
5. Runs all tests
6. Creates `review/test_implementation.md`

---

### 6. `/finalize`

**Purpose**: Prepare for production and create pull request.

**Usage**:
```
/finalize WOD-123
```

**What it does**:
1. Validates all prerequisites:
   - Implementation review score ≥ 85%
   - All tests passing
2. Runs code quality checks:
   - `dotnet format` for backend
   - `npm run build` for frontend
3. Creates professional commit
4. Pushes to feature branch
5. Creates comprehensive pull request via GitHub MCP
6. Updates JIRA ticket to "Ready for Review"
7. Creates `FINAL_REPORT.md`

**Quality Gates**:
- Implementation review score ≥ 85%
- 100% test success rate
- Code quality standards enforced
- No critical issues remaining

---

### 7. `/pr-comments`

**Purpose**: Analyze and implement PR feedback automatically.

**Usage**:
```
/pr-comments WOD-123
```

**What it does**:
1. Reads work directory context
2. Analyzes git branch changes
3. Fetches unresolved PR comments via GitHub API
4. Categorizes comments:
   - Actionable code changes
   - Questions/clarifications
   - Suggestions
5. Implements actionable changes
6. Creates implementation report
7. **Does NOT auto-resolve comments** (requires human verification)

---

## Agents Reference

### Planner Agents

#### database-schema-planner
**Color**: Green | **Model**: Opus

Creates `schema_changes.md` containing:
- PostgreSQL CREATE/ALTER TABLE statements
- Flyway migration file specifications (V{n}__description.sql)
- Entity Framework Core model definitions
- DbContext updates
- Index definitions
- Foreign key relationships

#### service-layer-planner
**Color**: Green | **Model**: Opus

Creates `services_changes.md` containing:
- Service class specifications
- Interface definitions
- DTO class designs
- Specification pattern implementations
- Mapping profile configurations
- Dependency injection registrations

#### api-planner
**Color**: Green | **Model**: Opus

Creates `api_changes.md` containing:
- Controller specifications
- HTTP endpoint definitions (verbs, routes)
- Request/response model designs
- Authorization requirements
- OpenAPI documentation attributes
- JSON response structures

#### ui-planner
**Color**: Green | **Model**: Opus

Creates `ui_changes.md` containing:
- React component hierarchy
- Custom hook specifications
- TypeScript type definitions
- API service layer design
- CSS/styling approach
- Implementation order

---

### Executor Agents

#### database-executor
**Color**: Red | **Model**: Opus

Executes database changes:
1. Cleans and restarts database (Docker)
2. Creates Flyway migration files
3. Runs migrations
4. Creates/updates Entity Framework models
5. Updates WodStratDbContext
6. Validates database structure

Commands used:
```bash
docker compose -f infra/docker-compose.yml down -v
docker compose -f infra/docker-compose.yml up -d postgres
docker compose -f infra/docker-compose.yml up flyway
dotnet build backend/WodStrat.sln
```

#### server-executor
**Color**: Blue | **Model**: Opus

Executes backend implementation:
1. Creates service interfaces and implementations
2. Implements DTOs and specifications
3. Creates/updates controllers
4. Implements request/response models
5. Configures dependency injection
6. Validates API endpoints

Pattern followed:
```
backend/src/WodStrat.Services/
├── Interfaces/IYourService.cs
├── Services/YourService.cs
└── Dtos/YourDto.cs

backend/src/WodStrat.Api/
├── Controllers/YourController.cs
└── ViewModels/YourViewModel.cs
```

#### ui-executor
**Color**: Blue | **Model**: Opus

Executes frontend implementation:
1. Creates TypeScript type definitions
2. Implements API service layer
3. Creates custom hooks
4. Builds React components (bottom-up)
5. Adds CSS styling
6. Validates TypeScript compilation

Pattern followed:
```
frontend/src/
├── types/your.ts
├── services/yourService.ts
├── hooks/useYour.ts
└── components/features/your/
    ├── YourComponent.tsx
    └── YourComponent.css
```

---

## Step-by-Step Workflow Guide

### Complete Feature Implementation

```bash
# Step 1: Define the feature (optional but recommended)
/jira-description WOD-123

# Step 2: Create comprehensive plans
/jira-plan WOD-123

# Step 3: Execute the implementation
/jira-execute WOD-123

# Step 4: Review implementation quality
/implementation-reviewer WOD-123

# Step 5: Generate tests (if review score >= 85%)
/test-implementer WOD-123

# Step 6: Finalize and create PR
/finalize WOD-123

# Step 7: Handle PR feedback (after code review)
/pr-comments WOD-123
```

### Handling Issues

**If planning confidence is low**:
- Answer the clarifying questions
- The planner will re-run with additional context

**If execution fails at a phase**:
- Review the error message
- Choose to retry, skip, or abort
- Fix issues manually if needed

**If implementation review score < 85%**:
- Address critical issues listed in the review
- Re-run `/implementation-reviewer` after fixes

---

## Directory Structure

After running the workflow, your `.work/` directory will contain:

```
.work/
└── WOD-123/
    ├── implementation.md           # Master plan (created by /jira-plan)
    ├── schema_changes.md          # Database plan (database-schema-planner)
    ├── services_changes.md        # Service plan (service-layer-planner)
    ├── api_changes.md             # API plan (api-planner)
    ├── ui_changes.md              # UI plan (ui-planner)
    ├── execution_report.md        # Execution results (created by /jira-execute)
    ├── FINAL_REPORT.md            # Final summary (created by /finalize)
    └── review/
        ├── implementation_review.md    # Quality review (/implementation-reviewer)
        └── test_implementation.md      # Test report (/test-implementer)
```

---

## Confidence Assessment System

The workflow uses a confidence-driven approach to ensure quality:

### Confidence Levels

| Level | Percentage | Meaning | Action |
|-------|------------|---------|--------|
| **High** | 85-100% | Clear requirements, familiar patterns | Proceed |
| **Medium** | 70-84% | Some ambiguity, may need clarification | Questions asked |
| **Low** | <70% | Significant unknowns | Must clarify before proceeding |

### How Confidence is Calculated

Each planner assesses confidence based on:
- **Clarity of requirements** (25%)
- **Understanding of integration points** (25%)
- **Technical implementation clarity** (25%)
- **Risk assessment completeness** (25%)

### Iterative Refinement

If confidence is below threshold:
1. Agent generates specific clarifying questions
2. User provides answers
3. Agent replans with additional context
4. Repeat until confidence is high enough

---

## Best Practices

### Providing Good Context

1. **Clear JIRA descriptions**: Include acceptance criteria and edge cases
2. **UI mockups**: Provide screenshots or wireframes in `ui_requirements.md`
3. **Answer questions thoroughly**: The more context, the better the plans

### When to Use Each Command

| Scenario | Command |
|----------|---------|
| New feature from scratch | Full workflow: `/jira-plan` → `/jira-execute` → `/finalize` |
| Backend-only feature | Skip UI planner/executor |
| Quick fix | May not need full workflow |
| PR feedback iteration | `/pr-comments` |

### Handling Failures

1. **Read error messages carefully** - They contain debugging information
2. **Check prerequisites** - Ensure previous phases completed successfully
3. **Manual intervention** - Some edge cases may need human fixes
4. **Retry options** - Most phases offer retry capability

### Quality Gates

Before moving between phases, ensure:
- Planning → Execution: All plan documents exist with high confidence
- Execution → Review: `execution_report.md` shows success
- Review → Testing: Implementation score ≥ 85%
- Testing → Finalize: All tests pass
- Finalize → Merge: PR approved by reviewer

### Tips for Success

1. **Start with clear requirements** - Garbage in, garbage out
2. **Review planning documents** - Catch issues before execution
3. **Don't skip the reviewer** - It catches issues before testing
4. **Keep JIRA updated** - The workflow updates status automatically
5. **Use PR comments command** - It saves time on feedback loops

---

## Quick Reference Card

```
┌─────────────────────────────────────────────────────────────────┐
│                    WORKFLOW QUICK REFERENCE                     │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  COMMANDS                          AGENTS                       │
│  ────────                          ──────                       │
│  /jira-description  Define         database-schema-planner      │
│  /jira-plan         Plan           service-layer-planner        │
│  /jira-execute      Build          api-planner                  │
│  /implementation-   Review         ui-planner                   │
│    reviewer                        database-executor            │
│  /test-implementer  Test           server-executor              │
│  /finalize          Ship           ui-executor                  │
│  /pr-comments       Iterate                                     │
│                                                                 │
│  QUALITY GATES                     FILES CREATED                │
│  ─────────────                     ─────────────                │
│  Confidence: 80%+                  implementation.md            │
│  Review Score: 85%+                schema_changes.md            │
│  Test Pass: 100%                   services_changes.md          │
│                                    api_changes.md               │
│  TYPICAL FLOW                      ui_changes.md                │
│  ────────────                      execution_report.md          │
│  /jira-plan WOD-123                implementation_review.md     │
│  /jira-execute WOD-123             test_implementation.md       │
│  /implementation-reviewer WOD-123  FINAL_REPORT.md              │
│  /test-implementer WOD-123                                      │
│  /finalize WOD-123                                              │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## Troubleshooting

### Common Issues

| Issue | Solution |
|-------|----------|
| "Work directory not found" | Run `/jira-plan` first |
| "Missing planning documents" | Ensure `/jira-plan` completed successfully |
| Low confidence scores | Provide more detailed requirements |
| Execution phase fails | Check error messages, fix manually, retry |
| Tests failing | Review test output, fix implementation |
| PR creation fails | Ensure git branch is pushed |

### Getting Help

- Check the command/agent markdown files in `.claude/commands/` and `.claude/agents/`
- Review generated plan files for detailed specifications
- Manual intervention is always possible for edge cases
