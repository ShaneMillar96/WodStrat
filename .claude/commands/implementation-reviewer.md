# Implementation Reviewer

This command provides comprehensive quality assurance by reviewing all planning documents, analyzing implemented changes against plans, and creating detailed feedback for implementation validation.

## Usage

- `/implementation-reviewer <JIRA-TICKET-ID>` - Reviews implementation against planning documents for the specified JIRA ticket

Example: `/implementation-reviewer WOD-456`

## Command Flow

When `/implementation-reviewer` is invoked, follow these steps:

### 1. **Setup Review Environment**

a. **Input Validation**:
   - Ensure `$ARGUMENTS` contains a valid JIRA ticket ID (e.g., "WOD-456")
   - If no arguments provided, ask: "Please provide a JIRA ticket ID (e.g., WOD-456)"

b. **Validate Review Prerequisites**:
   - Check that `/.work/$ARGUMENTS/` directory exists
   - Verify `implementation.md` exists (planning completed)
   - Verify `execution_report.md` exists (execution completed)
   - Confirm all planning documents are present:
     - `schema_changes.md` (required)
     - `services_changes.md` (required)
     - `api_changes.md` (required)
     - `ui_changes.md` (if UI changes were planned)
   - If missing artifacts, inform user of prerequisites

c. **Create Review Directory**:
   - Create `/.work/$ARGUMENTS/review/` directory if it doesn't exist
   - Prepare for review artifact storage

### 2. **Planning Document Analysis**

a. **Read All Planning Documents**:
   - Parse `implementation.md` for overall plan and confidence levels
   - Analyze `schema_changes.md` for database requirements
   - Examine `services_changes.md` for service layer requirements
   - Study `api_changes.md` for API layer requirements
   - Review `ui_changes.md` for UI layer requirements (if applicable)

b. **Extract Expected Deliverables**:
   - Database: Migration files, entity models, context updates
   - Services: Service classes, DTOs, specifications, interfaces
   - API: Controllers, request/response models, mapping profiles, endpoints
   - UI: React components, hooks, types, API services, styling

c. **Consolidate Requirements**:
   - Create comprehensive list of all expected changes
   - Note specific files, classes, methods that should be created/modified
   - Identify performance expectations and quality criteria

### 3. **Implementation Analysis**

a. **Git Difference Analysis**:
   - Run `git status` to see current changes
   - Run `git diff` to analyze specific code changes
   - Compare actual changes against planned changes
   - Identify any unexpected modifications or missing implementations

b. **File System Validation**:
   - Check for existence of all expected new files
   - Verify modifications to expected existing files
   - Look for unexpected file deletions or renames
   - Validate file organization follows planned structure

c. **Code Quality Review**:
   - Review database migration files for SQL quality and safety
   - Analyze entity models for proper relationships and data annotations
   - Check service implementations for Clean Architecture compliance
   - Validate API controllers follow RESTful conventions and authorization patterns
   - Verify React components follow TypeScript and React best practices
   - Check frontend hooks and services for proper patterns

### 4. **Plan-to-Code Validation**

a. **Database Layer Validation**:
   ```
   For each planned database change:
   - ✅ Migration file created with correct version number
   - ✅ SQL syntax is correct and follows conventions
   - ✅ Entity model matches database schema
   - ✅ Navigation properties are properly defined
   - ✅ Context updates include new DbSets
   - ❌ Issues: [List any problems found]
   ```

b. **Service Layer Validation**:
   ```
   For each planned service change:
   - ✅ Service interface created/updated with correct methods
   - ✅ Service implementation follows Clean Architecture
   - ✅ DTOs created with proper validation
   - ✅ Specifications implemented for complex queries
   - ✅ Async/await patterns used consistently
   - ❌ Issues: [List any problems found]
   ```

c. **API Layer Validation**:
   ```
   For each planned API change:
   - ✅ Controllers created/updated with proper endpoints
   - ✅ Request/response models implemented
   - ✅ Authorization attributes applied correctly
   - ✅ Mapping profiles created and functional
   - ✅ OpenAPI documentation present
   - ✅ Proper HTTP status codes and error handling
   - ❌ Issues: [List any problems found]
   ```

