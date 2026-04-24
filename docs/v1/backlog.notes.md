# NOTES BACKLOG

> Mục tiêu: backlog thực thi cho Notes module, đủ rõ để chuyển thành sprint items cho backend, frontend, mobile, QC.

---

## 1. Backlog Principles

- Notes là business module độc lập
- Chỉ làm trong phạm vi MVP và scope đã chốt
- Ưu tiên end-to-end usable flow hơn feature phụ
- Không phá module boundaries
- Contracts phải rõ trước khi implementation chính
- Web và mobile cùng bám public API thống nhất

---

## 2. Module Goal

Notes module phải cho phép người dùng:
- tạo note
- xem danh sách notes
- xem chi tiết note
- cập nhật note
- xóa hoặc archive note theo policy
- tìm kiếm/lọc cơ bản
- sử dụng được trên web và mobile trong MVP

---

## 3. Epic List

1. Notes Domain & Rules
2. Notes API Contracts
3. Notes Persistence
4. Notes Backend Application Flows
5. Notes Web UI
6. Notes Mobile UI
7. Notes Quality & Testing
8. Notes Documentation

---

## 4. Prioritized Backlog

## EPIC 1 — Notes Domain & Rules

### NB-001 Define Note entity
**Goal**
- Xác định Note entity tối thiểu cho MVP

**Scope**
- id
- title
- content
- tags (nếu giữ trong MVP)
- archived flag
- createdAt
- updatedAt
- ownerId

**Acceptance criteria**
- Note entity rõ ràng
- Không chứa concerns của Tasks
- Audit/ownership fields rõ

### NB-002 Define Note business rules
**Goal**
- Chốt rules tối thiểu cho create/update/archive/delete

**Acceptance criteria**
- Có doc hoặc code rule rõ ràng
- Các case invalid được xác định
- Không mơ hồ giữa delete và archive policy

### NB-003 Define Note search/list filter rules
**Goal**
- Chốt tiêu chí list/search cơ bản cho MVP

**Acceptance criteria**
- Search keyword/title rule rõ
- Pagination rule rõ
- Archived visibility rule rõ

---

## EPIC 2 — Notes API Contracts

### NB-004 Define Note DTO contracts
**Goal**
- Tạo request/response DTO cho Notes

**Acceptance criteria**
- Có create/update/detail/list DTOs
- JSON shape theo `api-conventions.md`
- Không leak internal fields

### NB-005 Define Notes endpoints
**Goal**
- Chốt endpoints cho create/get/list/update/delete

**Acceptance criteria**
- Endpoint list rõ
- HTTP semantics đúng
- Ownership expectations rõ

### NB-006 Define validation/error cases for Notes
**Goal**
- Chuẩn hóa validation và lỗi public cho Notes

**Acceptance criteria**
- Có danh sách validation cases tối thiểu
- Error mapping thống nhất với toàn hệ thống

---

## EPIC 3 — Notes Persistence

### NB-007 Create Note persistence mapping
**Goal**
- Map entity Notes vào DB theo baseline đã chốt

**Acceptance criteria**
- Mapping rõ
- Không có field dư chưa cần cho MVP
- Tương thích migration strategy chung

### NB-008 Add Note migration
**Goal**
- Tạo migration/schema cho Notes

**Acceptance criteria**
- Tạo bảng/structure đúng
- Có ownership/audit fields
- Local DB update được

### NB-009 Add indexes for Notes list/search
**Goal**
- Chuẩn bị index strategy tối thiểu cho list/search MVP

**Acceptance criteria**
- Có index hợp lý cho owner, updated time, searchable fields quan trọng
- Không over-optimize quá sớm

---

## EPIC 4 — Notes Backend Application Flows

### NB-010 Implement create note flow
**Goal**
- Người dùng tạo note thành công

**Acceptance criteria**
- Ownership lấy từ auth context
- Validation đúng
- Trả response đúng contract

### NB-011 Implement get note detail flow
**Goal**
- Người dùng xem chi tiết note của mình

**Acceptance criteria**
- Không đọc được note của user khác
- 404/403 xử lý đúng theo policy

### NB-012 Implement list notes flow
**Goal**
- Người dùng xem danh sách notes theo paging/filter/search cơ bản

