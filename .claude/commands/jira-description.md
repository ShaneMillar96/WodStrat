# JIRA Technical Description Generator

This command helps create or update JIRA tickets with technical implementation descriptions based on feature requirements.

## Usage

- `/jira-description` - Creates a new JIRA ticket
- `/jira-description WOD-123` - Updates existing ticket WOD-123

## Command Flow

When `/jira-description` is invoked, follow these steps:

1. **Determine Operation Mode**
   - Check if `$ARGUMENTS` is provided:
     - If `$ARGUMENTS` exists (e.g., "WOD-123"), proceed in UPDATE mode with the ticket ID
     - If `$ARGUMENTS` is empty, proceed in CREATE mode for a new ticket

2. **Get Feature Description**
   Ask: "Please enter the feature description:"
   - Wait for user to provide detailed feature requirements

3. **Assess Understanding & Confidence Level**
   Analyze the feature description against current codebase understanding:

   a. **Calculate Confidence Level** based on:
      - Clarity of feature requirements (25%)
      - Understanding of integration points (25%)
      - Technical implementation clarity (25%)
      - Risk assessment completeness (25%)

   b. **Display Confidence Assessment**:
      - Show current confidence level as percentage
      - If confidence < 80%, proceed to iterative questioning
      - If confidence ≥ 80%, proceed to technical description generation

   c. **Iterative Questioning Process** (when confidence < 80%):
      Ask targeted follow-up questions in areas where confidence is low:

      **Integration & Architecture Questions:**
      - "Which existing services/controllers will this feature integrate with?"
      - "Does this require changes to the database schema?"
      - "Will this affect the authentication/authorization flow?"
      - "How should this integrate with the existing workout/athlete systems?"

      **Technical Implementation Questions:**
      - "What specific user interactions are required?"
      - "Are there performance requirements (response time, data volume)?"
      - "Should this follow existing patterns in the codebase (clean architecture, DI, etc.)?"
      - "Does this need both backend API and frontend UI changes?"
      - "What data validation and business rules are needed?"

      **Data & Business Logic Questions:**
      - "What workout/athlete data will this feature use?"
      - "Are there specific fitness calculations or analytics required?"
      - "What validation and business rules are needed?"
      - "How should this handle error cases and edge scenarios?"

      **Security & Compliance Questions:**
      - "What user authorization is required?"
      - "Are there audit trail or logging requirements?"
      - "Does this handle sensitive user data?"

      After each set of answers, recalculate confidence level and continue until ≥ 80%

4. **Generate Technical Description**
   Based on the feature description and current codebase context:

   a. **Analyze the codebase** to understand:
      - Current architecture and patterns
      - Relevant existing components/modules
      - Technology stack and frameworks in use
      - Testing patterns and conventions
      - Code style and structure

   b. **Generate a comprehensive technical description** including:
      - **Overview**: High-level summary of the feature
      - **Technical Requirements**: Specific technical needs and constraints
      - **Implementation Steps**: Detailed breakdown of development tasks
      - **Files/Components to Modify**: Specific files or components that need changes
      - **Dependencies**: Any new libraries or tools needed
      - **Testing Strategy**: Unit tests, integration tests, and manual testing required
      - **Potential Risks/Considerations**: Technical challenges or edge cases
      - **Acceptance Criteria**: Clear definition of done
      - **Confidence Assessment**: Include final confidence level and key assumptions made

5. **JIRA Integration**
   Using the MCP JIRA tools (configured via atlassian MCP server):

   - **For UPDATE mode (ticket ID provided in $ARGUMENTS)**:
     - First, use `jira_get_issue` MCP tool to fetch the existing ticket details:
       - Pass the ticket ID from $ARGUMENTS (e.g., "WOD-123")
       - Review the current ticket state and existing description
     - Then, use `jira_update_issue` MCP tool to update the ticket:
       - Pass the ticket ID and the generated technical description
       - Preserve any existing ticket metadata (assignee, status, etc.)

   - **For CREATE mode (no arguments)**:
     - Use `jira_create_issue` MCP tool to create a new ticket:
       - Set project key to "WOD"
       - Use appropriate issue type (Story, Task, etc.)
       - Set the summary from the feature overview
       - Include all generated content in the description field

   **Available MCP JIRA Tools:**
   - `jira_get_issue` - Retrieve ticket details by ID
   - `jira_create_issue` - Create a new JIRA ticket
   - `jira_update_issue` - Update an existing ticket's fields
   - `jira_add_comment` - Add a comment to a ticket
   - `jira_get_issue_comments` - Get all comments on a ticket
   - `jira_get_transitions` - Get available status transitions
   - `jira_download_attachments` - Download ticket attachments

6. **Confirmation**
   Provide feedback to the user:
   - Confirm successful ticket creation/update
   - Include the ticket URL or ID
   - Summarize the key technical points added

## Technical Description Format

The generated technical description should follow this structure:

```
## Feature Overview
[Brief summary of what needs to be built]

## Technical Requirements
- [Specific technical needs]
- [Performance requirements]
- [Compatibility requirements]

## Implementation Plan

### Phase 1: Database & Backend
- [ ] Database schema changes (if needed)
- [ ] Entity models and migrations
- [ ] Service layer implementation
- [ ] API endpoints

### Phase 2: Frontend UI
- [ ] React components
- [ ] Custom hooks
- [ ] API integration
- [ ] Styling

### Phase 3: Testing & Integration
- [ ] Unit tests
- [ ] Integration tests
- [ ] Manual testing

## Files to Modify/Create

### Backend
- `backend/src/WodStrat.Dal/Models/[Entity].cs` - [Purpose]
- `backend/src/WodStrat.Services/Services/[Service].cs` - [Purpose]
- `backend/src/WodStrat.Api/Controllers/[Controller].cs` - [Purpose]

### Frontend
- `frontend/src/components/[Component].tsx` - [Purpose]
- `frontend/src/hooks/use[Hook].ts` - [Purpose]
- `frontend/src/services/[service].ts` - [Purpose]

## Dependencies
- [New packages/libraries needed]
- [Version requirements]

## Testing Strategy
- Unit Tests: [What to test]
- Integration Tests: [What to test]
- Manual Testing: [Steps to verify]

## Acceptance Criteria
- [ ] [Specific deliverable 1]
- [ ] [Specific deliverable 2]

## Confidence Assessment
- **Final Confidence Level**: [XX%]
- **Key Assumptions Made**: [List critical assumptions]
- **Areas Requiring Clarification During Development**: [List any remaining uncertainties]

## Risks & Considerations
- [Technical challenges]
- [Potential breaking changes]
- [Performance impacts]
```

## Error Handling

- If JIRA connection fails, inform the user and provide the generated description for manual entry
- If ticket ID is not found (UPDATE mode), ask user to verify the ticket ID and try again
- If feature description is too vague, ask follow-up questions for clarification
- If $ARGUMENTS is provided but doesn't look like a valid ticket ID format, inform the user about the expected format (e.g., "WOD-123")
- If confidence level cannot reach 80% after multiple rounds of questions, proceed with a warning about potential gaps in technical understanding
- If user provides contradictory information during questioning, ask for clarification before proceeding
