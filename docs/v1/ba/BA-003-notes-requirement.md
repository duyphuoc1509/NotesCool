# BA-003 — Notes Module Requirement

## 1. Overview

Notes module cho phép authenticated user tạo, xem, tìm kiếm, cập nhật và archive ghi chú cá nhân.

## 2. Business Goal

Giúp user lưu và quản lý ghi chú nhanh, riêng tư, có thể tìm lại nội dung cơ bản.

## 3. Actors

- User
- System

## 4. Current Flow

Chưa có.

## 5. Proposed Flow

1. User đăng nhập.
2. User vào Notes list.
3. User có thể tạo Note mới.
4. User có thể mở detail và chỉnh sửa Note thuộc sở hữu của mình.
5. User có thể search trong Notes của mình.
6. User có thể archive/delete Note. Note archive không xuất hiện trong list mặc định.

## 6. Functional Requirements

### FR-N-001: Create Note

User tạo note bằng `title` và `content`.

### FR-N-002: List Notes

User xem danh sách notes của chính mình, phân trang và mặc định chỉ hiển thị active notes.

### FR-N-003: Search Notes

User search theo keyword trong `title` và `content`.

### FR-N-004: View Note Detail

User xem detail note thuộc sở hữu của mình.

### FR-N-005: Update Note

User cập nhật `title` và/hoặc `content` của note.

### FR-N-006: Archive/Delete Note

User xóa note theo policy soft archive/delete. List mặc định không trả archived notes.

## 7. Business Rules

### BR-N-001: Ownership

Notes thuộc về đúng một owner.

### BR-N-002: Default Visibility

Archived notes bị ẩn khỏi default list.

### BR-N-003: Search Scope

Search chỉ áp dụng trong tập notes user hiện tại sở hữu.

### BR-N-004: No Tags in MVP V1

Tags không phải behavior bắt buộc của MVP V1 public contract.

## 8. Validation Rules

| Field | Rule | Error Message Expectation |
|---|---|---|
| title | Required, trim whitespace, max 200 chars | Title is required / Title is too long |
| content | Optional or required by product? MVP decision: optional but max 20000 chars | Content is too long |
| page | Integer >= 1 | Page must be greater than 0 |
| size | Integer 1..100 | Page size is invalid |
| q | Optional string, trim, max 100 chars | Search keyword is too long |

## 9. State / Status Rules

| Current Status | Action | Next Status | Note |
|---|---|---|---|
| active | archive/delete | archived | Owner only |
| archived | list default | hidden | Unless explicit archived filter exists |
| archived | update | not allowed by default | Requires restore feature if added later |

## 10. Permission Rules

| Role | Action | Allowed |
|---|---|---|
| Authenticated User | Create own note | Yes |
| Authenticated User | Read/update/archive own note | Yes |
| Authenticated User | Read/update/archive other's note | No |
| Unauthenticated User | Any Notes API | No |

## 11. Acceptance Criteria

### AC-N-001: Create note successfully

Given authenticated User submits valid title/content  
When System creates the note  
Then note is saved under current user and response includes note DTO.

### AC-N-002: List own active notes

Given User has active and archived notes  
When User loads default Notes list  
Then System returns only active notes owned by that user.

### AC-N-003: Search own notes

Given User owns notes containing keyword  
When User searches by that keyword  
Then System returns matching owned active notes only.

### AC-N-004: View detail

Given User owns a note  
When User opens note detail  
Then System returns title/content/timestamps for that note.

### AC-N-005: Update note

Given User owns an active note  
When User updates valid fields  
Then System updates note and `updatedAt` changes.

### AC-N-006: Archive note

Given User owns an active note  
When User archives/deletes the note  
Then System marks it archived and it disappears from default list.

### AC-N-007: Ownership protected

Given another user's note exists  
When current user requests it by ID  
Then System must not expose its data.

## 12. Edge Cases

- Empty note list -> return empty items with valid pagination metadata.
- Keyword has no match -> empty items, not error.
- Invalid page/size -> validation error.
- Duplicate archive request -> idempotent success or not found, but must be consistent and documented by BE.

## 13. Notes for Dev

- `DELETE /notes/{id}` should implement archive/soft-delete behavior.
- Default list excludes archived records.
- Avoid exposing `ownerId` in public DTO unless explicitly needed.

## 14. Notes for QC

- Test create/list/detail/update/archive positive flows.
- Test title required, title length, content length.
- Test ownership by using two accounts.
- Test pagination with more records than page size.

## 15. Open Questions

- (None for MVP V1)
