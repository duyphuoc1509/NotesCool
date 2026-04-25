# PRD — MVP V1

> Mục tiêu: Product Requirement Document cho release đầu tiên của dự án Notes - Tasks.

---

## 1. Product Overview

Notes - Tasks là sản phẩm hỗ trợ người dùng quản lý ghi chú và công việc trong cùng một hệ thống gọn nhẹ, rõ ràng, dễ dùng trên web và mobile.

MVP v1 tập trung vào:
- giải quyết nhu cầu quản lý note và task cá nhân
- tạo nền kiến trúc đủ sạch để phát triển lâu dài
- có thể demo nội bộ và pilot sớm với nhóm người dùng đầu tiên

---

## 2. Problem Statement

Người dùng hiện thường phải:
- ghi chú ở một nơi
- quản lý việc cần làm ở một nơi khác
- tự kết nối thông tin giữa note và task
- chịu trải nghiệm rời rạc, khó duy trì

Sản phẩm này giải quyết bằng cách:
- đưa Notes và Tasks vào cùng hệ thống
- giữ trải nghiệm gọn, tập trung, dễ dùng
- cho phép mở rộng liên kết giữa Notes và Tasks theo phase sau

---

## 3. Product Vision

Xây dựng một workspace cá nhân đơn giản nhưng có cấu trúc tốt, nơi người dùng có thể ghi chú và quản lý công việc hằng ngày trong cùng một sản phẩm, trên cả web và mobile.

---

## 4. Product Goals

### G1. Validate problem-solution fit
Xác minh rằng người dùng thấy giá trị khi quản lý Notes và Tasks trong cùng một app.

### G2. Deliver a usable MVP quickly
Ra được một phiên bản nhỏ nhưng dùng được thật, không sa vào over-engineering.

### G3. Establish scalable product foundation
Đặt nền module boundaries, contracts, solution structure và delivery discipline đủ tốt cho giai đoạn mở rộng.

---

## 5. Target Users

### Primary users
- Cá nhân quản lý công việc và ghi chú hằng ngày
- Freelancer, maker, developer, BA, PO hoặc người làm việc tri thức cần lưu ý tưởng và việc cần làm ở cùng một nơi

### Early adopter profile
- Muốn sản phẩm gọn
- Không cần hệ thống project management quá nặng
- Ưu tiên tốc độ và clarity hơn nhiều tính năng phức tạp

---

## 6. User Needs

Người dùng cần:
- ghi chú nhanh
- xem lại và chỉnh sửa note dễ dàng
- tạo task từ nhu cầu hằng ngày
- biết việc nào đang cần làm, sắp tới hạn, đã xong
- truy cập từ web và mobile
- cảm thấy dữ liệu của mình rõ ràng, nhất quán và an toàn

---

## 7. MVP Scope

## Must-have
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

## Should-have
- Archive thay vì hard delete
- Basic tags
- Link task với note ở mức đơn giản
- Search cải thiện hơn basic keyword
- UI polish cho empty/loading/error states

## Not-now
- Collaboration realtime
- Reminder delivery thực
- Push notifications
- Realtime sync
- Kanban nâng cao
- Recurrence phức tạp
- AI features
- Plugin marketplace
- Enterprise RBAC phức tạp

---

## 8. Core Use Cases

### UC-01 Create and manage notes
Người dùng có thể tạo, sửa, xem, xóa hoặc archive note của mình.

### UC-02 Create and manage tasks
Người dùng có thể tạo, sửa, xem, xóa hoặc archive task của mình.

### UC-03 Update task status
Người dùng có thể đổi trạng thái task để phản ánh tiến độ.

### UC-04 Find important information quickly
Người dùng có thể tìm notes và lọc tasks để quay lại công việc nhanh.

### UC-05 Work across web and mobile
Người dùng có thể dùng cùng dữ liệu trên web và mobile với trải nghiệm nhất quán.

---

## 9. Functional Requirements

### FR-01 Authentication & Ownership
- Hệ thống phải hỗ trợ xác thực mức tối thiểu cho MVP
- Mỗi note/task phải thuộc về một owner rõ ràng
- User không thấy dữ liệu của user khác

