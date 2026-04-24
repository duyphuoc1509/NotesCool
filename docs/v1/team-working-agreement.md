# TEAM WORKING AGREEMENT

> Mục tiêu: thống nhất cách team làm việc để dự án Notes - Tasks vận hành rõ ràng, hiệu quả, ít hiểu sai và ít rework.

---

## 1. Purpose

Tài liệu này xác định:
- cách team phối hợp
- cách ra quyết định
- cách quản lý scope
- cách review và handoff
- nguyên tắc giao tiếp và delivery

Mục tiêu là giúp team:
- đi nhanh nhưng không loạn
- giữ chất lượng sản phẩm
- giữ boundaries kiến trúc
- giảm ambiguity và rework

---

## 2. Team Principles

### P1. Scope clarity first
Không bắt đầu implementation khi scope và acceptance criteria còn mơ hồ ở mức quan trọng.

### P2. Contract-first
Public contracts phải được làm rõ trước khi làm sâu backend/web/mobile.

### P3. Boundary-safe delivery
Không hy sinh module boundaries để “xong nhanh” trong ngắn hạn.

### P4. Small, reviewable increments
Ưu tiên chia nhỏ việc để review và demo sớm.

### P5. Docs stay close to delivery
Tài liệu cần cập nhật đủ để hỗ trợ delivery, không để lệch xa code và scope thực tế.

### P6. Raise risks early
Blocker, ambiguity, dependency phải được nêu sớm, không giữ đến cuối sprint.

---

## 3. Roles & Responsibilities

## CEO/PO
- Chốt product direction
- Chốt ưu tiên
- Chống lan scope
- Ra quyết định cuối với trade-off product
- Approve MVP scope và release readiness

## BA
- Làm rõ use cases
- Bóc tách requirement
- Viết acceptance criteria
- Hỗ trợ PO kiểm soát scope và consistency

## Tech Lead
- Chốt technical direction
- Giữ architecture guardrails
- Review decisions về contracts, boundaries, persistence strategy
- Escalate technical risks sớm

## Backend Dev
- Triển khai host/shared/modules theo conventions
- Giữ API contracts ổn định
- Không tạo coupling sai giữa modules

## Frontend Dev
- Bám API contracts
- Giữ feature boundaries rõ
- Không encode business logic phức tạp sai chỗ

## Mobile Dev
- Bám API contracts
- Tối ưu flows cốt lõi cho mobile
- Giữ consistency với web ở mức MVP

## QC
- Viết/check test scenarios theo acceptance criteria
- Thực hiện smoke/regression checks
- Nêu rõ severity, impact, repro steps

## Designer
- Chốt flow, information layout, interaction states
- Giữ consistency web/mobile
- Tránh mở rộng scope visual quá mức trong MVP

---

## 4. Planning Rules

- Mọi sprint phải có sprint goal rõ
- Mỗi item phải có owner rõ
- Mỗi item phải có acceptance criteria hoặc link tới acceptance criteria
- Không đưa item vào sprint nếu thiếu quyết định đầu vào quan trọng
- Scope mới phát sinh phải qua PO review

---

## 5. Backlog Rules

Mỗi backlog item nên có:
- ID
- Title
- Goal ngắn
- Acceptance criteria
- Owner
- Dependency
- Priority
- Status

### Priority model
- P0: blocker/critical path
- P1: must-have cho MVP
- P2: should-have
- P3: not-now hoặc follow-up

---

## 6. Delivery Rules

### DR1. Do not start with ambiguous requirements
Nếu requirement chưa rõ ở điểm ảnh hưởng thiết kế/contract, phải hỏi hoặc chốt lại trước.

### DR2. Do not bypass contracts
Frontend/mobile không bám internal model hoặc undocumented fields.

### DR3. Do not bypass boundaries
Notes không được phụ thuộc trực tiếp internals của Tasks và ngược lại.

### DR4. Do not hide unfinished work
Nếu item chưa xong hoặc có compromise, phải ghi rõ.

### DR5. Keep PRs manageable
PR nên đủ nhỏ để review thực chất được.

---

## 7. Communication Rules

### Daily communication
- Ngắn
- Rõ blockers
- Rõ dependency
- Không kể quá nhiều chi tiết không cần thiết

### Escalation triggers
Phải escalate sớm nếu:
- scope mơ hồ
- risk ảnh hưởng sprint goal
- contract chưa thống nhất
- dependency bị chậm
- architecture direction có dấu hiệu lệch

### Decision logging
Mọi quyết định quan trọng nên được lưu thành doc/ngắn gọn:
- vấn đề
- options
- lựa chọn
- lý do
- impact

---

## 8. Review Rules

### Product review
- So với PRD/MVP scope
- So với acceptance criteria
- So với UX flow đã chốt

### Technical review
- So với module-map
- So với api-conventions
- So với solution-structure
- So với quality gates

### QA review
- So với qa-checklist
- Có repro steps rõ
- Có severity rõ
- Có expected vs actual rõ

---

## 9. Definition of Ready

Một item được xem là ready để làm khi:
- scope đủ rõ
- owner rõ
- acceptance criteria rõ
- dependencies chính đã biết
- contract/flow liên quan đã rõ
- không còn ambiguity lớn

---

## 10. Demo Rules

Mỗi sprint review/demo nên:
- bám sprint goal
- demo flow thật, không chỉ show code
- nêu rõ phần done / phần chưa done
- nêu risks còn lại
- nêu next step rõ

---

## 11. Change Management Rules

Khi có thay đổi quan trọng về:
- scope
- contract
- architecture
- timeline

thì cần:
- đánh giá impact
- cập nhật docs liên quan
- thông báo cho các vai trò bị ảnh hưởng
- re-plan nếu cần

---

## 12. Conflict Resolution

Khi có xung đột:
- ưu tiên scope clarity
- ưu tiên product value
- ưu tiên long-term maintainability trong các quyết định nền
- CEO/PO quyết về product trade-off
- Tech Lead quyết về technical guardrails
- Nếu chưa đủ dữ liệu, timebox và chọn hướng đơn giản hơn cho MVP

---

## 13. Quality Expectations

- Không merge knowingly breaking contract nếu chưa có plan rõ
- Không chấp nhận known blocker bị “để sau” mà không được PO/QC/Tech Lead đồng ý
- Không coi “build chạy” là đủ done
- Luôn review ownership/security cho các flow chính

---

## 14. Working Agreement Summary

Team đồng ý rằng:
- làm đúng scope quan trọng hơn làm nhiều
- boundaries quan trọng hơn shortcut ngắn hạn
- demo flow thật quan trọng hơn báo cáo tiến độ mơ hồ
- docs, contracts, tests là một phần của delivery
