# BE Tasks — NotesCool MVP V1

## Source Documents
- `README.md`
- `docs/v1/prd.mvp-v1.md`
- `docs/v1/solution-structure.md`
- `docs/v1/module-map.md`
- `docs/v1/api-conventions.md`

## Architecture & Tech Stack Target
- .NET 8, ASP.NET Core Web API, PostgreSQL, EF Core
- Modular monolith first: `Notes`, `Tasks`, `Shared`

## BE-001 — Initialize solution and foundational projects

### Objective
Setup backend solution structure based on `solution-structure.md`.

### Steps
1. Create global `.sln` file.
2. Initialize backend directory structure:
   - `src/backend/NotesCool.Api`
   - `src/backend/NotesCool.Shared`
   - `src/backend/NotesCool.Notes`
   - `src/backend/NotesCool.Tasks`
3. Add solution configurations and baseline Nuget packages (EF Core, JWT, etc).

### Constraints
- Enforce strict project references (Notes and Tasks depend on Shared, API depends on Notes and Tasks; Notes must not depend on Tasks, Tasks must not depend on Notes).

## BE-002 — Setup Shared Kernel (Authentication & Common)

### Objective
Establish the foundation for cross-cutting concerns and primitives.

### Scope
- Implement JWT Auth handler/middleware.
- Set up global Exception Handling and error response formatting (following `api-conventions.md`).
- Define base entities, repository interfaces, and paging/filter/sort record types.

### Constraints
- Shared module must not contain business logic specific to Notes or Tasks.

## BE-003 — Implement Notes Module Contract and EF Core Context

### Objective
Create API contracts and data access for Notes.

### Scope
- Define `Note` entity and EF Core configuration.
- Setup `NotesDbContext` or register `Note` dbset in a unified context.
- Create DTOs for Create, Update, and Note responses.
- Implement REST API endpoints for Notes (CRUD, Search, Pagination).
- Secure Notes endpoints with ownership checks.

### Constraints
- Ensure boundary-level input validation.
- Implement soft-delete (Archive) according to BA requirements.
- Prevent N+1 queries.

## BE-004 — Implement Tasks Module Contract and EF Core Context

### Objective
Create API contracts and data access for Tasks.

### Scope
- Define `TaskItem` entity and EF Core configuration.
- Setup `TasksDbContext` or register `TaskItem` dbset.
- Create DTOs for Create, Update, Status Change, and Task responses.
- Implement REST API endpoints for Tasks (CRUD, Filter, Sort, Pagination).
- Secure Tasks endpoints with ownership checks.

### Constraints
- Ensure boundary-level input validation.
- Define explicit task statuses based on BA requirements.
- Implement soft-delete (Archive).
- Prevent N+1 queries.

## BE-005 — Setup backend unit and contract tests baseline

### Objective
Ensure backend code reliability from the start.

### Scope
- Setup test projects: `tests/backend/NotesCool.Notes.Tests`, `tests/backend/NotesCool.Tasks.Tests`.
- Add baseline unit tests for key domain logic (status transitions, validation).
- Add basic integration/contract tests using WebApplicationFactory to verify endpoints return correct status codes and JSON structures.

### Constraints
- Tests must pass before PRs are merged.
- Focus on ownership validation tests to prevent cross-tenant data leaks.