### FR-02 Notes
- Tạo note
- Xem danh sách notes
- Xem chi tiết note
- Cập nhật note
- Xóa/archive note
- Search/filter cơ bản

### FR-03 Tasks
- Tạo task
- Xem danh sách tasks
- Xem chi tiết task
- Cập nhật task
- Đổi trạng thái task
- Xóa/archive task
- Filter/sort cơ bản

### FR-04 Web Client
- Hỗ trợ full flow MVP cho Notes và Tasks

### FR-05 Mobile Client
- Hỗ trợ các flow cốt lõi của MVP cho Notes và Tasks

---

## 10. Non-Functional Requirements

### NFR-01 Maintainability
- Kiến trúc modular monolith first
- Boundaries rõ giữa Notes, Tasks, Shared

### NFR-02 Contract Stability
- Public API phải ổn định cho web/mobile

### NFR-03 Security
- Ownership enforced
- Input validation đầy đủ ở boundary
- Không lộ dữ liệu ngoài phạm vi user

### NFR-04 Performance
- List APIs phải có pagination
- Search/filter mức cơ bản đủ dùng cho MVP
- Query chính không có pattern rõ ràng gây N+1

### NFR-05 Delivery Efficiency
- Repo structure và docs phải đủ rõ để team khởi động nhanh
- Có baseline test và CI để giảm regressions sớm

---

## 11. UX Principles for MVP

- Gọn
- Rõ
- Ít bước
- Nhất quán giữa web và mobile
- Ưu tiên use case chính trước visual polish nâng cao
- Empty/loading/error states phải dễ hiểu

---

## 12. Success Metrics

### Product metrics
- Có thể demo toàn bộ flow MVP nội bộ trơn tru
- User đầu tiên có thể hoàn thành use case chính mà không cần hướng dẫn nhiều
- Team có thể mở rộng feature mà không phải sửa lớn kiến trúc nền

### Delivery metrics
- Phase 1 foundation hoàn tất đúng exit criteria
- Notes/Tasks implementation bắt đầu trên nền contracts rõ
- Tỷ lệ rework do scope ambiguity giảm

---

## 13. Risks

### R1. Scope creep
**Mitigation**
- Bám `mvp-scope.md`
- Chia Must-have / Should-have / Not-now rõ ràng

### R2. Over-engineering
**Mitigation**
- Chỉ làm plugin-ready ở mức direction, không xây plugin system hoàn chỉnh trong MVP

### R3. Cross-module coupling
**Mitigation**
- Bám `module-map.md`
- Có architecture guardrails và review checklist

### R4. Contract drift giữa web/mobile/backend
**Mitigation**
- Bám `api-conventions.md`
- Contract-first và test/check từ sớm

---

## 14. Release Strategy

### Release 1
- Foundation hoàn chỉnh
- Notes MVP
- Tasks MVP
- Web usable
- Mobile usable cho flows chính

### Release 1.1
- Polish UX
- Link note-task đơn giản
- Archive/tag/search tốt hơn
- Ổn định hóa sau pilot

---

## 15. Dependencies

- `kickoff-prompt.foundation.md`
- `kickoff-prompt.notes.md`
- `kickoff-prompt.tasks.md`
- `kickoff-prompt.mvp-v1.md`
- `module-map.md`
- `mvp-scope.md`
- `api-conventions.md`
- `solution-structure.md`
- `backlog.foundation.md`
- `backlog.notes.md`
- `backlog.tasks.md`

---

## 16. Open Decisions

- Auth approach cụ thể cho MVP
- Archive vs hard delete policy mặc định
- Có đưa basic tags vào Must-have hay Should-have
- Có đưa linkedNoteId vào MVP hay phase 1.1
- Mức độ mobile parity chính xác với web

---

## 17. Approval Criteria

PRD MVP v1 được xem là chốt khi:
- CEO/PO đồng ý phạm vi
- Tech lead đồng ý technical direction
- BA đồng ý requirements đủ rõ
- DEV/QC/Designer có thể dùng tài liệu để bắt đầu delivery
