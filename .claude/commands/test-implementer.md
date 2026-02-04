# Test Implementer

This command generates comprehensive test coverage for implemented features, including unit tests for service and API layers, frontend tests, integration tests, and end-to-end test scenarios based on planning documents and implementation review results.

## Usage

- `/test-implementer <JIRA-TICKET-ID>` - Generates comprehensive test coverage for the specified JIRA ticket implementation

Example: `/test-implementer WOD-456`

## Command Flow

When `/test-implementer` is invoked, follow these steps:

### 1. **Setup Testing Environment**

a. **Input Validation**:
   - Ensure `$ARGUMENTS` contains a valid JIRA ticket ID (e.g., "WOD-456")
   - If no arguments provided, ask: "Please provide a JIRA ticket ID (e.g., WOD-456)"

b. **Validate Testing Prerequisites**:
   - Check that `/.work/$ARGUMENTS/` directory exists
   - Verify `implementation.md` exists (planning completed)
   - Verify `execution_report.md` exists (execution completed)
   - Check for `/.work/$ARGUMENTS/review/implementation_review.md` (review preferred but not required)
   - Confirm implementation files exist in codebase

c. **Analyze Implementation Scope**:
   - Parse planning documents to understand testing requirements
   - Read execution report to identify implemented components
   - Review implementation review (if available) to understand any issues
   - Determine test scenarios and coverage requirements

### 2. **Test Strategy Analysis**

a. **Extract Testing Requirements from Plans**:
   - **From services_changes.md**:
     - Service methods that need unit testing
     - Business logic scenarios to cover
     - Error conditions and edge cases
     - Mock requirements and dependencies

   - **From api_changes.md**:
     - Controller endpoints that need testing
     - Request/response validation scenarios
     - Authorization testing requirements
     - Integration testing needs

   - **From schema_changes.md**:
     - Database integration testing requirements
     - Data validation scenarios
     - Entity relationship testing

   - **From ui_changes.md** (if applicable):
     - Component rendering tests
     - Hook behavior tests
     - API integration tests
     - User interaction tests

b. **Identify Test Categories**:
   - **Unit Tests**: Service layer business logic, individual methods
   - **Controller Tests**: API endpoint behavior, request/response handling
   - **Frontend Tests**: Component rendering, hook behavior, API integration
   - **Integration Tests**: Database operations, service-to-API flow, frontend-to-backend
   - **End-to-End Tests**: Complete user scenarios and workflows

### 3. **Backend Unit Test Generation** (Service Layer)

a. **Service Class Testing**:
   - Generate test classes for each new/modified service
   - Follow existing test patterns in `backend/tests/` (when established)
   - Use NSubstitute for mocking dependencies
   - Use AutoFixture with custom configurations for test data

   ```csharp
   public class YourServiceTests
   {
       private readonly IFixture _fixture;
       private readonly IWodStratDatabase _database;
       private readonly YourService _yourService;

       public YourServiceTests()
       {
           _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
           _fixture.Customize(new YourEntityCustomization()); // If needed

           _database = Substitute.For<IWodStratDatabase>();
           _yourService = new YourService(_database);
       }

       [Fact]
       public async Task GetYourDataAsync_ValidId_ReturnsDto()
       {
           // Arrange
           var entity = _fixture.Create<YourEntity>();

           var queryable = new[] { entity }.AsQueryable().BuildMock();
           _database.Get<YourEntity>().Returns(queryable);

           // Act
           var result = await _yourService.GetYourDataAsync(entity.Id);

           // Assert
           result.Should().NotBeNull();
           result.Id.Should().Be(entity.Id);
       }

       [Fact]
       public async Task GetYourDataAsync_InvalidId_ThrowsKeyNotFoundException()
       {
           // Arrange
           var queryable = Array.Empty<YourEntity>().AsQueryable().BuildMock();
           _database.Get<YourEntity>().Returns(queryable);

           // Act & Assert
           await FluentActions.Invoking(() => _yourService.GetYourDataAsync(999))
               .Should().ThrowAsync<KeyNotFoundException>();
       }
   }
   ```

