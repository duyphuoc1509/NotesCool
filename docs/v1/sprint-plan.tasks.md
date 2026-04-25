# SPRINT PLAN — TASKS MODULE

> Mục tiêu: kế hoạch sprint cho Tasks module, đủ rõ để giao việc cho backend, frontend, mobile, QC theo từng đợt thực thi.

---

## 1. Sprint Goal

Tasks sprint plan nhằm:
- triển khai Tasks module theo phạm vi MVP đã chốt
- bám đúng contracts, lifecycle rules và acceptance criteria
- tạo flow end-to-end usable trên backend, web, mobile

---

## 2. Prerequisites

Chỉ bắt đầu Tasks sprint khi:
- phase 1 foundation đã đạt exit criteria
- `api-conventions.md` đã chốt
- `solution-structure.md` đã chốt
- `module-map.md` đã chốt
- `acceptance-criteria.tasks.md` đã sẵn sàng
- auth/ownership baseline và DB baseline đã có

---

## 3. Sprint Structure Recommendation

Khuyến nghị chia Tasks module thành 3 sprint ngắn:

- Sprint T1: Contracts & Backend core
- Sprint T2: Web/Mobile flows
- Sprint T3: Quality, polish, docs

Nếu team nhỏ, có thể triển khai theo batch thay vì sprint formal.

---

## 4. Sprint T1 — Contracts & Backend Core

### Goal
Xây xong domain, contracts, persistence và backend flows cốt lõi cho Tasks.

### Scope
- Task entity và lifecycle rules
- Request/response DTOs
- Tasks endpoints
- Validation/error mapping
- Persistence mapping/migration
- Create/get/list/update/status/delete or archive flows
- Backend tests baseline

### Backlog items
- TB-001 Define Task entity
- TB-002 Define Task lifecycle rules
- TB-003 Define Tasks list/filter/sort rules
- TB-004 Define Task DTO contracts
- TB-005 Define Tasks endpoints
- TB-006 Define validation/error cases for Tasks
- TB-007 Create Task persistence mapping
- TB-008 Add Task migration
- TB-009 Add indexes for Tasks list/filter
- TB-010 Implement create task flow
- TB-011 Implement get task detail flow
- TB-012 Implement list tasks flow
- TB-013 Implement update task flow
- TB-014 Implement update task status flow
- TB-015 Implement delete/archive task flow
- TB-023 Add unit tests for Task rules
- TB-024 Add integration tests for Tasks API
- TB-025 Add contract checks for Tasks responses

### Deliverables
- Tasks backend chạy được end-to-end
- CRUD/status/update/archive policy hoạt động
- List/filter/sort cơ bản hoạt động
- Ownership đúng
- Tests baseline có

### Exit criteria
- API Tasks đủ dùng cho web/mobile bắt đầu tích hợp
- Acceptance criteria backend chính đã pass
- Status contract ổn định
- Không có coupling sai sang Notes

### Suggested owners
- Backend Lead
- Backend Dev
- QC
- Tech Lead review

---

## 5. Sprint T2 — Web & Mobile Flows

### Goal
Tạo flow Tasks usable trên web và mobile theo public contracts đã chốt.

### Scope
- Tasks list page/screen
- Task detail/editor
- Filter/sort/status actions
- Delete/archive action
- Loading/empty/error states
- UX đủ tốt cho MVP

### Backlog items
- TB-016 Create Tasks list page
- TB-017 Create Task detail/editor page
- TB-018 Add Tasks filter/sort/status actions on web
- TB-019 Add delete/archive action on web
- TB-020 Create Tasks list screen
- TB-021 Create Task detail/editor screen
- TB-022 Add filter/status/delete or archive on mobile

### Deliverables
- Web Tasks flow usable
- Mobile Tasks flow usable
- Tasks UI bám đúng contracts
- Shared UI states được áp dụng

### Exit criteria
- User có thể tạo/sửa/xem/xóa hoặc archive task trên web
- User có thể đổi trạng thái task trên web/mobile
- User có thể dùng tasks flow trên mobile ở mức MVP
- UX đủ rõ cho demo nội bộ

### Suggested owners
- Frontend Dev
- Mobile Dev
- Designer
- QC

---

## 6. Sprint T3 — Quality, Polish, Docs

### Goal
Ổn định Tasks module trước khi xem là MVP-ready.

### Scope
- Chạy full acceptance review
- Fix issues mức MVP blocker/high
- Hoàn thiện docs Tasks
- Demo readiness
- Regression checks

### Backlog items
- TB-026 Write Tasks API doc
- TB-027 Write Tasks module overview
- Review lại TB-023, TB-024, TB-025 nếu còn gap
- Bug fixes từ QA/UAT của Tasks
- UX polish tối thiểu cho empty/loading/error/save/status feedback

### Deliverables
- Tasks docs đủ dùng
- QA pass cho luồng chính
- Demo Tasks module trơn tru

### Exit criteria
- `acceptance-criteria.tasks.md` pass cho các flow chính
- Docs đã cập nhật
- Không còn blocker severity cho Tasks MVP

### Suggested owners
- QC
- Backend Dev
- Frontend Dev
- Mobile Dev
- PO review

---

## 7. Suggested Timeline

### Week Tasks-1
- Sprint T1 bắt đầu
- khóa contracts/rules
- xong backend create/get/list

### Week Tasks-2
- hoàn tất backend update/status/delete/archive
- bắt đầu web/mobile integration

### Week Tasks-3
- hoàn tất web/mobile flows
- test/regression/docs/demo readiness

Có thể co giãn tùy team size.

---

## 8. Risks

### Risk 1 — Status flow phức tạp hóa quá sớm
**Mitigation**
- Dùng status set tối thiểu cho MVP
- Chưa mở rộng workflow engine

### Risk 2 — Filter/sort lan scope
**Mitigation**
- Chỉ hỗ trợ các filter/sort cốt lõi đã chốt
- Không thêm dashboard phức tạp

### Risk 3 — Web/mobile behavior khác nhau
**Mitigation**
- Review theo acceptance criteria chung
- Bám cùng contracts

### Risk 4 — linkedNoteId gây coupling sớm
**Mitigation**
- Nếu giữ trong MVP thì chỉ là optional external reference
- Không gọi internals của Notes

---

## 9. Sprint Tracking Format

Mỗi item Tasks nên có:
- ID (TB-xxx)
- Owner
- Status
- Dependency
- Acceptance criteria ref
- Demo note nếu là item UI

---

## 10. Tasks MVP Readiness Checklist

- [ ] Task entity/rules đã chốt
- [ ] Task contracts đã chốt
- [ ] Tasks API chạy được
- [ ] Ownership đúng
- [ ] List/filter/sort cơ bản đúng
- [ ] Status update đúng
- [ ] Web flow usable
- [ ] Mobile flow usable
- [ ] Unit/integration/contract tests có
- [ ] Docs Tasks có
- [ ] Acceptance criteria chính đã pass
