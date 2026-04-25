# MASTER PLAYBOOK — MVP V1

> Mục tiêu: tài liệu điều hành trung tâm cho dự án Notes - Tasks ở giai đoạn MVP v1, dùng cho CEO/PO, BA, Tech Lead, DEV, QC, Designer như một nguồn tham chiếu thống nhất.

---

## 1. Purpose of This Playbook

Tài liệu này tổng hợp toàn bộ hướng đi của MVP v1:
- product direction
- architecture direction
- scope
- delivery plan
- backlog
- sprint execution
- acceptance & QA
- governance & risk

Mục tiêu là để cả team có thể trả lời nhanh các câu hỏi:
- Ta đang xây cái gì?
- Ta không xây cái gì?
- Kiến trúc phải giữ điều gì?
- Sprint tới làm gì?
- Khi nào được coi là done?
- Rủi ro nào phải theo dõi sát?

---

## 2. Product Summary

### Product name
- Notes - Tasks

### Product idea
Một sản phẩm giúp người dùng quản lý ghi chú và công việc trong cùng một hệ thống gọn nhẹ, rõ ràng, dùng được trên web và mobile.

### MVP v1 goal
- validate nhu cầu thật của người dùng với combo Notes + Tasks
- ra được phiên bản nhỏ nhưng usable
- đặt nền kiến trúc đủ sạch để mở rộng về sau

### Product vision
Xây dựng một workspace cá nhân đơn giản nhưng có cấu trúc tốt, nơi người dùng có thể ghi chú và quản lý công việc hằng ngày trong cùng một sản phẩm.

---

## 3. MVP Scope

### Must-have
- Auth/ownership tối thiểu
- Notes CRUD
- Tasks CRUD
- Task status update
- Notes list + search cơ bản
- Tasks list + filter/sort cơ bản
- Web flows cho use cases chính
- Mobile flows cho use cases chính
- API contracts rõ
- Test baseline đủ dùng

### Should-have
- Archive thay vì hard delete
- Basic tags
- Link task với note ở mức đơn giản
- Better search
- Better empty/loading/error states

### Not-now
- Collaboration realtime
- Reminder delivery thực
- Push notifications
- Realtime sync
- Recurrence phức tạp
- Kanban nâng cao
- AI features
- Plugin marketplace
- Enterprise RBAC phức tạp

### Product success criteria
- User có thể dùng web để hoàn thành flow chính
- User có thể dùng mobile cho flow cốt lõi
- Notes và Tasks usable ở mức MVP
- Demo nội bộ trơn tru
- Kiến trúc đủ sạch để tiếp tục phase sau

---

## 4. Architecture Direction

### Chosen direction
- Modular monolith first
- Plugin-ready
- Contract-first
- Module boundaries rõ
- UI feature-based structure
- Shared kernel tối giản

### Core modules
- Shared Kernel
- Notes Module
- Tasks Module
- Host/API Composition Layer
- Frontend Web App
- Mobile App

### Key rules
- Notes không phụ thuộc trực tiếp internals của Tasks
- Tasks không phụ thuộc trực tiếp internals của Notes
- Shared không chứa business logic module-specific
- Web/mobile không bám backend internals
- Cross-module communication đi qua contracts/interfaces/events nếu cần

### Guardrails
- Architecture tests/checks
- Review theo `module-map.md`
- Không merge coupling sai chỉ vì tiện

---

## 5. Contract Direction

### API principles
- REST-first
- Base URL: `/api/v1`
- JSON camelCase
- Error format thống nhất
- Pagination bắt buộc cho list endpoints
- Ownership lấy từ auth context
- Web/mobile dùng cùng public contracts

### Main endpoints

#### Notes
- `POST /api/v1/notes`
- `GET /api/v1/notes`
- `GET /api/v1/notes/{id}`
- `PATCH /api/v1/notes/{id}`
- `DELETE /api/v1/notes/{id}`

#### Tasks
- `POST /api/v1/tasks`
- `GET /api/v1/tasks`
- `GET /api/v1/tasks/{id}`
- `PATCH /api/v1/tasks/{id}`
- `PATCH /api/v1/tasks/{id}/status`
- `DELETE /api/v1/tasks/{id}`

### Ownership rule
- User chỉ được thấy và thao tác trên dữ liệu của mình
- Ownership không lấy từ request body
- Security là một phần của MVP, không để làm sau

---

## 6. Solution Structure

### Monorepo direction
Khuyến nghị dùng monorepo cho phase đầu để:
- đồng bộ docs
- đồng bộ contracts
- tăng tốc collaboration
- phù hợp modular monolith first