b. **Test Scenarios to Cover**:
   - Happy path scenarios with valid inputs
   - Edge cases and boundary conditions
   - Error scenarios (invalid inputs, not found, unauthorized)
   - Business rule validation scenarios

### 4. **Controller Test Generation** (API Layer)

a. **Controller Class Testing**:
   - Generate test classes for each new/modified controller
   - Follow existing patterns in `backend/tests/`
   - Mock service dependencies
   - Test authorization, request validation, response mapping

   ```csharp
   public class YourControllerTests
   {
       private readonly IFixture _fixture;
       private readonly IYourService _yourService;
       private readonly YourController _controller;

       public YourControllerTests()
       {
           _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());

           _yourService = Substitute.For<IYourService>();
           _controller = new YourController(_yourService);
       }

       [Fact]
       public async Task GetYourEntity_ValidId_ReturnsOkWithViewModel()
       {
           // Arrange
           var entityId = _fixture.Create<int>();
           var dto = _fixture.Create<YourDto>();

           _yourService.GetYourDataAsync(entityId).Returns(dto);

           // Act
           var result = await _controller.GetYourEntity(entityId);

           // Assert
           var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
           okResult.Value.Should().NotBeNull();
       }

       [Fact]
       public async Task GetYourEntity_NotFound_ReturnsNotFound()
       {
           // Arrange
           var entityId = _fixture.Create<int>();
           _yourService.GetYourDataAsync(entityId).ThrowsAsync<KeyNotFoundException>();

           // Act
           var result = await _controller.GetYourEntity(entityId);

           // Assert
           result.Should().BeOfType<NotFoundResult>();
       }
   }
   ```

b. **Controller Test Scenarios**:
   - Valid request scenarios with expected responses
   - Invalid request validation (model binding errors)
   - Authorization scenarios (authorized, unauthorized, forbidden)
   - Service exception handling (not found, validation errors)
   - Response model mapping validation

### 5. **Frontend Test Generation** (if UI changes)

a. **Component Testing**:
   - Test React components with React Testing Library
   - Verify component rendering and interactions
   - Test loading and error states

   ```typescript
   // Example: frontend/src/components/__tests__/YourComponent.test.tsx
   import { render, screen, waitFor } from '@testing-library/react';
   import userEvent from '@testing-library/user-event';
   import { YourComponent } from '../YourComponent';

   // Mock the service
   jest.mock('../../services/yourService', () => ({
     yourService: {
       getById: jest.fn(),
     },
   }));

   describe('YourComponent', () => {
     it('renders loading state initially', () => {
       render(<YourComponent id={1} />);
       expect(screen.getByText(/loading/i)).toBeInTheDocument();
     });

     it('renders data when loaded', async () => {
       const mockData = { id: 1, name: 'Test' };
       (yourService.getById as jest.Mock).mockResolvedValue(mockData);

       render(<YourComponent id={1} />);

       await waitFor(() => {
         expect(screen.getByText('Test')).toBeInTheDocument();
       });
     });

     it('renders error state on failure', async () => {
       (yourService.getById as jest.Mock).mockRejectedValue(new Error('Failed'));

       render(<YourComponent id={1} />);

       await waitFor(() => {
         expect(screen.getByText(/error/i)).toBeInTheDocument();
       });
     });
   });
   ```

b. **Hook Testing**:
   - Test custom hooks behavior
   - Verify state updates and side effects

   ```typescript
   // Example: frontend/src/hooks/__tests__/useYourData.test.ts
   import { renderHook, waitFor } from '@testing-library/react';
   import { useYourData } from '../useYourData';

   jest.mock('../../services/yourService');

   describe('useYourData', () => {
     it('returns loading state initially', () => {
       const { result } = renderHook(() => useYourData(1));
       expect(result.current.loading).toBe(true);
       expect(result.current.data).toBeNull();
     });

     it('returns data on success', async () => {
       const mockData = { id: 1, name: 'Test' };
       (yourService.getById as jest.Mock).mockResolvedValue(mockData);

       const { result } = renderHook(() => useYourData(1));

       await waitFor(() => {
         expect(result.current.loading).toBe(false);
         expect(result.current.data).toEqual(mockData);
       });
     });
   });
   ```

