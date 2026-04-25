# SPRINT PLAN — PHASE 1

> Mục tiêu: chuyển từ backlog foundation sang kế hoạch thực thi theo sprint, đủ rõ để giao việc cho team và theo dõi tiến độ.

---

## 1. Phase 1 Objective

Phase 1 tập trung vào:
- dựng nền tảng kỹ thuật và cấu trúc delivery
- khóa conventions và architecture guardrails
- chuẩn bị sẵn sàng để bước sang implementation Notes và Tasks

Phase này **không** nhằm hoàn thành toàn bộ feature MVP.

---

## 2. Phase 1 Success Definition

Phase 1 được xem là thành công khi:
- repo structure rõ ràng
- backend skeleton chạy được
- frontend skeleton chạy được
- mobile skeleton chạy được
- API conventions và solution structure đã được áp dụng
- auth/ownership baseline đã chốt
- database baseline và migration strategy đã có
- test baseline và CI baseline đã sẵn sàng
- team có thể bắt đầu Notes/Tasks implementation với ambiguity thấp

---

## 3. Sprint Structure Recommendation

Khuyến nghị chia Phase 1 thành 3 sprint ngắn:

- Sprint 1: Foundation setup
- Sprint 2: Technical baseline
- Sprint 3: Quality gates & readiness

Nếu team nhỏ, có thể coi đây là 3 batch trong một phase liên tục.

---

## 4. Sprint 1 — Foundation Setup

### Goal
Dựng cấu trúc repo và skeleton cho backend, web, mobile.

### Scope
- Monorepo structure
- Docs folders
- Backend solution skeleton
- Frontend app shell
- Mobile app shell
- Shared initial structure
- Test folders baseline

### Backlog items
- FB-001 Create monorepo structure
- FB-002 Define naming conventions
- FB-003 Create backend solution skeleton
- FB-004 Define module registration pattern
- FB-016 Create React app skeleton
- FB-019 Create React Native app skeleton
- FB-021 Setup backend unit test projects

### Deliverables
- Repo structure thật trong codebase
- Backend host/shared/modules skeleton
- Frontend feature-based structure
- Mobile feature-based structure
- README root được khởi tạo

### Exit criteria
- Team clone repo và chạy được skeleton cơ bản
- Không còn ambiguity lớn về folder structure
- Có baseline để bắt đầu wiring technical concerns

### Suggested owners
- Tech Lead
- Backend Dev
- Frontend Dev
- Mobile Dev

---

## 5. Sprint 2 — Technical Baseline

### Goal
Thêm các technical baseline dùng chung cho toàn dự án.

### Scope
- Shared primitives/contracts
- API conventions trong docs và định hướng code
- DB/PostgreSQL baseline
- EF Core baseline
- Auth approach cho MVP
- Ownership baseline
- API client baseline cho web/mobile

### Backlog items
- FB-006 Create shared primitives
- FB-007 Define API response conventions
- FB-008 Define time/user context abstractions
- FB-009 Choose auth approach for MVP
- FB-010 Implement ownership baseline
- FB-011 Create API conventions doc
- FB-013 Setup PostgreSQL integration
- FB-014 Setup EF Core baseline
- FB-017 Setup API client conventions
- FB-020 Setup API/auth flow baseline for mobile

### Deliverables
- Shared contracts baseline
- Auth/ownership direction rõ
- DB baseline chạy được local
- API client structure có thể reuse
- Tài liệu conventions được review

### Exit criteria
- Có technical foundation đủ để Notes/Tasks bắt đầu build
- Team thống nhất cách gọi API và xử lý ownership
- DB migration flow hoạt động

### Suggested owners
- Tech Lead
- Backend Lead
- Frontend Lead
- Mobile Dev

---

## 6. Sprint 3 — Quality Gates & Readiness

### Goal
Dựng cơ chế kiểm soát chất lượng và chốt readiness trước khi vào feature implementation.

### Scope
- Validation/error handling base
- Audit/archive policy
- Shared UI states guideline
- Integration test baseline
- Architecture tests
- CI baseline
- Getting started docs
- Architecture overview docs
- MVP delivery checklist

### Backlog items
- FB-012 Implement validation/error handling base
- FB-015 Define audit and soft-delete policy
- FB-018 Define shared UI states
- FB-022 Setup integration test baseline
- FB-023 Add architecture tests
- FB-024 Add CI baseline
- FB-025 Create getting-started docs
- FB-026 Create architecture overview doc
- FB-027 Create MVP delivery checklist

### Deliverables
- Error handling chuẩn
- Test baseline chạy được
- CI kiểm tra build/test chính
- Runbook/onboarding docs đủ dùng
- Ready checklist trước phase feature

### Exit criteria
- Có chất lượng nền tối thiểu
- Có thể phát hiện coupling sai sớm
- Team sẵn sàng kick off Notes/Tasks feature work

### Suggested owners
- QC
- Tech Lead
- Backend Dev
- Frontend Dev
- Mobile Dev

---

## 7. Sprint Ceremonies

### Sprint planning
- Review sprint goal
- Confirm scope theo backlog IDs
- Chốt owner từng item
- Nêu dependency/blockers trước khi bắt đầu

### Mid-sprint sync
- Kiểm tra blockers
- Kiểm tra decision cần escalate
- Kiểm tra rủi ro lan scope hoặc lệch architecture

### Sprint review
- Demo deliverables thực tế
- So với exit criteria
- Ghi nhận gap còn lại

### Retrospective
- Điều gì chậm?
- Điều gì mơ hồ?
- Điều gì cần chuẩn hóa tiếp trước phase 2?

---

## 8. Delivery Risks During Phase 1

### Risk 1 — Làm skeleton nhưng thiếu consistency
**Mitigation**
- Bám `solution-structure.md`
- Review tech lead trước khi merge structure lớn

### Risk 2 — Chậm do tranh luận kỹ thuật quá lâu
**Mitigation**
- Timebox decision
- Ưu tiên hướng đơn giản, đủ cho MVP

### Risk 3 — Team bắt đầu Notes/Tasks quá sớm
**Mitigation**
- Không vào feature sprint khi phase 1 exit criteria chưa đạt

### Risk 4 — Docs không theo kịp code
**Mitigation**
- Mỗi sprint phải có deliverable docs tương ứng
- Không đóng sprint nếu docs nền chưa cập nhật

---

## 9. Tracking Recommendation

Khuyến nghị board theo 4 trạng thái:
- Todo
- In Progress
- Review
- Done

Mỗi task nên có:
- ID
- Owner
- Scope ngắn
- Acceptance criteria
- Dependency nếu có

---

## 10. Phase 1 Final Readiness Checklist

- [ ] Repo structure đã chốt và áp dụng
- [ ] Backend skeleton chạy được
- [ ] Frontend skeleton chạy được
- [ ] Mobile skeleton chạy được
- [ ] Shared contracts baseline đã có
- [ ] Auth/ownership baseline rõ
- [ ] DB baseline + migrations rõ
- [ ] API conventions rõ
- [ ] Error handling base có
- [ ] Test baseline có
- [ ] CI baseline có
- [ ] Getting started docs có
- [ ] Team sẵn sàng bước sang Notes/Tasks implementation

---

## 11. Next Step After Phase 1

Sau khi hoàn tất Phase 1:
- kick off `backlog.notes.md`
- kick off `backlog.tasks.md`
- giao sprint theo module
- dùng acceptance criteria riêng để QC và PO review
