# KICKOFF PROMPT

> Mục tiêu: chỉ cần điền file này một lần cho mỗi đợt làm việc.
>
> Khi bắt đầu session, chỉ cần nhắn: **"Bắt đầu từ `kickoff-prompt.foundation.md`"**.
>
> Agent sẽ đọc file này đầu tiên, rồi mới tiếp tục discovery / planning / implementation.

---

## 1. Task Summary

**Tên task / initiative**
- Foundation architecture setup cho dự án Notes - Tasks

**Mục tiêu chính**
- Thiết lập nền tảng kỹ thuật và nguyên tắc kiến trúc cho toàn bộ hệ sản phẩm Notes - Tasks theo hướng modular monolith first, plugin-ready, contract-first; đủ rõ để các vai trò PO, BA, DEV, QC, Designer cùng triển khai nhất quán.

**Kết quả mong muốn**
- Bộ tài liệu foundation architecture
- Project skeleton cho backend / frontend / mobile
- Module map và dependency rules
- Coding conventions và working conventions ban đầu
- Backlog foundation ưu tiên cao
- Definition of Done và quality gates cho giai đoạn khởi tạo

---

## 2. Working Mode

**Loại công việc**
- [x] Discovery
- [x] Planning
- [ ] Implementation
- [ ] Refactor
- [ ] Bug fix
- [ ] Review
- [x] Documentation

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
- API: REST-first
- Auth: JWT + Refresh Token (dự kiến cho baseline)
- Testing: xUnit/NUnit cho backend, React testing stack cho web, test strategy mở rộng dần theo phase

**Architecture direction**
- Modular monolith first
- Plugin-ready nhưng không over-engineering ở phase đầu
- Contract-first giữa modules
- Domain-oriented boundaries
- Shared kernel tối thiểu
- Ưu tiên maintainability, testability, clear ownership, dễ scale team
- Chuẩn bị sẵn đường nâng cấp sang tách service/module độc lập nếu sản phẩm phát triển lớn hơn

**Boundaries cần tôn trọng**
- Notes và Tasks là hai business modules độc lập
- Shared/Common chỉ chứa primitives, abstractions, cross-cutting concerns thật sự dùng chung
- Không cho phép module này truy cập trực tiếp internals/domain internals của module khác
- Cross-module communication đi qua application contracts, integration events hoặc facade abstractions
- Plugin chỉ phụ thuộc vào contracts/abstractions, không phụ thuộc implementation chi tiết
- UI không được chứa business rules lõi nếu rule đó thuộc domain/application layer
- Không để persistence concern làm rò rỉ vào API contracts

**Non-goals / không làm trong task này**
- Chưa triển khai đầy đủ feature business cho Notes hoặc Tasks
- Chưa tối ưu production infra, CI/CD nâng cao, observability nâng cao
- Chưa tách microservices
- Chưa làm AI/ML features
- Chưa làm collaboration realtime phức tạp
- Chưa làm enterprise-grade permission matrix nhiều tầng
- Chưa tối ưu performance chuyên sâu khi chưa có baseline đo lường

---

## 4. Scope & Paths

**Files / folders cần ưu tiên đọc trước**
- README.md
- docs/
- docs/architecture/
- docs/product/
- src/backend/
- src/frontend/
- src/mobile/
- tests/

**Files / folders không được đụng vào**
- None ở bước foundation, nhưng mọi thay đổi phải giữ đúng module boundaries và không tạo coupling sai hướng

**Target paths nếu có sửa**
- docs/architecture/
- docs/conventions/
- docs/backlog/
- src/backend/
- src/frontend/
- src/mobile/
- tests/

---

## 5. Requirements

**Business / functional requirements**
- [REQ-01] Nền tảng phải hỗ trợ phát triển độc lập Notes module và Tasks module.
- [REQ-02] Kiến trúc phải cho phép mở rộng thêm module/plugin trong tương lai mà không phá vỡ module hiện có.
- [REQ-03] Web app và mobile app phải có thể dùng chung domain capabilities thông qua API/contracts nhất quán.
- [REQ-04] Cách tổ chức dự án phải đủ rõ để PO, BA, DEV, QC, Designer phối hợp hiệu quả.
- [REQ-05] Foundation phải đủ tốt để sau phase khởi tạo có thể bắt đầu implementation feature mà không cần sửa kiến trúc lõi lớn.