### 6. **Integration Test Generation**

a. **Database Integration Tests**:
   - Test database operations with real database context
   - Validate entity relationships and constraints
   - Test specification patterns with actual queries

b. **API Integration Tests**:
   - End-to-end API testing with test database
   - Request/response validation with real serialization
   - Authentication and authorization flow testing

c. **Frontend-Backend Integration Tests**:
   - Test frontend API calls against running backend
   - Verify data flow from UI to database
   - Test error handling across the stack

### 7. **Test Data Management**

a. **AutoFixture Customizations** (Backend):
   - Create customization classes for complex domain objects
   - Handle entity relationships and foreign keys
   - Ensure test data validity and consistency

   ```csharp
   public class YourEntityCustomization : ICustomization
   {
       public void Customize(IFixture fixture)
       {
           fixture.Customize<YourEntity>(c => c
               .With(x => x.Status, "Active")
               .With(x => x.CreatedDate, DateTime.UtcNow.AddDays(-1))
               .Without(x => x.Id)); // Let EF assign ID
       }
   }
   ```

b. **Mock Data Factories** (Frontend):
   - Create factory functions for test data
   - Support realistic data relationships
   - Enable easy test data variation

### 8. **Test Execution and Validation**

a. **Run Generated Tests**:
   ```bash
   # Backend tests
   dotnet test backend/WodStrat.sln

   # Frontend tests (when configured)
   cd frontend && npm test
   ```

b. **Integration with Existing Tests**:
   - Ensure new tests don't conflict with existing ones
   - Validate that existing tests still pass
   - Check for any test infrastructure updates needed

c. **Performance Testing**:
   - Add performance benchmarks for critical operations
   - Test with realistic data volumes
   - Validate query performance and optimization

### 9. **Manual API Endpoint Testing**

After unit tests pass, perform manual endpoint testing using curl against the local development environment with the local test user.

a. **Local Test Infrastructure**:
   - Test data is seeded via `db/local-migrations/V1000__seed_local_test_data.sql`
   - Run local migrations: `cd infra && docker compose --profile migrate-local run --rm flyway-local migrate`
   - Local test user credentials: `localtest@example.com` / `TestPassword123`
   - Test user has: Athlete (ID: 3), 12 benchmarks, 3 workouts (ForTime ID:7, AMRAP ID:8, EMOM ID:9)

b. **Start Backend for Testing**:
   ```bash
   # Ensure database is running
   cd infra && docker compose up -d postgres

   # Start backend (in background or separate terminal)
   cd backend && DB_CONNECTION_STRING="Host=localhost;Port=5432;Database=wodstrat;Username=wodstrat;Password=wodstrat_dev" dotnet run --project src/WodStrat.Api
   ```

c. **Authenticate and Get JWT Token**:
   ```bash
   # Get JWT token
   TOKEN=$(curl -s -X POST http://localhost:5000/api/auth/login \
     -H "Content-Type: application/json" \
     -d '{"email":"localtest@example.com","password":"TestPassword123"}' \
     | python3 -c "import sys,json; print(json.load(sys.stdin).get('token',''))")

   echo "Token: $TOKEN"
   ```

d. **Test Each New Endpoint**:
   For each endpoint implemented in the feature:

   ```bash
   # GET endpoint example
   curl -s -w "\nHTTP Status: %{http_code}\n" \
     http://localhost:5000/api/your-endpoint \
     -H "Authorization: Bearer $TOKEN"

   # POST endpoint example
   curl -s -w "\nHTTP Status: %{http_code}\n" \
     -X POST http://localhost:5000/api/your-endpoint \
     -H "Authorization: Bearer $TOKEN" \
     -H "Content-Type: application/json" \
     -d '{"field1":"value1","field2":123}'
   ```

