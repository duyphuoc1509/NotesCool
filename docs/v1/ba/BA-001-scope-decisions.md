# BA-001 — MVP V1 Scope Decisions

## 1. Overview

Tài liệu này chốt phạm vi nghiệp vụ cho NotesCool MVP V1 để Dev, FE, Mobile và QC có cùng cách hiểu trước khi triển khai.

## 2. Confirmed MVP Goal

MVP V1 tập trung vào một sản phẩm quản lý ghi chú và công việc cá nhân, gồm:

- Người dùng đăng ký/đăng nhập và chỉ quản lý dữ liệu của chính mình.
- Notes CRUD với search/pagination cơ bản.
- Tasks CRUD với status/filter/sort/pagination cơ bản.
- Web hỗ trợ đầy đủ core flow.
- Mobile hỗ trợ core flow tương đương ở mức usable MVP.

## 3. Must-have Decisions

| Area | Decision | Notes |
|---|---|---|
| Auth | In scope | Registration, login, logout, JWT access token và refresh token ở mức MVP. |
| Ownership | In scope | Notes/Tasks luôn thuộc về authenticated user hiện tại; không cho client truyền ownerId khi tạo/sửa. |
| Notes CRUD | In scope | Create, list, detail, update, archive/delete. |
| Tasks CRUD | In scope | Create, list, detail, update, status change, archive/delete. |
| Archive vs hard delete | Soft archive/delete is default for MVP | DELETE API thực hiện soft delete/archive; dữ liệu không hiện ở list mặc định. Hard delete không thuộc MVP UI/API public. |
| Search | In scope at basic level | Notes search theo keyword trong title/content; Tasks search theo keyword trong title/description nếu có. |
| Pagination | In scope | List endpoints phải hỗ trợ page/size và trả metadata. |
| Task status | In scope | `todo`, `in_progress`, `done`, `archived`. |
| Web parity | In scope | Web cần hỗ trợ đầy đủ core MVP flows. |
| Mobile parity | In scope for core flows | Mobile hỗ trợ core create/list/detail/update/status/archive; advanced polish có thể deferred. |
| Empty/loading/error states | In scope at minimum | Client cần hiển thị được trạng thái cơ bản, không yêu cầu polish nâng cao. |

## 4. Should-have / Release 1.1 Decisions

| Item | Decision | Target Phase |
|---|---|---|
| Tags | Deferred from MVP V1 public behavior | Release 1.1 |
| Linked note-task | Deferred from MVP V1 public behavior | Release 1.1 |
| Advanced search | Deferred | Release 1.1+ |
| Dashboard/home summary | Optional only if no impact to core scope | Release 1.1 |
| Mobile polish beyond core parity | Deferred | Release 1.1 |

## 5. Not-now Decisions

The following are explicitly out of MVP V1:

- Collaboration/realtime/shared workspace.
- Push notifications and reminder delivery.
- Recurrence, calendar, kanban, analytics.
- Enterprise RBAC.
- AI features.
- Plugin marketplace.

## 6. Confirmed Assumptions

- MVP V1 is personal workspace only; no team workspace sharing.
- A user can only access Notes/Tasks owned by their own authenticated account.
- Archived/deleted records are hidden from default list responses.
- Detail access for archived records is allowed only to owner if API explicitly requests/uses the ID; clients should visually indicate archived state if shown.
- All API responses follow `docs/v1/api-conventions.md` envelope/error conventions.
- JSON public fields use camelCase.
- Server timestamps are source of truth for `createdAt` and `updatedAt`.

## 7. Open Decisions Resolved

| Question | Decision |
|---|---|
| Auth MVP approach? | Email/password registration/login + JWT access token + refresh token. |
| Archive or hard delete? | Soft archive/delete by default. |
| Tags in MVP? | No; defer to Release 1.1. |
| Linked note-task in MVP? | No; defer to Release 1.1. |
| Mobile parity expectation? | Mobile supports core MVP flows, not all polish/desktop edge UX. |

## 8. Impact

### Dev

- Implement ownership as a backend rule, not a client-side filter.
- Do not implement tags or linked note-task as required MVP contracts.
- DELETE behavior should not physically remove records through public API.

### FE/Mobile

- Use shared API contracts.
- Implement minimum loading/empty/error states.
- Hide archived items in default active list.

### QC

- Test ownership isolation as critical security acceptance.
- Test archive/delete as soft-hide behavior.
- Treat tags and linked note-task as out-of-scope unless separately approved.

## 9. Acceptance Criteria

### AC-001: Must-have scope is clear

Given Dev/QC review MVP V1 scope  
When they check each Must-have item  
Then every item has an explicit in-scope decision and business expectation.

### AC-002: Deferred items are visible

Given an item is not part of MVP V1  
When Dev/QC review this decision log  
Then the item has a target phase or is marked Not-now.

### AC-003: Delete policy is unambiguous

Given user deletes a note or task  
When the public DELETE API is called  
Then the system performs soft archive/delete and the item is hidden from default active lists.
