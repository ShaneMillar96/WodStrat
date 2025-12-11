# JIRA Planning Orchestrator

This command orchestrates the comprehensive planning phase of the JIRA-driven development workflow, managing specialized planner agents and ensuring high-confidence implementation plans.

## Usage

- `/jira-plan <JIRA-TICKET-ID>` - Creates comprehensive implementation plan for the specified JIRA ticket

Example: `/jira-plan WOD-456`

## Command Flow

When `/jira-plan` is invoked, follow these steps:

### 1. **Setup Planning Environment**

a. **Validate Input**:

- Ensure `$ARGUMENTS` contains a valid JIRA ticket ID (e.g., "WOD-456")
- If no arguments provided, ask: "Please provide a JIRA ticket ID (e.g., WOD-456)"

b. **Create Work Directory**:

- Create directory structure: `.work/$ARGUMENTS/`
- This will contain all planning documents for this ticket

c. **Fetch JIRA Details**:

- Using MCP JIRA connection, retrieve ticket details from the provided ticket ID
- Verify ticket exists and extract:
  - Title and description
  - Requirements and acceptance criteria
  - Any existing technical specifications
- If ticket not found, provide error and exit

d. **Create Feature Branch**:

- Check current git status and ensure working directory is clean
- Fetch latest changes from remote repository
- Checkout the `main` branch and pull latest changes
- Create new feature branch with format: `feature/$ARGUMENTS` (e.g., `feature/WOD-456`)
- Push the new branch to remote with upstream tracking
- Verify branch creation and confirm we're on the correct branch
- If branch creation fails, provide error and exit

### 2. **Orchestrate Planning Agents**

Launch all planner agents **in parallel** to maximize performance:

**IMPORTANT**: You MUST send a single message with multiple Task tool calls to run all agents in parallel.

a. **Database Schema Planner**:

- Use Task tool with `database-schema-planner` agent
- Provide JIRA ticket details and requirements
- Agent creates: `.work/$ARGUMENTS/schema_changes.md`

b. **Service Layer Planner**:

- Use Task tool with `service-layer-planner` agent
- Provide JIRA ticket details and `schema_changes.md`
- Agent creates: `.work/$ARGUMENTS/services_changes.md`

c. **API Planner**:

- Use Task tool with `api-planner` agent
- Provide JIRA ticket details and `services_changes.md`
- Agent creates: `.work/$ARGUMENTS/api_changes.md`

d. **UI Planner**:

- Use Task tool with `ui-planner` agent
- Provide JIRA ticket details, `api_changes.md`, and optional `ui_requirements.md`
- Agent creates: `.work/$ARGUMENTS/ui_changes.md`

**Note**: Since agents run in parallel, each agent should independently analyze the requirements and create their plans without depending on other agents' outputs. The confidence assessment phase will handle any interdependencies.

### 3. **Confidence Assessment Loop**

After all planners complete:

a. **Collect Confidence Ratings**:

- Read each planning document (schema_changes.md, services_changes.md, api_changes.md, ui_changes.md)
- Extract confidence levels from each plan
- Calculate overall confidence score

b. **Confidence Evaluation**:

- **High Confidence (85-100%)**: Proceed to step 4
- **Medium Confidence (70-84%)**: Ask clarifying questions and re-plan uncertain areas
- **Low Confidence (<70%)**: Generate detailed questions for human input

c. **Replanning Process** (if confidence is Medium/Low):

- Generate specific questions based on uncertainty areas from planning documents
- Present questions to user in structured format:

  ```
  ## Planning Questions - Confidence Too Low

  Based on the initial planning analysis, the following areas need clarification:

  ### Database/Schema Questions:
  - [Question 1 from schema planner]
  - [Question 2 from schema planner]

  ### Service Layer Questions:
  - [Question 1 from service planner]
  - [Question 2 from service planner]

  ### API Layer Questions:
  - [Question 1 from API planner]

  ### UI Layer Questions:
  - [Question 1 from UI planner]

  Please provide answers to improve planning confidence.
  ```