e. **Validate Error Cases**:
   Test expected error responses:
   ```bash
   # Test 404 - Resource not found
   curl -s -w "\nHTTP Status: %{http_code}\n" \
     http://localhost:5000/api/endpoint/999 \
     -H "Authorization: Bearer $TOKEN"

   # Test 400 - Invalid request
   curl -s -w "\nHTTP Status: %{http_code}\n" \
     -X POST http://localhost:5000/api/endpoint \
     -H "Authorization: Bearer $TOKEN" \
     -H "Content-Type: application/json" \
     -d '{"invalidField":"value"}'

   # Test 401 - Unauthorized (no token)
   curl -s -w "\nHTTP Status: %{http_code}\n" \
     http://localhost:5000/api/endpoint
   ```

f. **Document Results**:
   Record in test report:
   - Each endpoint tested with HTTP status
   - Sample responses for successful calls
   - Error case validation results
   - Any issues discovered

g. **Troubleshooting**:
   - If endpoints return 404, restart the backend to register new routes
   - Verify database has test data: `psql` or check via API
   - Check backend logs for errors

### 10. **Generate Test Implementation Report**

Create `/.work/$ARGUMENTS/review/test_implementation.md`:

```markdown
# Test Implementation Report - $ARGUMENTS

Generated: [Current Date/Time]

## Test Coverage Summary
**Total Test Classes Generated**: [Count]
**Total Test Methods Created**: [Count]
**Test Categories**:
- Unit Tests (Service Layer): [Count] classes, [Count] methods
- Controller Tests (API Layer): [Count] classes, [Count] methods
- Frontend Tests (Components): [Count] classes, [Count] methods
- Frontend Tests (Hooks): [Count] classes, [Count] methods
- Integration Tests: [Count] classes, [Count] methods

## Backend Service Layer Testing

### Service Tests Created
[List of service test classes with coverage details]

#### YourServiceTests
- **Methods Tested**: [Count]
- **Scenarios Covered**:
  - Happy path: GetYourDataAsync with valid input
  - Error handling: GetYourDataAsync with invalid ID
  - Edge cases: Empty results, boundary conditions
- **Mock Dependencies**: IWodStratDatabase
- **Test Data**: AutoFixture with YourEntityCustomization

## Backend API Layer Testing

### Controller Tests Created
[List of controller test classes with coverage details]

#### YourControllerTests
- **Endpoints Tested**: [Count]
- **Scenarios Covered**:
  - GET /api/your/{id}: Valid ID, Not Found, Authorization
  - POST /api/your: Valid request, Invalid model, Conflict
  - PUT /api/your/{id}: Update success, Not found, Validation
- **Mock Dependencies**: IYourService
- **Response Validation**: Status codes, model mapping, error formats

## Frontend Testing (if applicable)

### Component Tests Created
[List of component test files]

#### YourComponent.test.tsx
- **Scenarios Covered**:
  - Initial loading state
  - Successful data display
  - Error state handling
  - User interactions

### Hook Tests Created
[List of hook test files]

#### useYourData.test.ts
- **Scenarios Covered**:
  - Loading state
  - Success state with data
  - Error state

## Integration Testing

### Database Integration Tests
- **Entity Relationship Testing**: [Details]
- **Specification Pattern Testing**: [Details]
- **Database Constraint Testing**: [Details]

### API Integration Tests
- **End-to-End Scenarios**: [Count] test cases
- **Authentication Flow Testing**: [Details]
- **Request/Response Serialization**: [Validation results]

### Frontend-Backend Integration Tests
- **API Integration**: [Count] test cases
- **Data Flow Validation**: [Details]

## Test Data Management

### AutoFixture Customizations Created (Backend)
[List of customization classes and their purposes]

### Mock Factories Created (Frontend)
[List of mock factory functions]

## Test Execution Results
- **Backend Unit Tests**: [Pass/Fail count] - [Success rate]%
- **Backend Controller Tests**: [Pass/Fail count] - [Success rate]%
- **Frontend Tests**: [Pass/Fail count] - [Success rate]%
- **Integration Tests**: [Pass/Fail count] - [Success rate]%
- **Overall**: [Total pass/fail] - [Overall success rate]%

## Manual API Endpoint Testing

### Test Setup
- **Test User**: `localtest@example.com` / `TestPassword123`
- **Test Athlete ID**: 3
- **Test Data**: [List relevant test entities created by local migration]

### Endpoint Test Results

| Endpoint | Method | HTTP Status | Result |
|----------|--------|-------------|--------|
| `/api/your-endpoint` | GET | ✅ 200 | Returns expected data |
| `/api/your-endpoint` | POST | ✅ 201 | Creates resource |
| [Add all tested endpoints] | | | |

### Error Case Validation

| Scenario | Expected | Actual | Result |
|----------|----------|--------|--------|
| Resource not found | 404 | 404 | ✅ Pass |
| Invalid request body | 400 | 400 | ✅ Pass |
| Unauthorized access | 401 | 401 | ✅ Pass |
| [Add all tested error cases] | | | |

### Sample Responses
[Include relevant JSON response samples for key endpoints]

## Code Coverage Analysis
- **Service Layer Coverage**: [Percentage]% of new service methods
- **Controller Coverage**: [Percentage]% of new controller actions
- **Frontend Coverage**: [Percentage]% of new components/hooks
- **Critical Path Coverage**: [Assessment of key scenarios]

## Quality Metrics
- **Test Maintainability**: [Assessment of test quality]
- **Naming Conventions**: [Compliance with existing patterns]
- **Documentation**: [Test documentation quality]

## Issues and Recommendations

### Test Gaps Identified
- [Gap 1]: [Description and recommendation]
- [Gap 2]: [Description and recommendation]

### Improvement Suggestions
- [Suggestion 1]: [Details]
- [Suggestion 2]: [Details]

## Files Created
### Backend Test Classes: [Count]
[List of test files with their purposes]

### Frontend Test Files: [Count]
[List of test files with their purposes]

### Supporting Infrastructure: [Count]
[List of customizations, builders, helpers created]

### Local Test Infrastructure
- `db/local-migrations/V1000__seed_local_test_data.sql` - Test user and data (local only)
- `infra/docker-compose.yml` - flyway-local service with migrate-local profile

## Next Steps
- All tests passing and coverage adequate: Proceed to `/finalize $ARGUMENTS`
- Issues found: Address test failures and gaps before finalization
- Performance concerns: Optimize before production deployment

---
**Test Implementation Confidence**: [High/Medium/Low]
**Recommendation**: [Proceed to Finalize/Address Issues/Enhance Coverage]
```