**Technical requirements**
- [TECH-01] Backend sử dụng .NET 8.
- [TECH-02] Frontend web sử dụng ReactJS.
- [TECH-03] Mobile app sử dụng React Native.
- [TECH-04] Kiến trúc backend theo modular monolith, module-first.
- [TECH-05] Có rule rõ ràng cho module boundaries, dependency direction và cross-module communication.
- [TECH-06] Có API contracts, DTO conventions, error handling conventions, paging conventions.
- [TECH-07] Có cấu trúc solution/folder thống nhất cho backend, frontend, mobile.
- [TECH-08] Có test strategy tối thiểu cho unit, contract, integration và architecture tests.
- [TECH-09] Có tài liệu mô tả coding conventions, naming, decision log và workflow làm việc.

**Security / performance / compliance requirements**
- [SEC-01] Secure by default cho authentication và authorization baseline.
- [SEC-02] Input validation phải được thực hiện ở boundary phù hợp.
- [SEC-03] Không cho phép module truy cập dữ liệu nội bộ trái phép của module khác.
- [SEC-04] Error response không làm lộ chi tiết nhạy cảm không cần thiết.
- [PERF-01] Các list endpoints tương lai phải thiết kế sẵn cho pagination/filtering/sorting.
- [PERF-02] Tránh N+1 query patterns từ design level.
- [PERF-03] Contracts ổn định, hạn chế breaking changes gây ảnh hưởng web/mobile.
- [COMP-01] Các architectural decisions quan trọng phải được ghi nhận trong docs để team bám theo.

---

## 6. Contracts & Data

**API / contract impacted**
- Baseline REST API conventions
- Error response contract
- Pagination/filter/sort request-response contract
- Authentication contract
- Shared response/result conventions
- Internal module contracts cho Notes và Tasks
- Plugin/module extension contracts (ở mức abstraction ban đầu)

**Data model impacted**
- Chưa chốt đầy đủ business schema ở bước foundation
- Có thể định nghĩa base entity conventions: Id, audit fields, ownership, soft delete policy (nếu áp dụng)
- Chưa làm migration feature-specific sâu ở bước này, nhưng cần định hướng rõ data ownership theo module

**Compatibility constraints**
- Ưu tiên backward compatibility từ sớm ở public API contracts
- Không đổi contract mà không update docs + impact analysis
- Shared contracts phải giữ minimal và stable
- Module mới/plugin mới không được ép sửa sâu module cũ ngoài abstraction point đã định nghĩa

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
- Kiến trúc backend có thể enforce được rule: Notes module không được reference trực tiếp internals của Tasks module và ngược lại; mọi integration phải đi qua contracts/abstractions đã cho phép.

---

## 8. References

**Docs / issue / note liên quan**
- kickoff-prompt.md
- kickoff-prompt.foundation.md
- README.md
- docs/architecture/*
- docs/product/*
- docs/backlog/*

**Context thêm nếu cần**
- Agent đóng nhiều vai trò: PO, BA, DEV, QC, Designer
- Vai trò điều hành tổng thể là CEO cho bộ dự án: ưu tiên chất lượng sản phẩm, hiệu quả delivery, kiến trúc bền vững, module/plugin safety, và khả năng scale team
- Foundation phase phải thiên về đúng hướng hơn là làm nhanh nhưng tạo technical debt sớm

---

## 9. Output Preference

**Bạn muốn Agent trả kết quả theo kiểu nào?**
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
Bắt đầu từ kickoff-prompt.foundation.md
```

Hoặc nếu muốn rõ hơn:

```text
Đọc kickoff-prompt.foundation.md và thực hiện theo đó.
```

---

## 11. Notes for Agent

Khi đọc file này, Agent nên:
1. Tóm tắt lại hiểu biết từ file.
2. Xác định mode hiện tại là discovery + planning + documentation.
3. Bắt đầu từ foundation architecture trước khi đi sâu vào Notes/Tasks implementation.
4. Ưu tiên tạo ra module map, dependency rules, solution structure và backlog foundation.
5. Chỉ hỏi lại khi thiếu dữ liệu quan trọng hoặc có mâu thuẫn lớn.
6. Luôn đánh giá quyết định dưới góc nhìn product quality, maintainability, extensibility, delivery efficiency.
7. Tránh over-engineering; plugin-ready nhưng vẫn thực dụng, triển khai được ngay.

---

> Gợi ý: sau file này, nên tạo thêm `kickoff-prompt.notes.md`, `kickoff-prompt.tasks.md`, `kickoff-prompt.api.md`, `kickoff-prompt.frontend.md` để điều phối từng stream công việc cụ thể.
