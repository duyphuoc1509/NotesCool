# BA-004 — Tasks Module Requirement

## 1. Overview

Tasks module cho phép user quản lý việc cần làm, trạng thái công việc và theo dõi deadline cơ bản.

## 2. Business Goal

Cung cấp công cụ gọn nhẹ để user tạo task và theo dõi luồng trạng thái mà không cần project management lớn.

## 3. Actors

- User
- System

## 4. Current Flow

Chưa có.

## 5. Proposed Flow

1. User xem danh sách tasks.
2. User tạo task với title, optional description, priority, dueDate.
3. User thay đổi status của task qua pipeline (todo -> in_progress -> done).
4. User sửa thông tin task.
5. User archive/delete task (soft delete).
6. Danh sách hỗ trợ filter theo status.

## 6. Functional Requirements

### FR-T-001: Create Task

User tạo task. Mặc định status là `todo` nếu không gửi.

### FR-T-002: List Tasks

User xem danh sách task của mình, phân trang.

### FR-T-003: Filter Tasks

User có thể filter danh sách theo `status`.

### FR-T-004: View Task Detail

User xem thông tin chi tiết task của mình.

### FR-T-005: Update Task Details

User có thể cập nhật title, description, priority, dueDate.

### FR-T-006: Change Status

User có thể cập nhật riêng biệt trường `status` hoặc cập nhật kèm trong FR-T-005 tùy BE contract.

### FR-T-007: Archive/Delete Task

User archive task. Mặc định ẩn trong active list.

## 7. Business Rules

### BR-T-001: Task Status Pipeline

Status chuẩn: `todo`, `in_progress`, `done`, `archived`. Transition nên linh hoạt ở MVP nhưng khuyến nghị logic chuẩn.

### BR-T-002: Default Task Status

Khi tạo mới, status mặc định là `todo`.

### BR-T-003: Ownership

Tasks thuộc về authenticated user hiện tại.

### BR-T-004: Default Visibility

Archived tasks bị ẩn khỏi default list, trừ khi client gửi bộ lọc explicit.

## 8. Validation Rules

| Field | Rule | Error Expectation |
|---|---|---|
| title | Required, string, trim, max 200 chars | Validation error |
| description | Optional string, max 10000 chars | Validation error |
| priority | Optional string enum (e.g. low, medium, high) | Validation error |
| dueDate | Optional valid datetime | Validation error |
| status | Valid enum | Validation error |

## 9. State / Status Rules

| Current Status | Allowed Target Statuses | Note |
|---|---|---|
| todo | in_progress, done, archived | - |
| in_progress | todo, done, archived | - |
| done | todo, archived | - |

## 10. Permission Rules

(Giống Notes Module: Chỉ owner mới có quyền read/update/delete/archive).

## 11. Acceptance Criteria

### AC-T-001: Create Task

Given authenticated User  
When User submits valid task data  
Then System creates task with `todo` status and returns DTO.

### AC-T-002: List & Filter Tasks

Given User has tasks in different statuses  
When User requests task list with status filter (e.g., `status=todo`)  
Then System returns only owned tasks matching the filter.

### AC-T-003: Update Status

Given User owns an active task  
When User changes status to `done`  
Then System updates status, logs updated time.

### AC-T-004: Archive Task

Given User owns a task  
When User archives the task  
Then Task is hidden from default list.

### AC-T-005: Ownership Check

Given User requests detail/update of another user's task  
When the request is processed  
Then System denies access.

## 12. Edge Cases

- Status change validation: request invalid status enum.
- dueDate parsing and timezone behavior (must be defined in API doc).

## 13. Notes for Dev

- Define enum values properly and validate them explicitly.
- Allow separate PATCH endpoint for status if client requests it, otherwise support status update in standard PATCH task.

## 14. Notes for QC

- Test task creation, empty string handling for title.
- Test filtering by status.
- Test status transition.
- Test ownership boundary.
