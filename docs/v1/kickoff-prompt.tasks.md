# KICKOFF PROMPT

> Mục tiêu: chỉ cần điền file này một lần cho mỗi đợt làm việc.
>
> Khi bắt đầu session, chỉ cần nhắn: **"Bắt đầu từ `kickoff-prompt.tasks.md`"**.
>
> Claude sẽ đọc file này đầu tiên, rồi mới tiếp tục discovery / planning / implementation.

---

## 1. Task Summary

**Tên task / initiative**
- Xây dựng Tasks module cho dự án Notes - Tasks

**Mục tiêu chính**
- Thiết kế và triển khai Tasks module theo hướng module-first, plugin-safe, dễ mở rộng, hỗ trợ quản lý vòng đời công việc rõ ràng trên web và mobile.

**Kết quả mong muốn**
- Tài liệu khởi tạo riêng cho Tasks module
- Scope chức năng rõ ràng cho task lifecycle
- API/contracts mức implementation-ready
- Data model khởi đầu cho Tasks
- Test strategy, boundaries, non-goals và Definition of Done rõ ràng

---

## 2. Working Mode

**Loại công việc**
- [x] Discovery
- [x] Planning
- [x] Documentation
- [ ] Implementation
- [ ] Refactor
- [ ] Bug fix
- [ ] Review

**Ưu tiên cách làm**
- [ ] TDD-first
- [x] Contract-first
- [x] Minimal diff
- [x] Modular/plugin-safe
- [ ] Chỉ cập nhật docs
- [ ] Chỉ phân tích, chưa code

**Mức độ chủ động mong muốn**
- [ ] Hỏi tôi trước các quyết định lớn
- [ ] Tự chủ vừa phải, chỉ hỏi khi có ambiguity lớn
- [x] Chủ động cao, cứ làm theo best judgment

---

## 3. Project Direction

**Tech stack**
- Backend: .NET 8, ASP.NET Core Web API
- Frontend Web: ReactJS
- Mobile App: React Native
- Database: PostgreSQL
- ORM: EF Core

**Architecture direction**
- Tasks là business module độc lập trong modular monolith
- Plugin-ready, contract-first
- Domain logic phải nằm trong module Tasks
- Có thể mở rộng thêm comment, reminder, recurrence, label, collaboration ở phase sau mà không làm vỡ core design

**Boundaries cần tôn trọng**
- Tasks module không phụ thuộc trực tiếp vào internals của Notes module
- Shared chỉ chứa primitives, common abstractions, shared result/error contracts
- Nếu Tasks liên kết với Notes thì chỉ đi qua contract/interface/event
- UI không được phụ thuộc trực tiếp vào persistence model
- Plugin mở rộng Tasks chỉ được đi qua abstraction/contracts

**Non-goals / không làm trong task này**
- Chưa làm collaboration realtime
- Chưa làm task recurrence phức tạp
- Chưa làm dependency graph giữa tasks
- Chưa làm notification delivery thực tế
- Chưa làm reporting/analytics nâng cao
- Chưa làm board/timeline nâng cao nếu chưa cần cho v1

---

## 4. Scope & Paths

**Files / folders cần ưu tiên đọc trước**
- docs/product/
- docs/architecture/
- src/backend/modules/tasks/
- src/frontend/
- src/mobile/
- tests/

**Files / folders không được đụng vào**
- Internal implementation của module khác nếu không có contract rõ ràng
- None ngoài boundary rules

**Target paths nếu có sửa**
- docs/product/tasks/
- docs/architecture/modules/tasks/
- src/backend/modules/tasks/
- src/frontend/features/tasks/
- src/mobile/features/tasks/
- tests/tasks/

---

## 5. Requirements

**Business / functional requirements**
- [REQ-01] Người dùng có thể tạo task mới
- [REQ-02] Người dùng có thể cập nhật title, description, status, priority, due date của task
- [REQ-03] Người dùng có thể đánh dấu hoàn thành / chưa hoàn thành
- [REQ-04] Người dùng có thể xem danh sách tasks theo filter và sorting cơ bản
- [REQ-05] Người dùng có thể xóa hoặc archive task theo policy sản phẩm
- [REQ-06] Mỗi task phải có lifecycle rõ ràng và nhất quán trên web/mobile
- [REQ-07] Thiết kế phải sẵn sàng mở rộng cho reminder, label, recurrence, subtask ở phase sau
- [REQ-08] Có thể hỗ trợ liên kết task với note hoặc context khác qua contract mở rộng, không hard-couple