**Acceptance criteria**
- Có pagination
- Có keyword search cơ bản
- Có archived handling rõ

### NB-013 Implement update note flow
**Goal**
- Người dùng cập nhật note của mình

**Acceptance criteria**
- Update fields đúng
- updatedAt đổi chính xác
- Validation/business rules áp đúng

### NB-014 Implement delete/archive note flow
**Goal**
- Người dùng xóa hoặc archive note của mình

**Acceptance criteria**
- Hành vi nhất quán theo product policy
- Contract/documentation rõ

---

## EPIC 5 — Notes Web UI

### NB-015 Create Notes list page
**Goal**
- Web có trang danh sách notes

**Acceptance criteria**
- Hiển thị list
- Có loading/empty/error states
- Có pagination cơ bản

### NB-016 Create Note detail/editor page
**Goal**
- Web có trang tạo/sửa/xem note

**Acceptance criteria**
- Tạo note được
- Sửa note được
- Save/update flow rõ ràng

### NB-017 Add Notes search/filter on web
**Goal**
- Có search/filter cơ bản cho Notes trên web

**Acceptance criteria**
- Search hoạt động với API contract
- UX đủ dùng cho MVP

### NB-018 Add delete/archive action on web
**Goal**
- User xử lý note trực tiếp trên web

**Acceptance criteria**
- Action đúng policy
- UI phản hồi rõ
- Không gây nhầm lẫn với save/update

---

## EPIC 6 — Notes Mobile UI

### NB-019 Create Notes list screen
**Goal**
- Mobile có danh sách notes cơ bản

**Acceptance criteria**
- Hiển thị list
- Có loading/empty/error state cơ bản

### NB-020 Create Note editor/detail screen
**Goal**
- Mobile có flow tạo/sửa note

**Acceptance criteria**
- Tạo note được
- Sửa note được
- Trải nghiệm đủ tốt cho use case thường xuyên

### NB-021 Add delete/archive action on mobile
**Goal**
- Mobile có action xử lý note theo policy MVP

**Acceptance criteria**
- Hành vi nhất quán với web/backend
- UX đơn giản, dễ hiểu

---

## EPIC 7 — Notes Quality & Testing

### NB-022 Add unit tests for Note rules
**Goal**
- Kiểm tra rules create/update/archive/delete của Note

**Acceptance criteria**
- Domain/application rules chính có test

### NB-023 Add integration tests for Notes API
**Goal**
- Test end-to-end cho create/get/list/update/delete hoặc archive

**Acceptance criteria**
- Có integration coverage cho happy path chính
- Có ít nhất một authorization/ownership case

### NB-024 Add contract checks for Notes responses
**Goal**
- Giữ API Notes ổn định cho web/mobile

**Acceptance criteria**
- Response shape quan trọng có test/check

---

## EPIC 8 — Notes Documentation

### NB-025 Write Notes API doc
**Goal**
- Có doc endpoint/request/response/error cho Notes

**Acceptance criteria**
- Team dev/qc/frontend/mobile đọc là dùng được

### NB-026 Write Notes module overview
**Goal**
- Tóm tắt role, boundaries, data, flows của Notes module

**Acceptance criteria**
- Dev mới onboard nhanh
- Không nhầm responsibilities với Tasks

---

## 5. Suggested Delivery Order

### Batch 1 — Contracts & Domain
- NB-001
- NB-002
- NB-004
- NB-005
- NB-006

### Batch 2 — Persistence & Backend
- NB-007
- NB-008
- NB-009
- NB-010
- NB-011
- NB-012
- NB-013
- NB-014

### Batch 3 — Web & Mobile
- NB-015
- NB-016
- NB-017
- NB-018
- NB-019
- NB-020
- NB-021

### Batch 4 — Quality & Docs
- NB-022
- NB-023
- NB-024
- NB-025
- NB-026

---

## 6. Definition of Done for Notes Module

Notes module được xem là MVP-ready khi:
- Note CRUD hoạt động ổn định
- List/search cơ bản hoạt động
- Ownership đúng
- Web flow dùng được
- Mobile flow dùng được
- API docs đủ rõ
- Unit/integration/contract tests tối thiểu đã có
- Không vi phạm module boundary