### Top-level structure
```text
/
├── README.md
├── docs/
├── src/
├── tests/
├── scripts/
├── docker/
└── .env.example
```

### Main source layout
```text
src/
├── backend/
│   ├── host/
│   ├── shared/
│   └── modules/
│       ├── notes/
│       └── tasks/
├── frontend/
│   ├── app/
│   ├── features/
│   │   ├── notes/
│   │   └── tasks/
│   └── shared/
└── mobile/
    ├── app/
    ├── features/
    │   ├── notes/
    │   └── tasks/
    └── shared/
```

### Tests
```text
tests/
├── backend/
│   ├── unit/
│   ├── integration/
│   └── architecture/
├── frontend/
└── mobile/
```

---

## 7. Delivery Strategy

### Phase 0 — Direction
- Vision
- MVP scope
- Module map
- Solution structure
- API conventions

### Phase 1 — Foundation
- Backend skeleton
- Frontend skeleton
- Mobile skeleton
- Shared contracts
- Auth baseline
- DB baseline
- Test baseline
- CI baseline

### Phase 2 — Notes MVP
- Domain
- Contracts
- Backend flows
- Web flows
- Mobile flows
- Tests/docs

### Phase 3 — Tasks MVP
- Domain
- Contracts
- Backend flows
- Web flows
- Mobile flows
- Tests/docs

### Phase 4 — Integration & hardening
- Quality improvements
- Link note-task nếu được chọn
- Polish demo flows
- Stabilization

---

## 8. Phase 1 Execution Plan

### Sprint 1 — Foundation Setup
- repo structure
- backend skeleton
- frontend skeleton
- mobile skeleton
- naming conventions
- baseline test projects

### Sprint 2 — Technical Baseline
- shared primitives
- API response conventions
- user/time context abstractions
- auth/ownership baseline
- PostgreSQL + EF baseline
- API client baseline

### Sprint 3 — Quality Gates & Readiness
- validation/error handling base
- audit/archive policy
- shared UI states
- integration test baseline
- architecture tests
- CI baseline
- getting-started docs
- architecture overview docs

### Exit criteria for Phase 1
- backend/web/mobile skeleton chạy được
- contracts baseline rõ
- auth/ownership baseline rõ
- DB baseline rõ
- tests baseline có
- team sẵn sàng bước sang Notes/Tasks implementation

---

## 9. Notes Module Play

### Goal
Cho user tạo, xem, sửa, xóa/archive notes và tìm lại notes của mình trên web/mobile.

### Notes delivery sequence
#### Sprint N1
- entity/rules
- DTO contracts
- endpoints
- persistence
- create/get/list/update/delete or archive
- tests baseline

#### Sprint N2
- web Notes list/detail/editor
- mobile Notes list/detail/editor
- search/filter cơ bản
- delete/archive action

#### Sprint N3
- QA pass
- docs
- regression fixes
- demo readiness

### Notes done criteria
- CRUD usable
- ownership đúng
- list/search cơ bản đúng
- web/mobile flows usable
- tests/docs đủ
- `acceptance-criteria.notes.md` pass

---

## 10. Tasks Module Play

### Goal
Cho user tạo, xem, sửa, xóa/archive tasks và cập nhật trạng thái task trên web/mobile.

### Tasks delivery sequence
#### Sprint T1
- entity/rules
- lifecycle/status rules
- DTO contracts
- endpoints
- persistence
- create/get/list/update/status/delete or archive
- tests baseline

#### Sprint T2
- web Tasks list/detail/editor
- mobile Tasks list/detail/editor
- filter/sort/status actions
- delete/archive action

#### Sprint T3
- QA pass
- docs
- regression fixes
- demo readiness

### Tasks done criteria
- CRUD usable
- status update usable
- list/filter/sort cơ bản đúng
- ownership đúng
- web/mobile flows usable
- tests/docs đủ
- `acceptance-criteria.tasks.md` pass

---

## 11. Working Model

### Team principles
- scope clarity first
- contract-first
- boundary-safe delivery
- small, reviewable increments
- docs stay close to delivery
- raise risks early

### Role highlights
#### CEO/PO
- chốt scope
- chốt ưu tiên
- chống lan scope
- approve readiness

#### BA
- làm rõ use cases
- viết acceptance criteria
- giữ consistency requirement

#### Tech Lead
- giữ technical direction
- giữ module boundaries
- review contracts/architecture

#### DEVs
- triển khai đúng conventions
- không bypass contracts
- không bypass boundaries

#### QC
- test theo acceptance criteria
- báo lỗi rõ severity, repro, impact