d. **UI Layer Validation** (if applicable):
   ```
   For each planned UI change:
   - ✅ React components created with TypeScript
   - ✅ Custom hooks implemented correctly
   - ✅ TypeScript types match API responses
   - ✅ API service methods implemented
   - ✅ CSS/styling follows established patterns
   - ✅ Components properly handle loading/error states
   - ❌ Issues: [List any problems found]
   ```

### 5. **Functional Testing Validation**

a. **API Endpoint Testing**:
   - Test all new endpoints manually
   - Verify request/response JSON structures match planned formats
   - Check authorization works correctly
   - Validate error scenarios return appropriate responses
   - Confirm performance meets expectations

b. **Frontend Testing**:
   - Verify TypeScript compilation succeeds
   - Test components render correctly
   - Verify API integration works with backend
   - Check loading and error states
   - Validate user interactions function properly

c. **Integration Testing**:
   - Test complete data flow from UI through API to database
   - Verify new functionality integrates with existing features
   - Check that database changes work correctly with backend
   - Validate frontend correctly displays backend data

d. **Regression Testing**:
   - Run existing unit tests to ensure no regressions
   - Test existing API endpoints to confirm they still work
   - Verify existing UI components still function properly
   - Check that database changes don't break existing queries

### 6. **Quality Assessment and Scoring**

a. **Calculate Implementation Score**:
   ```
   Implementation Completeness Score:
   - Database Implementation: [X/Y points] - [Percentage]%
   - Service Implementation: [X/Y points] - [Percentage]%
   - API Implementation: [X/Y points] - [Percentage]%
   - UI Implementation: [X/Y points] - [Percentage]% (if applicable)
   - Testing Validation: [X/Y points] - [Percentage]%

   Overall Implementation Score: [Total Points] - [Overall Percentage]%
   ```

b. **Quality Criteria Assessment**:
   - **Architectural Compliance**: Clean Architecture principles followed
   - **Code Quality**: Follows existing conventions and patterns
   - **Security**: Proper authorization and input validation
   - **Performance**: Meets performance expectations
   - **Testing**: Comprehensive validation completed
   - **Documentation**: Code is self-documenting and clear
   - **TypeScript**: Full type coverage, no `any` types

### 7. **Generate Comprehensive Review Report**

Create `/.work/$ARGUMENTS/review/implementation_review.md`:

```markdown
# Implementation Review Report - $ARGUMENTS

Generated: [Current Date/Time]
Reviewer: Claude Code Implementation Reviewer

## Executive Summary
**Overall Status**: [Pass/Pass with Issues/Fail]
**Implementation Score**: [Percentage]% ([Points]/[Total Points])
**Recommendation**: [Proceed to Testing/Address Issues First/Major Revision Needed]

## Plan Compliance Analysis

### Database Layer ✅/❌
**Score**: [X/Y] - [Percentage]%
**Status**: [Complete/Issues Found/Incomplete]

**Planned vs Implemented**:
- Migration V1.X__description.sql: ✅ Created and executed successfully
- Entity Model Updates: ✅ All models match database schema
- Context Changes: ✅ DbSets added correctly
- Foreign Key Relationships: ✅ Navigation properties working

**Issues Found**:
- [Issue 1]: [Description and recommendation]
- [Issue 2]: [Description and recommendation]

### Service Layer ✅/❌
**Score**: [X/Y] - [Percentage]%
**Status**: [Complete/Issues Found/Incomplete]

**Planned vs Implemented**:
- Service Interfaces: ✅ All interfaces created with correct signatures
- Service Implementations: ✅ Clean Architecture principles followed
- DTOs: ✅ All DTOs created with proper validation
- Specifications: ✅ Complex queries use specification pattern
- Dependency Injection: ✅ Services properly registered

**Issues Found**:
- [Issue 1]: [Description and recommendation]

### API Layer ✅/❌
**Score**: [X/Y] - [Percentage]%
**Status**: [Complete/Issues Found/Incomplete]

**Planned vs Implemented**:
- Controllers: ✅ All endpoints implemented correctly
- Request/Response Models: ✅ Proper validation and structure
- Authorization: ✅ Correct auth attributes applied
- Mapping Profiles: ✅ Mappings working correctly
- OpenAPI Documentation: ✅ All endpoints documented

**API Endpoint Testing Results**:
- GET /api/endpoint1: ✅ Working correctly
- POST /api/endpoint2: ✅ Working correctly
- PUT /api/endpoint3: ❌ Returns incorrect status code

### UI Layer ✅/❌ (if applicable)
**Score**: [X/Y] - [Percentage]%
**Status**: [Complete/Issues Found/Incomplete/Not Applicable]

**Planned vs Implemented**:
- React Components: ✅ All components created with TypeScript
- Custom Hooks: ✅ Hooks implemented correctly
- TypeScript Types: ✅ Types match API responses
- API Services: ✅ Service methods working
- Styling: ✅ CSS follows patterns
- Loading/Error States: ✅ Properly handled

**Issues Found**:
- [Issue 1]: [Description and recommendation]

## Code Quality Assessment

### Architectural Compliance: [Score]
- Clean Architecture separation maintained
- Dependency injection patterns followed
- Proper error handling implemented

### Performance Analysis: [Score]
- Database queries optimized with specifications
- Proper async/await usage throughout
- Frontend components optimized

### Security Validation: [Score]
- Authorization properly implemented on all endpoints
- No sensitive data exposure in responses
- Input validation comprehensive

## Regression Testing Results
- Existing unit tests: [Pass/Fail] - [Details]
- Existing API endpoints: [Pass/Fail] - [Details]
- Frontend compilation: [Pass/Fail] - [Details]
- Database integrity: [Pass/Fail] - [Details]

## Critical Issues Requiring Resolution
1. **[Issue Type]**: [Detailed description]
   - **Impact**: [Business/technical impact]
   - **Resolution**: [Specific steps to fix]
   - **Priority**: [High/Medium/Low]

## Recommendations

### Before Proceeding to Testing:
- [ ] [Action item 1 with specific details]
- [ ] [Action item 2 with specific details]

### Code Quality Improvements:
- [Improvement suggestion 1]
- [Improvement suggestion 2]

### Performance Optimizations:
- [Optimization suggestion 1]
- [Optimization suggestion 2]

## Next Steps
Based on this review:
- **If Score ≥ 90%**: Proceed to `/test-implementer $ARGUMENTS`
- **If Score 70-89%**: Address issues listed above, then proceed to testing
- **If Score < 70%**: Major revision required before testing

## Files Reviewed
### New Files Created: [Count]
[List of new files with brief descriptions]

### Modified Files: [Count]
[List of modified files with change summaries]

## Review Methodology
This review analyzed:
- Plan-to-code compliance for all layers
- Git changes against expected deliverables
- Manual testing of all API endpoints
- Frontend compilation and component rendering
- Code quality against established patterns
- Integration with existing functionality
- Regression testing validation

---
**Review Confidence**: [High/Medium/Low]
**Reviewer Recommendations**: [Summary recommendations]
```

### 8. **Provide Review Summary to User**

Deliver concise summary:
- Overall implementation status and score
- Critical issues that must be addressed
- Recommendations for next steps
- Confidence level in the implementation

## Error Handling

- **Missing Prerequisites**: Guide user to complete planning or execution first
- **Git Analysis Failures**: Provide alternative analysis methods
- **API Testing Failures**: Capture specific errors and suggest debugging steps
- **Frontend Build Failures**: Provide TypeScript error details
- **Review Artifact Creation Issues**: Handle file system or permission problems

## Integration Points

- **Git**: For analyzing implementation changes
- **File System**: For validating planned vs actual file changes
- **API Testing**: For functional validation of endpoints
- **Frontend Build**: For TypeScript compilation validation
- **Database**: For validating schema changes and data integrity
- **JIRA**: For updating review status and findings

This command provides comprehensive quality assurance ensuring implementation quality meets production standards before final testing and deployment phases.
