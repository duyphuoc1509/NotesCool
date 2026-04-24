# KICKOFF PROMPT

> Mục tiêu: chỉ cần điền file này một lần cho mỗi đợt làm việc.
>
> Khi bắt đầu session, chỉ cần nhắn: **"Bắt đầu từ `kickoff-prompt.mvp-v1.md`"**.
>
> Claude sẽ đọc file này đầu tiên, rồi mới tiếp tục discovery / planning / implementation.

---

## 1. Task Summary

**Tên task / initiative**
- Chốt phạm vi MVP v1 cho dự án Notes - Tasks

**Mục tiêu chính**
- Xác định phạm vi release đầu tiên đủ nhỏ để triển khai nhanh, đủ giá trị để kiểm chứng sản phẩm, và đủ rõ để team có thể bắt đầu delivery theo module mà không bị lan scope.

**Kết quả mong muốn**
- Tài liệu MVP v1 rõ ràng
- Danh sách tính năng Must-have / Should-have / Not-now
- Phạm vi release đầu tiên cho backend, web, mobile
- Danh sách acceptance criteria ở mức sản phẩm
- Căn cứ để chuyển sang backlog phase 1 và implementation planning

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
- [x] Chỉ phân tích, chưa code

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
- Modular monolith first
- Plugin-ready / module-first
- Contract-first giữa backend và clients
- MVP v1 ưu tiên release nhanh, boundaries sạch, giữ đường mở rộng cho Notes/Tasks về sau

**Boundaries cần tôn trọng**
- Không mở rộng scope ngoài MVP nếu chưa có quyết định rõ
- Notes và Tasks vẫn là 2 business modules độc lập
- Shared chỉ chứa abstractions / common primitives / shared result-error contracts
- Mọi tích hợp giữa modules phải đi qua contract, không truy cập trực tiếp internals
- Web/mobile phải dùng cùng API contracts ổn định cho MVP

**Non-goals / không làm trong task này**
- Chưa xây plugin marketplace
- Chưa tách microservices
- Chưa làm collaboration realtime
- Chưa làm AI features
- Chưa làm notification delivery thực
- Chưa làm reporting/analytics nâng cao
- Chưa làm board/timeline nâng cao
- Chưa làm recurrence phức tạp
- Chưa làm enterprise permission model phức tạp

---

## 4. Scope & Paths

**Files / folders cần ưu tiên đọc trước**
- kickoff-prompt.foundation.md
- kickoff-prompt.notes.md
- kickoff-prompt.tasks.md
- docs/product/
- docs/architecture/
- README.md

**Files / folders không được đụng vào**
- None ở pha planning MVP, nhưng phải tôn trọng module boundaries và tránh cam kết scope vượt MVP

**Target paths nếu có sửa**
- docs/product/mvp/
- docs/product/backlog/
- docs/architecture/
- src/backend/
- src/frontend/
- src/mobile/

---

## 5. Requirements

**Business / functional requirements**
- [REQ-01] Người dùng có thể đăng nhập hoặc dùng cơ chế xác thực tối thiểu để sở hữu dữ liệu cá nhân
- [REQ-02] Người dùng có thể tạo, xem, sửa, xóa hoặc archive Notes cơ bản
- [REQ-03] Người dùng có thể tạo, xem, sửa, xóa hoặc archive Tasks cơ bản
- [REQ-04] Người dùng có thể đổi trạng thái task hoàn thành / chưa hoàn thành hoặc status flow cơ bản
- [REQ-05] Người dùng có thể xem danh sách Notes và Tasks với filter/sort/pagination tối thiểu
- [REQ-06] Web app phải hỗ trợ full flow MVP
- [REQ-07] Mobile app phải hỗ trợ các flow cốt lõi của MVP
- [REQ-08] Thiết kế phải sẵn sàng để mở rộng liên kết Note-Task ở phase kế tiếp, nhưng chỉ làm mức tối thiểu nếu thật sự cần cho MVP
- [REQ-09] MVP phải đủ tốt để demo nội bộ, pilot nhỏ hoặc validate với nhóm người dùng đầu tiên

