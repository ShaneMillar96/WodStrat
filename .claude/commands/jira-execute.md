# JIRA Execution Orchestrator

This command orchestrates the implementation phase of the JIRA-driven development workflow, managing executor agents in proper sequence and ensuring successful feature delivery.

## Usage

- `/jira-execute <JIRA-TICKET-ID>` - Executes the implementation plan for the specified JIRA ticket

Example: `/jira-execute WOD-456`

## Command Flow

When `/jira-execute` is invoked, follow these steps:

### 1. **Validate Execution Environment**

a. **Input Validation**:
   - Ensure `$ARGUMENTS` contains a valid JIRA ticket ID (e.g., "WOD-456")
   - If no arguments provided, ask: "Please provide a JIRA ticket ID (e.g., WOD-456)"

b. **Verify Planning Artifacts**:
   - Check that `/.work/$ARGUMENTS/` directory exists
   - Verify `implementation.md` exists and is complete
   - Validate all planning documents are present:
     - `schema_changes.md` (required)
     - `services_changes.md` (required)
     - `api_changes.md` (required)
     - `ui_changes.md` (required for full-stack features)
   - If missing artifacts, prompt user to run `/jira-plan $ARGUMENTS` first

c. **Read Implementation Plan**:
   - Parse `implementation.md` to understand execution sequence
   - Extract confidence levels and risk assessments
   - Identify which executors are needed based on planning documents

### 2. **Sequential Executor Orchestration**

Execute in **strict sequence** to maintain dependencies:

#### **Phase 1: Database Implementation**
- **Executor**: Database Executor
- **Input**: `schema_changes.md`
- **Process**:
  ```
  Use Task tool with `database-executor` agent:
  - Provide path to schema_changes.md
  - Agent will clean database, create migrations, update models
  - Agent will run Flyway and validate database
  ```
- **Success Criteria**: All migrations successful, models updated, database validated
- **Failure Handling**: Stop execution, provide debugging information, ask if user wants to retry

#### **Phase 2: Service Implementation**
- **Executor**: Server Executor
- **Input**: `services_changes.md`
- **Process**:
  ```
  Use Task tool with `server-executor` agent:
  - Provide path to services_changes.md
  - Agent will implement service layer business logic
  - Agent will create DTOs, specifications, interfaces
  - Agent will register services in DI container
  ```
- **Success Criteria**: Services implemented, business logic working, tests passing
- **Failure Handling**: Stop execution, provide debugging information, ask if user wants to retry

#### **Phase 3: API Implementation**
- **Executor**: Server Executor
- **Input**: `api_changes.md`
- **Process**:
  ```
  Use Task tool with `server-executor` agent:
  - Provide path to api_changes.md
  - Agent will implement API layer controllers and endpoints
  - Agent will create ViewModels and mapping profiles
  - Agent will test all endpoints and validate functionality
  ```
- **Success Criteria**: APIs implemented, endpoints working, all tests pass
- **Failure Handling**: Stop execution, provide debugging information, ask if user wants to retry

#### **Phase 4: UI Implementation**
- **Executor**: UI Executor
- **Input**: `ui_changes.md`
- **Process**:
  ```
  Use Task tool with `ui-executor` agent:
  - Provide path to ui_changes.md
  - Agent will create React components, hooks, types
  - Agent will implement API integration services
  - Agent will create CSS/styling files
  - Agent will validate TypeScript compilation
  ```
- **Success Criteria**: All components render, TypeScript compiles, API calls work
- **Failure Handling**: Stop execution, provide debugging information, ask if user wants to retry

### 3. **Execution Monitoring and Progress Tracking**

Throughout each phase:

a. **Progress Reporting**:
   - Display current phase and executor being used
   - Show estimated completion based on planning documents
   - Report success/failure of each phase immediately

b. **Error Capture**:
   - Capture all errors from executor agents
   - Provide detailed debugging information
   - Offer retry options for failed phases

c. **Dependency Validation**:
   - Ensure each phase completes successfully before starting next
   - Validate that outputs from previous phases are available
   - Check for any breaking changes or conflicts

### 4. **Post-Execution Validation**

After all executors complete:

a. **Integration Testing**:
   - Run a basic smoke test of the implemented feature
   - Verify database, services, API, and UI layers work together
   - Test key user scenarios if specified in planning documents

b. **Code Quality Checks**:
   - Run linting and formatting tools if available
   - Check for any compilation errors (backend and frontend)
   - Validate that existing tests still pass