#### Designer
- giữ flow rõ
- giữ consistency web/mobile
- không mở rộng visual scope quá sớm

---

## 12. Definition of Ready

Một item chỉ nên vào sprint khi:
- scope đủ rõ
- owner rõ
- acceptance criteria rõ
- dependency chính đã biết
- contract/flow liên quan đã rõ
- không còn ambiguity lớn

---

## 13. Definition of Done

Một item chỉ được xem là done khi:
- scope đã hoàn thành đúng
- acceptance criteria pass
- review phù hợp đã xong
- tests/check liên quan pass
- docs/UI/code liên quan đã cập nhật nếu cần
- không còn blocker nghiêm trọng bị che giấu

### MVP Done
MVP chỉ được xem là ready khi:
- must-have scope đạt
- Notes flows hoạt động
- Tasks flows hoạt động
- web usable
- mobile usable cho flow cốt lõi
- auth/ownership đúng
- API contracts ổn định
- QA checklist quan trọng pass
- không còn blocker severity mở

---

## 14. Acceptance & QA

### Notes acceptance source
- `acceptance-criteria.notes.md`

### Tasks acceptance source
- `acceptance-criteria.tasks.md`

### QA source
- `qa-checklist.mvp-v1.md`

### QA release rule
Không coi MVP là ready nếu:
- flow chính không demo được
- ownership/security còn lỗi
- blocker/high severity còn chưa xử lý rõ
- contract mismatch làm web/mobile chạy không ổn định

---

## 15. Risk Management

### Top risks cần review hàng tuần
- Scope creep
- Over-engineering quá sớm
- Cross-module coupling
- API contract drift
- Foundation chưa đủ nhưng vào feature quá sớm
- Auth/ownership chưa rõ
- Test baseline yếu
- Docs lệch implementation

### Risk review cadence
- review hàng tuần
- review tại sprint planning
- review trước demo/release

### Escalation triggers
- risk đe dọa sprint goal
- risk gây rework lớn
- risk liên quan security/ownership
- risk có thể gây breaking contract
- risk làm lệch MVP scope

---

## 16. Weekly Operating Cadence

### Monday
- sprint planning / weekly plan
- review priorities
- confirm scope & owners

### Mid-week
- sync kiến trúc/kỹ thuật
- review blockers
- review risks chính

### Friday
- demo
- retro
- update backlog
- chốt next steps

### Daily
- standup ngắn
- tập trung blockers và dependency

---

## 17. Recommended Source of Truth Map

### Kickoff prompts
- `kickoff-prompt.foundation.md`
- `kickoff-prompt.notes.md`
- `kickoff-prompt.tasks.md`
- `kickoff-prompt.mvp-v1.md`

### Product & architecture
- `prd.mvp-v1.md`
- `mvp-scope.md`
- `module-map.md`
- `api-conventions.md`
- `solution-structure.md`

### Delivery
- `delivery-plan.phase-1.md`
- `sprint-plan.phase-1.md`
- `sprint-plan.notes.md`
- `sprint-plan.tasks.md`

### Backlogs
- `backlog.foundation.md`
- `backlog.notes.md`
- `backlog.tasks.md`

### Quality & governance
- `acceptance-criteria.notes.md`
- `acceptance-criteria.tasks.md`
- `qa-checklist.mvp-v1.md`
- `team-working-agreement.md`
- `definition-of-done.md`
- `risk-register.mvp-v1.md`

---

## 18. Immediate Next Actions for CEO/PO

1. Review và chốt `master-playbook.mvp-v1.md` như tài liệu trung tâm
2. Chốt các open decisions còn lại:
   - auth approach
   - delete vs archive policy
   - tags vào Must-have hay Should-have
   - linkedNoteId ở MVP hay 1.1
   - mức parity mobile với web
3. Kick off phase 1 execution planning với team
4. Tạo board/task breakdown thật từ các backlog đã có
5. Chỉ bắt đầu feature implementation sau khi phase 1 đạt exit criteria

---

## 19. Suggested Command to Start Execution Planning

Sau khi review playbook này, bạn có thể dùng câu lệnh:

```text
Đọc master-playbook.mvp-v1.md cùng các tài liệu liên quan, sau đó lập implementation planning chi tiết cho phase 1 và đề xuất task breakdown theo owner.
```

---

## 20. Final Summary

Master playbook này là tài liệu trung tâm để:
- giữ cả team cùng một hướng
- tránh lan scope
- tránh coupling sai
- điều hành delivery theo từng phase
- giữ chất lượng đủ tốt cho MVP v1