### 11. **Test Maintenance Documentation**

Provide guidance on:
- How to maintain and update tests as code evolves
- Test data management best practices
- Integration with CI/CD pipeline
- Performance testing guidelines

a. **Local Test Data Management**:
   - Local test migrations are in `db/local-migrations/` (not deployed to other environments)
   - Version numbers should start at V1000 to avoid conflicts with main migrations
   - To add new test data for a feature, create or update the seed migration
   - Run local migrations: `cd infra && docker compose --profile migrate-local run --rm flyway-local migrate`
   - The local test user has IDs starting at 3 to avoid conflicts with existing data

b. **Updating Test User Data**:
   - If new features require additional test data (e.g., new entity types), update `V1000__seed_local_test_data.sql`
   - Ensure foreign key relationships are maintained
   - Document any new test entities in the migration comments

## Quality Standards

- **Comprehensive Coverage**: All new functionality has corresponding tests
- **Realistic Scenarios**: Tests cover real-world usage patterns
- **Maintainable Code**: Tests are easy to understand and modify
- **Performance Aware**: Tests validate performance expectations
- **Integration Ready**: Tests work in automated pipeline

## Integration Points

- **Test Frameworks**: xUnit, NSubstitute, AutoFixture (Backend); Jest, React Testing Library (Frontend)
- **Database**: Test database setup and teardown
- **Local Test Data**: `db/local-migrations/` with flyway-local service (local environment only)
- **API Testing**: Integration with ASP.NET Core test host + manual curl testing
- **Frontend Testing**: Component and hook testing utilities
- **CI/CD**: Preparation for automated test execution
- **Coverage Tools**: Integration with code coverage analysis

This command ensures comprehensive test coverage that validates both functionality and quality while maintaining the established testing patterns and standards.
