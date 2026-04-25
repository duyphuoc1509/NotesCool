# TASKS BACKLOG

> Mục tiêu: backlog thực thi cho Tasks module, đủ rõ để chuyển thành sprint items cho backend, frontend, mobile, QC.

---

## 1. Backlog Principles

- Tasks là business module độc lập
- Chỉ làm trong phạm vi MVP
- Ưu tiên task lifecycle rõ ràng hơn feature mở rộng
- Không phá module boundaries
- Contract-first trước implementation chính
- Web và mobile dùng cùng public API

---

## 2. Module Goal

Tasks module phải cho phép người dùng:
- tạo task
- xem danh sách tasks
- xem chi tiết task
- cập nhật task
- đổi trạng thái task
- xóa hoặc archive task theo policy
- lọc/sắp xếp cơ bản
- sử dụng được trên web và mobile trong MVP

---

## 3. Epic List

1. Tasks Domain & Rules
2. Tasks API Contracts
3. Tasks Persistence
4. Tasks Backend Application Flows
5. Tasks Web UI
6. Tasks Mobile UI
7. Tasks Quality & Testing
8. Tasks Documentation

---

## 4. Prioritized Backlog

## EPIC 1 — Tasks Domain & Rules

### TB-001 Define Task entity
**Goal**
- Xác định Task entity tối thiểu cho MVP

**Scope**
- id
- title
- description
- status
- priority
- dueDate
- linkedNoteId (optional nếu giữ trong MVP)
- archived flag
- createdAt
- updatedAt
- ownerId

**Acceptance criteria**
- Task entity rõ ràng
- Không chứa responsibilities của Notes
- Status/priority/dueDate ownership rõ

### TB-002 Define Task lifecycle rules
**Goal**
- Chốt rules cho create/update/status change/archive/delete

**Acceptance criteria**
- Status flow rõ
- Invalid transitions được xác định nếu áp dụng
- Update rules rõ

### TB-003 Define Tasks list/filter/sort rules
**Goal**
- Chốt rule cho listing/filtering/sorting của Tasks

**Acceptance criteria**
- Có status/priority/due date filter tối thiểu
- Pagination rule rõ
- Archived visibility rule rõ

---

## EPIC 2 — Tasks API Contracts

### TB-004 Define Task DTO contracts
**Goal**
- Tạo request/response DTO cho Tasks

**Acceptance criteria**
- Có create/update/status/detail/list DTOs
- Theo `api-conventions.md`
- Không expose internal fields

### TB-005 Define Tasks endpoints
**Goal**
- Chốt endpoint create/get/list/update/status/delete

**Acceptance criteria**
- Endpoint list rõ
- PATCH status contract rõ
- Ownership expectations rõ

### TB-006 Define validation/error cases for Tasks
**Goal**
- Chuẩn hóa validation và lỗi public cho Tasks

**Acceptance criteria**
- Validation về title/status/priority/dueDate rõ
- Error mapping thống nhất với toàn hệ thống

---

## EPIC 3 — Tasks Persistence

### TB-007 Create Task persistence mapping
**Goal**
- Map Task entity vào DB theo baseline đã chốt

**Acceptance criteria**
- Mapping đúng
- Không có relation dư chưa cần cho MVP
- Dễ mở rộng sau này

### TB-008 Add Task migration
**Goal**
- Tạo migration/schema cho Tasks

**Acceptance criteria**
- Có ownership/audit fields
- Local DB update được

### TB-009 Add indexes for Tasks list/filter
**Goal**
- Chuẩn bị index strategy cho owner/status/dueDate/updated time

**Acceptance criteria**
- Query chính có nền hiệu quả
- Không over-engineer sớm

---

## EPIC 4 — Tasks Backend Application Flows

### TB-010 Implement create task flow
**Goal**
- Người dùng tạo task thành công

**Acceptance criteria**
- Ownership lấy từ auth context
- Validation đúng
- Trả response đúng contract

### TB-011 Implement get task detail flow
**Goal**
- Người dùng xem chi tiết task của mình

**Acceptance criteria**
- Không truy cập được task của user khác
- Xử lý not-found/forbidden đúng policy

### TB-012 Implement list tasks flow
**Goal**
- Người dùng xem danh sách tasks theo paging/filter/sort cơ bản

**Acceptance criteria**
- Có pagination
- Có filter tối thiểu
- Có sort cơ bản
- Archived handling rõ

