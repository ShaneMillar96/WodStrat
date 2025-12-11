# PR Comments Implementation Command

This command analyzes PR comments for a JIRA ticket and automatically implements requested changes based on work context and git branch analysis.

## Usage

- `/pr-comments <JIRA-TICKET-ID>` - Analyzes and implements PR comment feedback for the specified JIRA ticket

Example: `/pr-comments WOD-23`

## Command Flow

When `/pr-comments` is invoked, follow these steps:

### 1. **Input Validation and Setup**

a. **Input Validation**:
   - Ensure `$ARGUMENTS` contains a valid JIRA ticket ID (e.g., "WOD-23")
   - If no arguments provided, ask: "Please provide a JIRA ticket ID (e.g., WOD-23)"
   - Validate ticket ID format matches pattern: `[A-Z]+-\d+`

b. **Verify Work Directory**:
   - Check that `/.work/$ARGUMENTS/` directory exists
   - If missing, respond: "No work directory found for $ARGUMENTS. Please run /jira-plan $ARGUMENTS first."

c. **Verify Git Branch**:
   - Check current git branch matches expected pattern `feature/$ARGUMENTS`
   - If different branch, ask user to switch to correct branch or confirm they want to continue

### 2. **Context Building Phase**

a. **Read Work Documents**:
   Read and analyze all available documents from `/.work/$ARGUMENTS/`:
   - `implementation.md` - Master implementation plan and feature overview
   - `api_changes.md` - API endpoints, contracts, and request/response models
   - `services_changes.md` - Business logic and service layer changes
   - `schema_changes.md` - Database schema modifications
   - `ui_changes.md` - React components, hooks, and frontend changes
   - `execution_report.md` - What was actually implemented vs planned
   - `FINAL_REPORT.md` - Final implementation summary (if exists)

b. **Extract Context**:
   From these documents, build understanding of:
   - Feature purpose and business requirements
   - Technical implementation approach
   - Key files and components involved
   - Database schema changes made
   - API endpoints created/modified
   - Service methods implemented
   - React components and hooks created
   - TypeScript types defined

### 3. **Git Branch Analysis**

a. **Branch Information**:
   ```bash
   # Get current branch
   git branch --show-current

   # Get commit history for this branch
   git log --oneline main..HEAD

   # Get full diff from main branch
   git diff main..HEAD
   ```

b. **Code Change Analysis**:
   - Map actual code changes to planned implementation
   - Identify files created, modified, or deleted
   - Understand the scope of changes made
   - Note any deviations from the original plan

### 4. **PR Comment Fetching and Analysis**

a. **Find Associated PR**:
   ```bash
   # Find PR for current branch
   gh pr list --state open --head $(git branch --show-current) --json number,title,url
   ```

b. **Fetch Unresolved PR Comments**:
   ```bash
   # Get unresolved review comments using GraphQL API
   gh api graphql -f owner=":owner" -f repo=":repo" -F pr="<PR_NUMBER>" -f query='
     query FetchUnresolvedComments($owner: String!, $repo: String!, $pr: Int!) {
       repository(owner: $owner, name: $repo) {
         pullRequest(number: $pr) {
           reviewThreads(first: 100) {
             edges {
               node {
                 id
                 isResolved
                 isOutdated
                 path
                 line
                 comments(first: 100) {
                   nodes {
                     id
                     author { login }
                     body
                     url
                     createdAt
                   }
                 }
               }
             }
           }
         }
       }
     }
   ' | jq '.data.repository.pullRequest.reviewThreads.edges | map(select(.node.isResolved == false))'

   # Also get general issue comments (these don't have resolution status)
   gh pr view <PR_NUMBER> --json comments
   ```

c. **Parse and Categorize Unresolved Comments**:
   For each unresolved comment, determine:
   - **Actionable Code Changes**: Specific requests for code modifications
   - **Questions**: Requests for clarification or explanation
   - **Suggestions**: Optional improvements or alternative approaches
   - **Non-Technical**: Comments about process, documentation, etc.

   Note: Only unresolved comments are processed - resolved conversations are automatically filtered out.

d. **Comment Analysis**:
   Extract from each actionable comment:
   - File path referenced (if any)
   - Line number or code section
   - Specific change requested
   - Context around the issue
   - Severity/importance level

### 5. **Change Implementation Phase**

For each actionable comment:

a. **Change Assessment**:
   - Determine if the change is technically feasible
   - Check if it conflicts with other requirements
   - Assess impact on existing functionality
   - Verify the change aligns with the overall feature plan

