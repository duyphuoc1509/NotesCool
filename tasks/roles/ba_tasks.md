# BA Tasks — NotesCool MVP V1

## Source Documents
- `README.md`
- `docs/v1/prd.mvp-v1.md`
- `docs/v1/mvp-scope.md`
- `docs/v1/module-map.md`
- `docs/v1/api-conventions.md`
- `docs/v1/backlog.foundation.md`
- `docs/v1/backlog.notes.md`
- `docs/v1/backlog.tasks.md`
- `docs/v1/acceptance-criteria.notes.md`
- `docs/v1/acceptance-criteria.tasks.md`

## BA-001 — Finalize MVP scope and open decisions

### Objective
Clarify MVP V1 scope so Dev, FE, and QC do not need to guess business behavior.

### Scope
- Confirm Must-have, Should-have, and Not-now items.
- Confirm Auth MVP approach at business level.
- Confirm archive vs hard delete default behavior.
- Confirm whether tags and linked note-task belong to MVP V1 or Release 1.1.
- Confirm expected mobile parity with web.

### Deliverables
- Updated scope note or decision log.
- List of confirmed assumptions.
- List of deferred items.

### Acceptance Criteria
- Given the MVP scope is reviewed, when BA publishes the decision log, then every Must-have item has a clear in/out status.
- Given an item is deferred, when Dev/QC review the scope, then the target phase is visible.

## BA-002 — Write detailed Auth & Ownership requirement

### Objective
Define authentication and ownership behavior for personal Notes and Tasks.

### Scope
- User registration/login/logout expectation.
- JWT + refresh token behavior at business/API level.
- Data ownership rule for Notes and Tasks.
- Unauthorized and forbidden behavior.

### Deliverables
- Requirement document with user stories, business rules, validation rules, and acceptance criteria.

### Acceptance Criteria
- Given a user owns notes/tasks, when another user requests them, then system must not expose those records.
- Given an unauthenticated request, when protected APIs are called, then system returns an authentication error.

## BA-003 — Write detailed Notes module requirement

### Objective
Define Notes CRUD, search, archive/delete, and ownership behavior.

### Scope
- Create note.
- View note list with pagination and search.
- View note detail.
- Update note.
- Archive/delete note.
- Empty/loading/error business expectations for clients.

### Key Business Rules
- Notes belong to one owner.
- Note title/content validation must be explicit.
- Archived notes must not appear in default active list unless explicitly requested.

### Acceptance Criteria
- Given a valid authenticated user, when the user creates a note with required fields, then the note is saved under that user.
- Given a keyword search, when matching notes exist, then only matching notes owned by the user are returned.
- Given a note is archived, when the default note list is loaded, then the archived note is hidden.

## BA-004 — Write detailed Tasks module requirement

### Objective
Define Tasks CRUD, status update, filters, sorting, archive/delete, and ownership behavior.

### Scope
- Create task.
- View task list with pagination, filter, and sort.
- View task detail.
- Update task.
- Change task status.
- Archive/delete task.

### Key Business Rules
- Tasks belong to one owner.
- Minimum task statuses: `todo`, `in_progress`, `done`, `archived`.
- Status transition rules must be defined before implementation.
- Due date behavior must be explicit.

### Acceptance Criteria
- Given a user creates a task, when required data is valid, then the task is created with default status `todo`.
- Given a task exists, when owner changes status to a valid next status, then system updates the status and audit timestamps.
- Given filters are applied, when task list is loaded, then only owned tasks matching filters are returned.

## BA-005 — Define API behavior expectations for Dev handoff

### Objective
Translate business requirements into API behavior expectations without writing implementation.

### Scope
- Request/response behavior for Auth, Notes, and Tasks.
- Error behavior for validation, unauthorized, forbidden, not found, and conflict cases.
- Paging/filter/sort expectations.
- DTO field definitions at business level.

### Acceptance Criteria
- Given BE starts contract design, when reviewing BA docs, then every endpoint has expected actor, input, output, and error behavior.
- Given FE/Mobile starts UI integration, when reviewing BA docs, then empty/error/loading states are understood.

## BA-006 — Prepare QC scenario matrix

### Objective
Provide QC with testable scenarios mapped to requirements.

### Scope
- Positive scenarios.
- Negative validation scenarios.
- Permission/ownership scenarios.
- Edge cases for search, filter, pagination, status changes, archive/delete.

### Acceptance Criteria
- Given QC reviews the matrix, when creating test cases, then no core MVP flow is missing.
- Given an open question remains, when QC reviews the matrix, then the blocked scenario is clearly marked.