c. **Documentation Generation**:
   - Create execution summary with all changes implemented
   - Document any deviations from original plans
   - List all new endpoints, services, components, and database objects created

### 5. **Completion Status Update**

a. **Create Execution Report**:
   ```markdown
   # Execution Report - $ARGUMENTS

   Generated: [Current Date/Time]

   ## Execution Summary
   **Status**: [Success/Failed]
   **Duration**: [Total execution time]

   ## Phase Results
   ### Phase 1: Database Implementation
   - **Status**: [Success/Failed]
   - **Executor**: Database Executor
   - **Key Changes**:
     - Migration files: [List of files created]
     - Entity models: [List of models created/updated]
     - Database validation: [Results]

   ### Phase 2: Service Implementation
   - **Status**: [Success/Failed]
   - **Executor**: Server Executor
   - **Key Changes**:
     - Services: [List of services created/updated]
     - DTOs: [List of DTOs created]
     - Specifications: [List of specifications created]

   ### Phase 3: API Implementation
   - **Status**: [Success/Failed]
   - **Executor**: Server Executor
   - **Key Changes**:
     - Controllers: [List of controllers created/updated]
     - Endpoints: [List of new endpoints with URLs]
     - ViewModels: [List of view models created]

   ### Phase 4: UI Implementation
   - **Status**: [Success/Failed]
   - **Executor**: UI Executor
   - **Key Changes**:
     - Components: [List of React components created]
     - Hooks: [List of custom hooks created]
     - Types: [List of TypeScript types defined]
     - Services: [List of API service methods created]

   ## API Endpoints Created
   [List all new API endpoints with HTTP methods and URLs]

   ## UI Components Created
   [List all new React components with their purposes]

   ## Files Created/Modified
   ### New Files ([count])
   [List of new files with purposes]

   ### Modified Files ([count])
   [List of modified files with change summaries]

   ## Testing Results
   - Database validation: [Results]
   - API endpoint testing: [Results]
   - Frontend compilation: [Results]
   - Integration testing: [Results]

   ## Next Steps
   - Run `/implementation-reviewer $ARGUMENTS` for quality review
   - Run `/test-implementer $ARGUMENTS` for comprehensive testing
   - Run `/finalize $ARGUMENTS` when ready for production

   ## Issues Encountered
   [List any issues and their resolutions]
   ```

b. **Update JIRA Ticket**:
   - Update ticket status to "In Review" or appropriate status
   - Add execution summary to ticket comments
   - Link to implementation artifacts and documentation

c. **Provide User Summary**:
   - Confirm successful implementation completion
   - Highlight key features implemented
   - Provide next steps in the workflow
   - Include links to test the new functionality

### 6. **Error Recovery and Retry Logic**

a. **Phase Failure Handling**:
   - Capture complete error context from failed executor
   - Provide debugging information to user
   - Offer options:
     - Retry the failed phase
     - Skip to next phase (with warnings)
     - Abort execution entirely

b. **Partial Implementation Recovery**:
   - If execution fails mid-process, provide recovery options
   - Allow resuming from specific phase
   - Maintain state of completed phases

c. **Rollback Capabilities**:
   - For database failures, offer to rollback migrations
   - For code failures, offer to revert code changes
   - Provide git reset options if needed

## File Structure After Execution

```
/.work/$ARGUMENTS/
├── implementation.md           # Original master plan
├── execution_report.md         # Execution results (CREATED BY THIS COMMAND)
├── schema_changes.md          # Database plan
├── services_changes.md        # Service layer plan
├── api_changes.md             # API layer plan
├── ui_changes.md              # UI layer plan
└── review/                    # For review artifacts
    └── execution_summary.md   # Detailed execution log
```

## Integration Points

- **Task Tool**: Launches all executor agents in sequence
- **JIRA**: Updates ticket status and adds execution comments
- **Database**: Through database executor for schema changes
- **Backend**: Through server executor for services and API
- **Frontend**: Through UI executor for React components
- **Git**: May create commits or branches for implementation tracking

## Success Criteria

- All planned database changes are successfully implemented
- All service layer business logic is implemented and tested
- All API endpoints are implemented and respond correctly
- All UI components are implemented and rendering correctly
- No existing functionality is broken
- Code quality checks pass (backend and frontend)
- Integration testing validates the complete feature

This command ensures systematic, reliable implementation of planned features while maintaining quality and providing comprehensive feedback throughout the execution process.
