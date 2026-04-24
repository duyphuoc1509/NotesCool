# FE Tasks — NotesCool MVP V1

## Source Documents
- `README.md`
- `docs/v1/prd.mvp-v1.md`
- `docs/v1/solution-structure.md`

## Tech Stack Target
- ReactJS, TypeScript
- Follow backend API contracts

## FE-001 — Initialize Web client project

### Objective
Setup the foundational React application structure.

### Scope
- Initialize project under `src/frontend`.
- Configure TypeScript, linter, and formatting rules.
- Set up routing (React Router) for base layouts (Auth Layout, App Layout).
- Establish folder structure: components, pages, hooks, services/api, utils.

## FE-002 — Implement Auth Flow UI

### Objective
Create login/registration screens and handle JWT tokens.

### Scope
- Build Login and Register forms.
- Implement API integration for authentication.
- Manage auth state (e.g., using Context or global store).
- Handle token storage, refresh logic (if applicable), and unauthenticated redirects.

### Constraints
- Ensure proper loading, error, and validation states on forms.

## FE-003 — Implement Notes UI

### Objective
Build the screens and components to manage Notes.

### Scope
- Build Notes List view (handle pagination, search).
- Build Note Detail view (view and edit mode).
- Build Create Note form/modal.
- Implement API integration for Notes CRUD.
- Handle UI states: empty, loading, error, and success notifications.

### Constraints
- Maintain clear separation from Tasks UI logic.
- Ensure smooth UX for edit and save actions.

## FE-004 — Implement Tasks UI

### Objective
Build the screens and components to manage Tasks.

### Scope
- Build Tasks List view (handle pagination, filter by status, sort).
- Build Task Detail view.
- Build Create Task form.
- Implement quick status toggle (e.g., check/uncheck for done).
- Implement API integration for Tasks CRUD.
- Handle UI states: empty, loading, error, and success notifications.

### Constraints
- Ensure the status transition UX is intuitive.
- Maintain clear separation from Notes UI logic.

## FE-005 — Global UI/UX Polish for MVP

### Objective
Ensure consistent and responsive user experience.

### Scope
- Refine navigation between Notes and Tasks.
- Ensure the layout is responsive (usable on mobile web as a fallback).
- Standardize button styles, inputs, typography, and color palette based on standard libraries or simple custom CSS.
- Verify error handling UX (toast messages or banners for API errors).