**Technical requirements**
- [TECH-01] Phải có solution skeleton rõ cho backend/web/mobile
- [TECH-02] Contracts phải được chốt trước implementation phần chính
- [TECH-03] Module boundaries phải rõ và có cơ chế giữ boundary
- [TECH-04] API phải ổn định đủ cho cả web và mobile cùng dùng
- [TECH-05] Có test strategy tối thiểu cho backend và contracts
- [TECH-06] Có cấu trúc docs đủ để team PO/BA/DEV/QC/Designer cùng bám theo
- [TECH-07] Build/run local phải đơn giản đủ cho team khởi động nhanh
- [TECH-08] Kiến trúc phải không khóa đường mở rộng plugin/module sau MVP

**Security / performance / compliance requirements**
- [SEC-01] Xác thực và ownership phải được áp dụng ngay từ MVP
- [SEC-02] Validate input tại boundary
- [SEC-03] Không expose dữ liệu ngoài phạm vi người dùng
- [PERF-01] List APIs phải có pagination
- [PERF-02] Search/filter mức cơ bản phải đủ dùng với dữ liệu thực tế ban đầu
- [PERF-03] Thiết kế query tránh N+1 ở các luồng chính
- [PERF-04] Response contracts phải gọn, dễ dùng cho mobile/web

---

## 6. Contracts & Data

**API / contract impacted**
- Auth contract mức tối thiểu cho MVP
- Notes CRUD APIs
- Tasks CRUD APIs
- Task status update API
- Shared paging/filter/result/error contracts
- Versioning/convention rules cho public APIs

**Data model impacted**
- User hoặc owner reference tối thiểu
- Notes entity/table
- Tasks entity/table
- Audit fields
- Archive/soft delete policy nếu áp dụng
- No advanced relationship requirement bắt buộc cho MVP nếu chưa thực sự cần

**Compatibility constraints**
- Không đổi contract tùy tiện sau khi web/mobile bắt đầu bám vào
- Nếu thay đổi enum/status/shape response phải cập nhật docs và impact list
- MVP phải ưu tiên backward compatibility sớm để tránh rework
- Những gì không chắc cho v1 thì defer, không thiết kế quá mức

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
- MVP foundation đủ để một user tạo được một Note và một Task hợp lệ qua contracts ổn định, với ownership đúng và không phá vỡ boundaries giữa các modules

---

## 8. References

**Docs / issue / note liên quan**
- kickoff-prompt.foundation.md
- kickoff-prompt.notes.md
- kickoff-prompt.tasks.md
- README.md
- docs/architecture/*
- docs/product/*

**Context thêm nếu cần**
- CEO/PO direction: release đầu tiên phải đủ nhỏ để đi nhanh nhưng đủ chất lượng để làm nền cho sản phẩm lâu dài
- Ưu tiên tính rõ ràng, dễ delivery, dễ demo, dễ validate với người dùng đầu tiên
- Tránh over-engineering ở MVP nhưng không đánh đổi module boundaries và maintainability

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
Bắt đầu từ kickoff-prompt.mvp-v1.md
```

Hoặc nếu muốn rõ hơn:

```text
Đọc kickoff-prompt.mvp-v1.md và thực hiện theo đó.
```

---

## 11. Notes for Claude

Khi đọc file này, Claude nên:
1. Tóm tắt lại hiểu biết từ file.
2. Xác định mode: discovery / planning / implementation / review.
3. Chốt rõ Must-have / Should-have / Not-now cho release đầu tiên.
4. Nếu thông tin đã đủ rõ thì chuyển sang backlog phase 1 và implementation planning.
5. Chỉ hỏi lại khi file còn thiếu dữ liệu quan trọng hoặc có mâu thuẫn lớn.
6. Luôn ưu tiên: phạm vi nhỏ, giá trị rõ, delivery nhanh, boundaries sạch, contracts ổn định, và khả năng demo/pilot sớm.

---

## 12. MVP Scope Suggestion

**Must-have**
- Auth/ownership tối thiểu
- Notes CRUD cơ bản
- Tasks CRUD cơ bản
- Task status update
- List + pagination + filter cơ bản
- Web UI cho các luồng chính
- Mobile UI cho các luồng chính
- Backend contracts ổn định
- Unit/integration/contract tests tối thiểu cho các luồng chính

**Should-have**
- Archive thay vì hard delete
- Basic tagging
- Search cơ bản theo title/keyword
- Liên kết task với note ở mức đơn giản
- Shared empty/loading/error states rõ ràng ở UI

**Not-now**
- Collaboration
- Reminder delivery thực
- Push notifications
- Realtime sync
- Recurrence phức tạp
- Activity log nâng cao
- Kanban/board nâng cao
- AI assistant/features
- Plugin marketplace
