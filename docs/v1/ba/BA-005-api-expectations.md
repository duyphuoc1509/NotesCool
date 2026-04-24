# BA-005 — API Behavior Expectations

## 1. Overview

This document translates MVP V1 business requirements into API behavior expectations for Backend, FE, Mobile, and QC. It does not prescribe implementation details.

## 2. General API Expectations

- Base path: `/api/v1`.
- JSON fields: camelCase.
- Success responses: top-level `data` envelope.
- List responses: `data.items` + `data.page` metadata.
- Error responses: consistent error code/message/details format as defined in `api-conventions.md`.
- Protected endpoints require Bearer access token.
- Server infers user/owner from access token; client must not send ownerId.

## 3. Auth API Expectations

| Endpoint | Actor | Input | Success | Error |
|---|---|---|---|---|
| `POST /auth/register` | Guest | email, password | 201/200 with auth/session data | 400 validation, 409 email exists |
| `POST /auth/login` | Guest | email, password | 200 with accessToken, refreshToken/session info | 400 validation, 401 invalid credentials |
| `POST /auth/refresh` | User/session | refreshToken | 200 with new accessToken | 401 invalid/expired refresh |
| `POST /auth/logout` | Authenticated User | token/session | 204/200 | 401 invalid token |

## 4. Notes API Expectations

| Endpoint | Actor | Input | Success | Error |
|---|---|---|---|---|
| `POST /notes` | Auth User | title, content | 201 note DTO | 400 validation, 401 |
| `GET /notes` | Auth User | page, size, q, archived? | 200 list of owned notes | 400 invalid paging, 401 |
| `GET /notes/{id}` | Auth User | id | 200 note DTO if owned | 401, 404 if not found/not owned |
| `PATCH /notes/{id}` | Auth User | title/content partial | 200 updated note DTO | 400, 401, 404 |
| `DELETE /notes/{id}` | Auth User | id | 204/200 soft archived | 401, 404 |

## 5. Tasks API Expectations

| Endpoint | Actor | Input | Success | Error |
|---|---|---|---|---|
| `POST /tasks` | Auth User | title, description, priority, dueDate | 201 task DTO with status `todo` | 400, 401 |
| `GET /tasks` | Auth User | page, size, status, sort, q | 200 owned tasks list | 400, 401 |
| `GET /tasks/{id}` | Auth User | id | 200 task DTO if owned | 401, 404 |
| `PATCH /tasks/{id}` | Auth User | partial task fields | 200 updated DTO | 400, 401, 404 |
| `PATCH /tasks/{id}/status` | Auth User | status | 200 updated DTO | 400 invalid status, 401, 404 |
| `DELETE /tasks/{id}` | Auth User | id | 204/200 soft archived | 401, 404 |

## 6. Paging / Filter / Sort Expectations

### Paging

- `page` starts at 1.
- `size` default suggested: 20.
- Max size suggested: 100.

### Filter

- Notes: `q`, `archived` if exposed.
- Tasks: `status`, `q`, `dueDateFrom`, `dueDateTo` if implemented, `archived` if exposed.

### Sort

- Default: `updatedAt desc` or `createdAt desc`, but must be consistent and documented by BE.
- Tasks can additionally support `dueDate asc` if available.

## 7. Error Behavior Expectations

| Case | Expected HTTP | Business Expectation |
|---|---|---|
| Missing/invalid token | 401 | Client redirects to login or refresh flow. |
| Valid token but other user's resource | 404 preferred | Do not expose resource existence. |
| Validation failure | 400 | Return field-level details. |
| Duplicate email | 409 | Registration must not create duplicate account. |
| Invalid enum | 400 | Return clear invalid field. |
| Resource not found | 404 | No internal details. |

## 8. Client State Expectations

| State | FE/Mobile Expected Behavior |
|---|---|
| Loading | Show loading indicator/skeleton. |
| Empty list | Show friendly empty state and create CTA. |
| Error | Show retry or clear message. |
| Unauthorized | Redirect to login / ask user to login. |
| Validation error | Show field-level messages where possible. |

## 9. DTO Fields

### Note DTO

- `id`
- `title`
- `content`
- `archived`
- `createdAt`
- `updatedAt`

### Task DTO

- `id`
- `title`
- `description`
- `status`
- `priority`
- `dueDate`
- `archived`
- `createdAt`
- `updatedAt`

## 10. Acceptance Criteria

### AC-API-001: Endpoint expectation complete

Given BE designs API contracts  
When reviewing this document  
Then every core Auth, Notes, Tasks endpoint has actor, input, success, and error behavior.

### AC-API-002: Ownership behavior clear

Given a request references another user's resource  
When BE implements endpoint behavior  
Then response must not expose that resource data.

### AC-API-003: FE/Mobile integration states clear

Given FE/Mobile integrates APIs  
When API returns empty/error/loading/unauthorized conditions  
Then client expected behavior is known.
