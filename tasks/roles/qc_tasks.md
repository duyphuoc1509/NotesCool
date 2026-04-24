# QC Tasks — NotesCool MVP V1

## Source Documents
- `README.md`
- `docs/v1/prd.mvp-v1.md`
- `docs/v1/qa-checklist.mvp-v1.md`
- `docs/v1/acceptance-criteria.notes.md`
- `docs/v1/acceptance-criteria.tasks.md`

## QC-001 — Prepare MVP test plan

### Objective
Create a complete test plan for MVP V1 based on BA requirements and acceptance criteria.

### Scope
- Define testing scope for Auth, Notes, Tasks, Web client, and Mobile core flows.
- Identify test environments and test data requirements.
- Define regression scope for each release candidate.

### Deliverables
- Test plan document.
- Test scenario matrix.
- Risk-based testing priority.

## QC-002 — Test Auth and Ownership

### Objective
Verify authentication and data ownership rules.

### Scenarios
- Login with valid credentials.
- Login with invalid credentials.
- Access protected API without token.
- User A cannot view, update, archive/delete User B's note or task.
- Token expiry/refresh behavior if implemented.

### Expected Result
- Data leakage between users must not occur.
- Unauthorized and forbidden errors must match API conventions.

## QC-003 — Test Notes Module

### Objective
Verify Notes CRUD and search behavior.

### Scenarios
- Create note with valid data.
- Create note with missing/invalid data.
- View note list with pagination.
- Search notes by keyword.
- View note detail.
- Update note.
- Archive/delete note.
- Confirm archived notes are hidden from default list.
- Confirm user cannot access another user's note.

### Expected Result
- All scenarios follow BA acceptance criteria and API conventions.

## QC-004 — Test Tasks Module

### Objective
Verify Tasks CRUD, status, filter, and sort behavior.

### Scenarios
- Create task with valid data.
- Create task with invalid data.
- View task list with pagination.
- Filter tasks by status and due date if supported.
- Sort tasks by due date/created date if supported.
- View task detail.
- Update task.
- Change task status through all allowed transitions.
- Archive/delete task.
- Confirm archived tasks are hidden from default list.
- Confirm user cannot access another user's task.

### Expected Result
- Task status behavior must follow BA-defined state rules.

## QC-005 — Test Web UI flows

### Objective
Verify the web client supports all core MVP user flows.

### Scenarios
- Register/Login and navigate to app.
- Create, view, edit, search, archive notes from web UI.
- Create, view, edit, filter, sort, and update tasks from web UI.
- Validate empty, loading, and error states.
- Verify responsive layout on common viewport sizes.

## QC-006 — Test Mobile core flows

### Objective
Verify mobile app supports core Notes and Tasks flows.

### Scenarios
- Login/logout.
- Notes list/detail/create/edit/archive.
- Tasks list/detail/create/edit/status/archive.
- Validate basic mobile UX consistency with web.
- Validate offline/retry behavior only if included in scope.

## QC-007 — Regression and release readiness checklist

### Objective
Confirm MVP V1 is ready for demo/pilot.

### Checklist
- Auth works and protects user data.
- Notes core flow works end-to-end.
- Tasks core flow works end-to-end.
- Web and mobile clients consume consistent API contracts.
- No known blocker or critical bug remains open.
- Documentation and acceptance criteria are updated.