### TB-013 Implement update task flow
**Goal**
- Người dùng cập nhật nội dung task

**Acceptance criteria**
- Update fields đúng
- updatedAt đổi đúng
- Không làm sai status contract

### TB-014 Implement update task status flow
**Goal**
- Người dùng đổi trạng thái task

**Acceptance criteria**
- Status contract rõ
- Rule status được áp đúng
- Response nhất quán

### TB-015 Implement delete/archive task flow
**Goal**
- Người dùng xóa hoặc archive task của mình

**Acceptance criteria**
- Hành vi đúng policy sản phẩm
- Contract/documentation rõ

---

## EPIC 5 — Tasks Web UI

### TB-016 Create Tasks list page
**Goal**
- Web có trang danh sách tasks

**Acceptance criteria**
- Hiển thị list
- Có loading/empty/error states
- Có filter/sort cơ bản

### TB-017 Create Task detail/editor page
**Goal**
- Web có flow tạo/sửa/xem task

**Acceptance criteria**
- Tạo task được
- Sửa task được
- Status update dùng được

### TB-018 Add Tasks filter/sort/status actions on web
**Goal**
- Người dùng quản lý task hiệu quả trên web

**Acceptance criteria**
- Filter/sort/status flow rõ
- UX đủ dùng cho MVP

### TB-019 Add delete/archive action on web
**Goal**
- Web xử lý xóa/archive task đúng policy

**Acceptance criteria**
- Hành vi nhất quán với backend/mobile

---

## EPIC 6 — Tasks Mobile UI

### TB-020 Create Tasks list screen
**Goal**
- Mobile có danh sách tasks cơ bản

**Acceptance criteria**
- Hiển thị list
- Có loading/empty/error state cơ bản

### TB-021 Create Task detail/editor screen
**Goal**
- Mobile có flow tạo/sửa task

**Acceptance criteria**
- Tạo task được
- Sửa task được
- Chỉnh status được

### TB-022 Add filter/status/delete or archive on mobile
**Goal**
- Mobile hỗ trợ use case cốt lõi cho task management

**Acceptance criteria**
- Đủ dùng cho user hằng ngày
- Hành vi thống nhất với web/backend

---

## EPIC 7 — Tasks Quality & Testing

### TB-023 Add unit tests for Task rules
**Goal**
- Kiểm tra lifecycle rules của Task

**Acceptance criteria**
- Domain/application rules chính có test

### TB-024 Add integration tests for Tasks API
**Goal**
- Test end-to-end cho create/get/list/update/status/delete hoặc archive

**Acceptance criteria**
- Có integration coverage cho happy path chính
- Có ít nhất một ownership/authorization case

### TB-025 Add contract checks for Tasks responses
**Goal**
- Giữ API Tasks ổn định cho web/mobile

**Acceptance criteria**
- Response shape quan trọng có test/check

---

## EPIC 8 — Tasks Documentation

### TB-026 Write Tasks API doc
**Goal**
- Có doc endpoint/request/response/error cho Tasks

**Acceptance criteria**
- Team dev/qc/frontend/mobile đọc là dùng được

### TB-027 Write Tasks module overview
**Goal**
- Tóm tắt role, boundaries, data, flows của Tasks module

**Acceptance criteria**
- Dev mới onboard nhanh
- Không nhầm responsibilities với Notes

---

## 5. Suggested Delivery Order

### Batch 1 — Contracts & Domain
- TB-001
- TB-002
- TB-003
- TB-004
- TB-005
- TB-006

### Batch 2 — Persistence & Backend
- TB-007
- TB-008
- TB-009
- TB-010
- TB-011
- TB-012
- TB-013
- TB-014
- TB-015

### Batch 3 — Web & Mobile
- TB-016
- TB-017
- TB-018
- TB-019
- TB-020
- TB-021
- TB-022

### Batch 4 — Quality & Docs
- TB-023
- TB-024
- TB-025
- TB-026
- TB-027

---

## 6. Definition of Done for Tasks Module

Tasks module được xem là MVP-ready khi:
- Task CRUD hoạt động ổn định
- Status update hoạt động rõ ràng
- List/filter/sort cơ bản hoạt động
- Ownership đúng
- Web flow dùng được
- Mobile flow dùng được
- API docs đủ rõ
- Unit/integration/contract tests tối thiểu đã có
- Không vi phạm module boundary