- Wait for user responses
- Re-run relevant planner agents with additional context
- Repeat confidence assessment

### 4. **Generate Master Implementation Plan**

When confidence is high, create `implementation.md`:

```markdown
# Implementation Plan - $ARGUMENTS

Generated: [Current Date/Time]

## Overview

[Summary of the feature and implementation approach]

## Confidence Assessment

**Overall Confidence**: [High] - [Percentage]%

### Individual Component Confidence:

- Database Schema: [Confidence from schema_changes.md]
- Service Layer: [Confidence from services_changes.md]
- API Layer: [Confidence from api_changes.md]
- UI Layer: [Confidence from ui_changes.md]

## Implementation Sequence

### Phase 1: Database Changes

- **Executor**: Database Executor
- **Input**: schema_changes.md
- **Deliverables**:
  - Flyway migration files executed
  - Entity models updated
  - Database validated

### Phase 2: Service Implementation

- **Executor**: Server Executor
- **Input**: services_changes.md
- **Deliverables**:
  - Service classes implemented
  - DTOs created
  - Business logic functional

### Phase 3: API Implementation

- **Executor**: Server Executor
- **Input**: api_changes.md
- **Deliverables**:
  - Controllers implemented
  - ViewModels created
  - Endpoints tested

### Phase 4: UI Implementation

- **Executor**: UI Executor
- **Input**: ui_changes.md
- **Deliverables**:
  - React components implemented
  - Custom hooks created
  - TypeScript types defined
  - API integration working

## Execution Command

Use `/jira-execute $ARGUMENTS` to begin implementation.

## Planning Documents

- Database: [Link to schema_changes.md]
- Services: [Link to services_changes.md]
- API: [Link to api_changes.md]
- UI: [Link to ui_changes.md]

## Risk Assessment

[Consolidated risks from all planning documents]

## Estimated Complexity

[Based on planner assessments]
```

### 5. **Final Validation**

Before completion:

- Verify all planning documents exist and have high confidence ratings
- Ensure implementation.md is comprehensive and actionable
- Confirm work directory contains all necessary files
- Check that execution sequence is logically ordered

### 6. **Completion Summary**

Provide user with:

- Confirmation of successful planning completion
- Overall confidence assessment
- Summary of key implementation highlights
- Next steps (run `/jira-execute $ARGUMENTS`)
- File locations and quick access links

## Error Handling

- **Invalid JIRA ID**: Prompt for correct format (e.g., "WOD-123")
- **JIRA Connection Failure**: Retry once, then ask user to provide ticket details manually
- **Git Branch Creation Failure**: Check for existing branch, dirty working directory, or network issues
- **Planner Agent Failures**: Capture errors, ask user if they want to retry or continue with available plans
- **Directory Creation Issues**: Check permissions and provide alternative locations
- **Low Confidence Loops**: Limit to 3 replanning iterations to prevent infinite loops

## File Structure Created

After successful execution:

```
.work/$ARGUMENTS/
├── implementation.md           # Master plan (THIS COMMAND CREATES)
├── schema_changes.md          # Database planner output
├── services_changes.md        # Service planner output
├── api_changes.md             # API planner output
├── ui_changes.md              # UI planner output
├── ui_requirements.md         # Optional UI specs (user provided)
└── review/                    # Created for future review artifacts
```

## Integration Points

- **JIRA**: Retrieves ticket details, may update planning status
- **Task Tool**: Launches all specialized planner agents
- **File System**: Creates work directory and validates all outputs
- **Context7**: Available to planners for documentation access (optional)
- **Git**: Creates feature branch and manages version control

This command serves as the central orchestrator ensuring all planning agents work in harmony to create high-confidence, implementation-ready plans for the full stack (backend + frontend).