**Technical requirements**
- [TECH-01] Backend dùng .NET 8
- [TECH-02] Contracts rõ ràng cho create/update/get/list task APIs
- [TECH-03] Data model phải hỗ trợ status, priority, due date, audit fields
- [TECH-04] Phải có pagination/filtering/sorting cho task listing
- [TECH-05] Không để domain rules nằm rải rác ở controller/UI
- [TECH-06] Thiết kế plugin-safe cho extensions sau này
- [TECH-07] Có architecture tests hoặc equivalent checks để giữ module boundary
- [TECH-08] DTO/API contracts phải ổn định đủ cho web/mobile cùng dùng

**Security / performance / compliance requirements**
- [SEC-01] Chỉ cho phép người dùng truy cập tasks thuộc phạm vi được cấp quyền
- [SEC-02] Validate input tại API boundary và application boundary
- [SEC-03] Không expose internal fields không cần thiết qua public API
- [PERF-01] List API phải hỗ trợ query hiệu quả với pagination
- [PERF-02] Cần chuẩn bị index strategy cho owner, status, due date, updated time
- [PERF-03] Tránh N+1 query khi mở rộng relation về sau
- [PERF-04] Mobile/web phải có contract nhẹ, dễ cache và ổn định

---

## 6. Contracts & Data

**API / contract impacted**
- POST /tasks
- GET /tasks/{id}
- GET /tasks
- PUT/PATCH /tasks/{id}
- POST /tasks/{id}/complete hoặc PATCH status
- DELETE /tasks/{id} hoặc archive endpoint theo policy
- Shared paging/filter/result/error contracts
- Internal interfaces cho application services / repositories / events

**Data model impacted**
- Task entity
- Task status
- Task priority
- Due date / reminder-ready fields
- Audit fields: created at, updated at, created by, updated by
- Soft delete hoặc archive fields nếu sản phẩm chọn hướng đó

**Compatibility constraints**
- API contracts phải tránh breaking change tùy tiện
- Nếu đổi enum/status/priority phải đánh giá impact lên web/mobile
- Nếu thêm relation với Notes hoặc modules khác phải đi qua contract rõ ràng
- Nếu chọn soft delete/archive thì policy phải nhất quán từ đầu

---

## 7. Testing Expectations

**Test strategy cho task này**
- [x] Unit tests
- [x] Contract tests
- [x] Integration tests
- [x] Architecture tests
- [ ] E2E / smoke tests
- [ ] Chưa cần test ở bước này

**Definition of Done cho task này**
- [x] Code/doc đúng requirement
- [x] Tests pass
- [x] Không vi phạm module boundary
- [x] Update docs liên quan
- [x] Review xong

**Nếu phải làm theo TDD, RED test đầu tiên nên chứng minh gì?**
- Tasks module có thể tạo mới một task hợp lệ qua contract chuẩn, áp đúng domain rules tối thiểu và không phụ thuộc trực tiếp vào internals của module khác

---

## 8. References

**Docs / issue / note liên quan**
- kickoff-prompt.foundation.md
- kickoff-prompt.notes.md
- README.md
- docs/architecture/*
- docs/product/*

**Context thêm nếu cần**
- Tasks là một trong hai business modules cốt lõi của sản phẩm
- Thiết kế phải ưu tiên khả năng scale tính năng theo phase mà không phải rewrite core
- CEO/product direction: chất lượng tốt, boundaries sạch, delivery hiệu quả, dễ phối hợp giữa PO/BA/DEV/QC/Designer

---

## 9. Output Preference

**Bạn muốn Claude trả kết quả theo kiểu nào?**
- [ ] Chỉ summary ngắn
- [x] Summary + file list
- [x] Summary + next steps
- [x] Giải thích kỹ quyết định

**Ngôn ngữ phản hồi mong muốn**
- [x] Tiếng Việt
- [ ] English
- [ ] Song ngữ

---

## 10. Ready-to-Run Command

Khi điền xong file này, chỉ cần nhắn:

```text
Bắt đầu từ kickoff-prompt.tasks.md
```

Hoặc nếu muốn rõ hơn:

```text
Đọc kickoff-prompt.tasks.md và thực hiện theo đó.
```

---

## 11. Notes for Claude

Khi đọc file này, Claude nên:
1. Tóm tắt lại hiểu biết từ file.
2. Xác định mode: discovery / planning / implementation / review.
3. Nếu thông tin đã đủ rõ thì bắt đầu luôn.
4. Chỉ hỏi lại khi file còn thiếu dữ liệu quan trọng hoặc có mâu thuẫn lớn.
5. Luôn ưu tiên task lifecycle rõ ràng, stable contracts, module boundaries, khả năng mở rộng và trải nghiệm nhất quán giữa web/mobile.
