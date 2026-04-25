# KICKOFF PROMPT

> Mục tiêu: chỉ cần điền file này một lần cho mỗi đợt làm việc.
>
> Khi bắt đầu session, chỉ cần nhắn: **"Bắt đầu từ `kickoff-prompt.notes.md`"**.
>
> Assistant sẽ đọc file này đầu tiên, rồi mới tiếp tục discovery / planning / implementation.

---

## 1. Task Summary

**Tên task / initiative**
- Thiết kế và triển khai Notes module cho dự án Notes - Tasks

**Mục tiêu chính**
- Xây dựng Notes module như một business module độc lập, rõ domain boundaries, có thể phát triển riêng nhưng vẫn tương thích với foundation architecture của toàn hệ thống.

**Kết quả mong muốn**
- Tài liệu scope và kiến trúc cho Notes module
- Domain model và application flow cho Notes
- API/contracts ở mức implementation-ready
- Skeleton code/backend/frontend/mobile cho Notes nếu bước làm chuyển sang implementation
- Test strategy và definition of done riêng cho Notes module

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
- Notes là một module độc lập trong modular monolith
- Contract-first giữa API layer, application layer và các module khác
- Plugin-ready: mọi mở rộng của Notes phải đi qua abstraction/contracts
- Dễ tách riêng về sau nếu cần mà không phá vỡ core design

**Boundaries cần tôn trọng**
- Notes module không phụ thuộc trực tiếp vào internal implementation của Tasks module
- Shared chỉ chứa primitives, common abstractions, cross-cutting concerns
- Nếu Notes cần tương tác module khác thì chỉ qua contracts hoặc events
- UI không được bypass business rules của Notes bằng cách thao tác trực tiếp persistence assumptions
- Không nhét logic ngoài phạm vi Notes vào Notes module

**Non-goals / không làm trong task này**
- Chưa triển khai AI note summarization
- Chưa triển khai collaboration realtime phức tạp
- Chưa triển khai version history đầy đủ kiểu document editor nâng cao
- Chưa xây rich text editor enterprise-level
- Chưa làm full-text search nâng cao nếu chưa có baseline
- Chưa giải quyết triệt để offline sync phức tạp trong giai đoạn đầu

---

## 4. Scope & Paths

**Files / folders cần ưu tiên đọc trước**
- docs/architecture/
- docs/product/
- src/backend/modules/notes/
- src/frontend/src/modules/notes/
- src/mobile/src/modules/notes/
- src/shared/

**Files / folders không được đụng vào**
- Internal implementation của Tasks module
- Các module ngoài Notes nếu không có impact rõ ràng

**Target paths nếu có sửa**
- docs/modules/notes/
- src/backend/modules/notes/
- src/frontend/src/modules/notes/
- src/mobile/src/modules/notes/
- tests/backend/notes/
- tests/frontend/notes/

---

## 5. Requirements

**Business / functional requirements**
- [REQ-01] Người dùng có thể tạo note mới
- [REQ-02] Người dùng có thể cập nhật nội dung note
- [REQ-03] Người dùng có thể xem danh sách notes của mình
- [REQ-04] Người dùng có thể xem chi tiết một note
- [REQ-05] Người dùng có thể xóa hoặc archive note theo policy được chọn
- [REQ-06] Notes phải hỗ trợ trạng thái cơ bản như active / archived / deleted nếu domain cần
- [REQ-07] Notes phải hỗ trợ metadata cơ bản: title, content, created/updated time, owner
- [REQ-08] Thiết kế phải sẵn sàng cho tagging / pinning / linking trong tương lai mà không phá vỡ core model

**Technical requirements**
- [TECH-01] Notes module có domain model rõ ràng
- [TECH-02] API contracts cho Notes phải ổn định và dễ mở rộng
- [TECH-03] Notes application services không lộ persistence details
- [TECH-04] Query/list APIs phải hỗ trợ pagination
- [TECH-05] Validation phải rõ tại boundary layers
- [TECH-06] Mapping giữa entity và DTO không làm rò rỉ domain internals
- [TECH-07] Có module-level tests cho Notes
- [TECH-08] Frontend và mobile dùng chung contract semantics khi gọi Notes APIs

**Security / performance / compliance requirements**
- [SEC-01] Người dùng chỉ truy cập được notes của chính họ nếu chưa có sharing feature
- [SEC-02] Input phải được validate và sanitize phù hợp
- [SEC-03] Không để lộ note data qua cross-module access trái phép
- [PERF-01] Danh sách notes phải phân trang được
- [PERF-02] Truy vấn chi tiết note phải tối ưu, tránh load dư dữ liệu không cần thiết
- [PERF-03] Thiết kế đủ linh hoạt để bổ sung search/indexing sau này

---

## 6. Contracts & Data

**API / contract impacted**
- Create Note API
- Update Note API
- Get Note Detail API
- Get Notes List API
- Delete/Archive Note API
- Shared DTOs cho paging, error handling, result envelope nếu có
- Internal application contracts cho Notes services

**Data model impacted**
- Notes table/entity
- Audit fields: created_at, updated_at, created_by, updated_by
- Ownership field / user relation
- Trạng thái note nếu dùng soft delete hoặc archive

**Compatibility constraints**
- API cần giữ backward compatibility khi thêm field mới
- Tránh breaking change cho web/mobile contract
- Nếu đổi semantics của note state phải update docs và impact analysis
- Mở rộng tính năng tương lai nên ưu tiên additive change hơn destructive change

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
- Notes module có thể tạo một note hợp lệ qua contract/application boundary mà không phụ thuộc trực tiếp vào implementation của module khác

---

## 8. References

**Docs / issue / note liên quan**
- kickoff-prompt.foundation.md
- kickoff-prompt.notes.md
- docs/architecture/*
- docs/modules/notes/*
- README.md

**Context thêm nếu cần**
- Notes là một capability cốt lõi của sản phẩm
- Thiết kế phải cân bằng giữa simplicity cho MVP và extensibility cho các tính năng như tags, reminders, attachments, sharing trong tương lai
- Ưu tiên chất lượng domain model và consistency giữa backend/web/mobile

---

## 9. Output Preference

**Bạn muốn Assistant trả kết quả theo kiểu nào?**
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
Bắt đầu từ kickoff-prompt.notes.md
```

Hoặc nếu muốn rõ hơn:

```text
Đọc kickoff-prompt.notes.md và thực hiện theo đó.
```

---

## 11. Notes for Assistant

Khi đọc file này, Assistant nên:
1. Tóm tắt lại hiểu biết từ file.
2. Xác định mode: discovery / planning / implementation / review.
3. Ưu tiên bám scope của Notes module, không lan sang module khác nếu chưa cần.
4. Nếu thông tin đã đủ rõ thì bắt đầu luôn.
5. Chỉ hỏi lại khi thiếu dữ liệu quan trọng hoặc có mâu thuẫn lớn.
6. Luôn giữ Notes module clean, extensible, plugin-safe, dễ test và dễ maintain.

---

> Gợi ý: khi bắt đầu implementation cho Notes, có thể tạo thêm các file con như `kickoff-prompt.notes-api.md`, `kickoff-prompt.notes-web.md`, `kickoff-prompt.notes-mobile.md` nếu muốn tách context theo stream công việc.