b. **Code Modification**:
   - Use Read tool to examine current code
   - Use Edit tool to implement the requested change
   - Follow existing code patterns and conventions
   - Maintain consistency with the codebase architecture
   - Ensure TypeScript compliance for frontend changes

c. **Validation**:
   - Ensure the change compiles/builds successfully
   - Verify the change doesn't break existing functionality
   - Check that the change addresses the comment completely

### 6. **PR Interaction Phase**

a. **Optional Reply to Comments**:
   For complex changes, optionally add a brief reply to indicate work done:
   ```bash
   gh pr review <PR_NUMBER> --comment --body "✅ Addressed: [brief description of change made] - please review and resolve manually"
   ```

b. **Comment Tracking**:
   - Keep track of which comments have been addressed through code changes
   - Note any comments that couldn't be automatically implemented
   - Track any new questions or issues that arise
   - **Important**: Do NOT automatically mark comments as resolved - leave for human reviewer

### 7. **Summary and Reporting**

a. **Generate Implementation Report**:
   ```markdown
   # PR Comments Implementation Report - $ARGUMENTS

   Generated: [Current Date/Time]
   PR: [PR URL and number]

   ## Context Analysis
   **Feature**: [Feature name and purpose]
   **Files Analyzed**: [List of work documents read]
   **Git Changes**: [Summary of branch changes]

   ## Unresolved Comments Analysis
   **Total Unresolved Review Threads**: [count]
   **Total General Comments**: [count]
   **Actionable Code Changes**: [count]
   **Questions/Clarifications**: [count]
   **Non-Technical Comments**: [count]

   ## Changes Implemented

   ### Successfully Implemented ([count])
   [For each implemented change:]
   - **Comment**: [Brief summary of request]
   - **File**: [File modified]
   - **Change**: [Description of change made]
   - **Status**: ✅ Implemented (requires manual resolution by reviewer)

   ### Could Not Implement ([count])
   [For each unimplemented change:]
   - **Comment**: [Brief summary of request]
   - **Reason**: [Why it couldn't be implemented]
   - **Manual Action Required**: [What needs human attention]

   ### Files Modified
   [List all files that were changed with brief descriptions]

   ## Next Steps
   - Review implemented changes manually
   - Test the functionality to ensure changes work correctly
   - **Manually resolve addressed comments in GitHub PR interface**
   - Address any remaining comments that require human intervention
   - Build and test:
     - Backend: `dotnet build backend/WodStrat.sln`
     - Frontend: `cd frontend && npm run build`
   - Push changes: `git add -A && git commit -m "feat: implement PR feedback for $ARGUMENTS"`

   ## Important Notes
   - All processed comments were UNRESOLVED at the time of analysis
   - Comments require MANUAL RESOLUTION by the reviewer after verification
   - No comments were automatically marked as resolved by this command
   ```

b. **Display Summary to User**:
   - Highlight successful implementations of unresolved feedback
   - Call attention to any comments requiring manual intervention
   - Remind user to manually resolve comments in GitHub after verification
   - Provide next steps for completing the PR review process

### 8. **Error Handling and Edge Cases**

a. **Missing PR**:
   - If no PR found for the branch, inform user and suggest creating one first
   - Provide command to create PR if needed

b. **No Unresolved Comments**:
   - If PR has no unresolved comments, inform user that no changes are needed
   - All feedback has already been addressed and resolved
   - Suggest reviewing the PR manually to ensure it's ready for merge

c. **API Rate Limits**:
   - Handle GitHub API rate limiting gracefully
   - Provide fallback options or suggest retrying later

d. **Conflicting Changes**:
   - If requested changes conflict with each other, flag for manual review
   - Don't implement potentially conflicting changes automatically

e. **Build/Compilation Errors**:
   - If changes cause compilation errors, revert and flag for manual intervention
   - Provide error details to help with manual resolution

## Integration Points

- **GitHub CLI**: For PR comment fetching and interaction
- **Git**: For branch analysis and change tracking
- **File System**: For reading work documents and modifying code
- **Work Directory**: For understanding feature context and implementation
- **Build Tools**: For validation (dotnet build, npm run build)

## Success Criteria

- All unresolved PR comments are identified and analyzed correctly
- Only unresolved feedback is processed - resolved comments are ignored
- Implementable changes are successfully applied to the codebase
- A comprehensive report is generated showing what was accomplished
- User is clearly informed of manual resolution requirement
- User is clearly informed of any manual actions still required
- Code quality and functionality are maintained throughout the process
- No comments are automatically resolved - manual verification preserved

This command streamlines the PR review feedback loop by automatically implementing unresolved reviewer suggestions while maintaining human oversight and control over the comment resolution process.
