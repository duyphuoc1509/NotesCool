# SPRINT PLAN — NOTES MODULE

> Mục tiêu: kế hoạch sprint cho Notes module, đủ rõ để giao việc cho backend, frontend, mobile, QC theo từng đợt thực thi.

---

## 1. Sprint Goal

Notes sprint plan nhằm:
- triển khai Notes module theo phạm vi MVP đã chốt
- bám đúng contracts, boundaries và acceptance criteria
- tạo ra flow end-to-end usable trên backend, web, mobile

---

## 2. Prerequisites

Chỉ bắt đầu Notes sprint khi:
- phase 1 foundation đã đạt exit criteria
- `api-conventions.md` đã chốt
- `solution-structure.md` đã chốt
- `module-map.md` đã chốt
- `acceptance-criteria.notes.md` đã sẵn sàng
- auth/ownership baseline và DB baseline đã có

---

## 3. Sprint Structure Recommendation

Khuyến nghị chia Notes module thành 3 sprint ngắn:

- Sprint N1: Contracts & Backend core
- Sprint N2: Web/Mobile flows
- Sprint N3: Quality, polish, docs

Nếu team nhỏ, có thể triển khai theo batch thay vì sprint formal.

---

## 4. Sprint N1 — Contracts & Backend Core

### Goal
Xây xong domain, contracts, persistence và backend flows cốt lõi cho Notes.

### Scope
- Note entity và business rules
- Request/response DTOs
- Notes endpoints
- Validation/error mapping
- Persistence mapping/migration
- Create/get/list/update/delete or archive flows
- Backend tests baseline

### Backlog items
- NB-001 Define Note entity
- NB-002 Define Note business rules
- NB-003 Define Note search/list filter rules
- NB-004 Define Note DTO contracts
- NB-005 Define Notes endpoints
- NB-006 Define validation/error cases for Notes
- NB-007 Create Note persistence mapping
- NB-008 Add Note migration
- NB-009 Add indexes for Notes list/search
- NB-010 Implement create note flow
- NB-011 Implement get note detail flow
- NB-012 Implement list notes flow
- NB-013 Implement update note flow
- NB-014 Implement delete/archive note flow
- NB-022 Add unit tests for Note rules
- NB-023 Add integration tests for Notes API
- NB-024 Add contract checks for Notes responses

### Deliverables
- Notes backend chạy được end-to-end
- CRUD hoặc archive policy hoạt động
- List/search cơ bản hoạt động
- Ownership đúng
- Tests baseline có

### Exit criteria
- API Notes đủ dùng cho web/mobile bắt đầu tích hợp
- Acceptance criteria backend chính đã pass
- Không có coupling sai sang Tasks

### Suggested owners
- Backend Lead
- Backend Dev
- QC
- Tech Lead review

---

## 5. Sprint N2 — Web & Mobile Flows

### Goal
Tạo flow Notes usable trên web và mobile theo public contracts đã chốt.

### Scope
- Notes list page/screen
- Note detail/editor
- Search/filter cơ bản
- Delete/archive action
- Loading/empty/error states
- UX đủ tốt cho MVP

### Backlog items
- NB-015 Create Notes list page
- NB-016 Create Note detail/editor page
- NB-017 Add Notes search/filter on web
- NB-018 Add delete/archive action on web
- NB-019 Create Notes list screen
- NB-020 Create Note editor/detail screen
- NB-021 Add delete/archive action on mobile

### Deliverables
- Web Notes flow usable
- Mobile Notes flow usable
- Notes UI bám đúng contracts
- Shared UI states được áp dụng

### Exit criteria
- User có thể tạo/sửa/xem/xóa hoặc archive note trên web
- User có thể tạo/sửa/xem/xóa hoặc archive note trên mobile ở mức MVP
- UX đủ rõ cho demo nội bộ

### Suggested owners
- Frontend Dev
- Mobile Dev
- Designer
- QC

---

## 6. Sprint N3 — Quality, Polish, Docs

### Goal
Ổn định Notes module trước khi xem là MVP-ready.

### Scope
- Chạy full acceptance review
- Fix issues mức MVP blocker/high
- Hoàn thiện docs Notes
- Demo readiness
- Regression checks

### Backlog items
- NB-025 Write Notes API doc
- NB-026 Write Notes module overview
- Review lại NB-022, NB-023, NB-024 nếu còn gap
- Bug fixes từ QA/UAT của Notes
- UX polish tối thiểu cho empty/loading/error/save feedback

### Deliverables
- Notes docs đủ dùng
- QA pass cho luồng chính
- Demo Notes module trơn tru

### Exit criteria
- `acceptance-criteria.notes.md` pass cho các flow chính
- Docs đã cập nhật
- Không còn blocker severity cho Notes MVP

### Suggested owners
- QC
- Backend Dev
- Frontend Dev
- Mobile Dev
- PO review

---

## 7. Suggested Timeline

### Week Notes-1
- Sprint N1 bắt đầu
- khóa contracts/rules
- xong backend create/get/list

### Week Notes-2
- hoàn tất backend update/delete/archive
- bắt đầu web/mobile integration

### Week Notes-3
- hoàn tất web/mobile flows
- test/regression/docs/demo readiness

Có thể co giãn tùy team size.

---

## 8. Risks

### Risk 1 — Search scope bị lan quá sớm
**Mitigation**
- Chỉ hỗ trợ search cơ bản theo MVP
- Không tối ưu advanced search ở giai đoạn này

### Risk 2 — Note editor bị mở rộng quá mức
**Mitigation**
- Không thêm formatting/rich editor phức tạp trong MVP nếu chưa chốt

### Risk 3 — Web/mobile behavior khác nhau
**Mitigation**
- Review theo acceptance criteria chung
- Bám cùng contracts

### Risk 4 — Delete/archive policy mơ hồ
**Mitigation**
- Chốt policy sớm ở Sprint N1
- Document rõ behavior

---

## 9. Sprint Tracking Format

Mỗi item Notes nên có:
- ID (NB-xxx)
- Owner
- Status
- Dependency
- Acceptance criteria ref
- Demo note nếu là item UI

---

## 10. Notes MVP Readiness Checklist

- [ ] Notes entity/rules đã chốt
- [ ] Notes contracts đã chốt
- [ ] Notes API chạy được
- [ ] Ownership đúng
- [ ] List/search cơ bản đúng
- [ ] Web flow usable
- [ ] Mobile flow usable
- [ ] Unit/integration/contract tests có
- [ ] Docs Notes có
- [ ] Acceptance criteria chính đã pass
