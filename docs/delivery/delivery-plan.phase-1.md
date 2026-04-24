# DELIVERY PLAN — PHASE 1

> Mục tiêu: kế hoạch thực thi Phase 1 để chuyển từ planning sang implementation readiness cho dự án Notes - Tasks.

---

## 1. Phase 1 Goal

Phase 1 tập trung vào:

- Dựng nền kỹ thuật và delivery foundation
- Khóa các quyết định quan trọng để team không đi sai hướng
- Chuẩn bị đủ điều kiện để bắt đầu implementation Notes và Tasks một cách kiểm soát được

Phase 1 **chưa** nhằm hoàn tất toàn bộ MVP feature.

---

## 2. Phase 1 Outcomes

Kết thúc Phase 1, team phải có:

- Repo structure rõ ràng
- Docs nền đủ dùng
- Backend skeleton chạy được
- Frontend skeleton chạy được
- Mobile skeleton chạy được
- API conventions đã chốt
- Module boundaries đã rõ
- Auth/ownership baseline đã có quyết định
- Test baseline đã dựng xong
- Backlog Notes/Tasks sẵn sàng cho phase tiếp theo

---

## 3. Workstreams

### 3.1 Product & Architecture

**Owner đề xuất:** CEO/PO + Tech Lead + BA

**Deliverables:**

- MVP scope chốt
- Module map chốt
- API conventions chốt
- Solution structure chốt

### 3.2 Backend Foundation

**Owner đề xuất:** Backend Lead + Backend Dev

**Deliverables:**

- .NET 8 host skeleton (ASP.NET Core Web API)
- Shared kernel baseline
- Notes module skeleton
- Tasks module skeleton
- Middleware / error handling base
- DB baseline (PostgreSQL + EF Core)
- Auth baseline (JWT Bearer + Refresh Token direction)

### 3.3 Frontend Web Foundation

**Owner đề xuất:** Frontend Lead + Frontend Dev + Designer

**Deliverables:**

- React app shell
- Routing / layout base
- Feature folder structure
- API client baseline
- Shared UI / state patterns

### 3.4 Mobile Foundation

**Owner đề xuất:** Mobile Dev + Designer

**Deliverables:**

- React Native app shell
- Navigation base
- Feature folder structure
- API / auth baseline
- Shared mobile utilities

### 3.5 Quality & Delivery

**Owner đề xuất:** QC + Tech Lead + Devs

**Deliverables:**

- Unit test baseline (xUnit/NUnit cho backend, React testing stack cho web)
- Integration test baseline
- Architecture tests baseline
- CI baseline
- Runbook / getting started docs

---

## 4. Recommended Sequence

### Step 1 — Lock decisions

**Hoạt động:**

- Review `kickoff-prompt.mvp-v1.md`
- Review `module-map.md`
- Review `mvp-scope.md`
- Review `api-conventions.md`
- Review `solution-structure.md`

**Exit criteria:**

- Không còn ambiguity lớn về scope phase đầu
- Team thống nhất boundaries và conventions

### Step 2 — Create repo / skeleton

**Hoạt động:**

- Tạo monorepo structure theo `solution-structure.md`
- Dựng backend skeleton (.NET 8 Web API host)
- Dựng frontend skeleton (React + TypeScript)
- Dựng mobile skeleton (React Native)
- Dựng docs folders và tests folders

**Exit criteria:**

- Repo chạy được ở mức skeleton
- Team có thể clone và khởi động môi trường cơ bản

### Step 3 — Implement technical baseline

**Hoạt động:**

- Middleware / error handling
- Shared contracts (DTO conventions, error response contract, pagination contract)
- Auth baseline (JWT + Refresh Token)
- DB baseline (EF Core + PostgreSQL setup)
- API client baseline (cho frontend / mobile)

**Exit criteria:**

- Có nền để Notes/Tasks bắt đầu cắm vào
- Không phải quay lại sửa foundation liên tục

### Step 4 — Add quality gates

**Hoạt động:**

- Unit test harness
- Integration test baseline
- Architecture checks (module dependency direction enforcement)
- CI pipeline cơ bản

**Exit criteria:**

- Build / test tự động chạy được
- Dependency sai chiều có cơ chế phát hiện

### Step 5 — Prepare feature execution

**Hoạt động:**

- Chuyển Notes / Tasks thành backlog thực thi
- Ước lượng effort
- Chốt Phase 2 kickoff

**Exit criteria:**

- Team sẵn sàng bắt đầu implementation feature

---

## 5. Suggested Timeline

### Week 1

| Ngày | Hoạt động |
|------|-----------|
| Day 1–2 | Lock product / architecture decisions (review tất cả tài liệu nền) |
| Day 3 | Setup repo structure + backend skeleton |
| Day 4 | Setup frontend skeleton |
| Day 5 | Setup mobile skeleton |

### Week 2

| Ngày | Hoạt động |
|------|-----------|
| Day 1 | Add shared contracts + API conventions vào codebase / docs |
| Day 2 | Setup DB baseline (EF Core + PostgreSQL) |
| Day 3 | Setup auth / ownership baseline (JWT Bearer + Refresh Token) |
| Day 4 | Setup API client baseline (frontend + mobile) |
| Day 5 | Review integration points + fix issues |

### Week 3

| Ngày | Hoạt động |
|------|-----------|
| Day 1–2 | Add unit test + integration test baseline |
| Day 3 | Add architecture tests + CI baseline |
| Day 4 | Polish runbook / getting started docs |
| Day 5 | Review readiness for Notes / Tasks implementation |

> Nếu team nhỏ, có thể gộp linh hoạt.
> Nếu team lớn hơn, có thể chạy song song theo workstream.

---

## 6. Team Cadence Recommendation

### Weekly cadence

- **Monday:** plan tuần + chốt scope
- **Mid-week:** architecture / dev sync + blocker review
- **Friday:** demo progress + retro + adjust backlog

### Daily cadence

- Standup rất ngắn (< 15 phút)
- Focus vào blockers, dependency, decision cần escalation

---

## 7. Role Expectations

### CEO / PO

- Chốt scope
- Chống lan scope
- Ra quyết định ưu tiên
- Duy trì nhịp delivery

### BA

- Viết acceptance criteria
- Làm rõ use cases
- Bóc tách Notes / Tasks backlog

### Tech Lead

- Chốt technical choices
- Giữ module boundaries
- Review conventions và implementation direction

### Backend Dev

- Dựng host / shared / modules foundation
- Chốt API / public contracts với team

### Frontend Dev

- Dựng web shell
- Bám API conventions
- Tổ chức feature boundaries rõ

### Mobile Dev

- Dựng mobile shell
- Bám contracts như web
- Tối ưu flow cốt lõi cho mobile

### QC

- Xây checklist smoke
- Xác định test coverage ưu tiên
- Review acceptance criteria từ sớm

### Designer

- Chốt flow và wireframe MVP
- Giữ consistency web / mobile
- Không mở rộng scope visual quá sớm

---

## 8. Risks & Mitigations

### Risk 1 — Scope creep

**Mitigation:**

- Bám `mvp-scope.md`
- Mọi feature mới phải qua PO decision
- Phase 1 chỉ focus foundation, không thêm business features

### Risk 2 — Coupling sai giữa modules

**Mitigation:**

- Bám `module-map.md`
- Có architecture tests enforce dependency direction
- Review checklist theo dependency rules
- Notes module không truy cập trực tiếp internals Tasks module và ngược lại

### Risk 3 — Web / mobile diverge contracts

**Mitigation:**

- Dùng chung `api-conventions.md`
- Không tạo response shape riêng cho từng client
- Shared contracts phải giữ minimal và stable

### Risk 4 — Over-engineering từ quá sớm

**Mitigation:**

- Phase 1 chỉ làm foundation đủ dùng
- Plugin-ready nhưng chưa xây plugin system hoàn chỉnh
- YAGNI: chỉ implement what's needed now

### Risk 5 — Team bắt đầu feature khi foundation chưa ổn

**Mitigation:**

- Có exit criteria Phase 1 rõ ràng
- Không vào Phase 2 nếu skeleton / quality gates chưa đủ
- Mandatory review trước khi chuyển phase

---

## 9. Phase 1 Exit Criteria

Phase 1 hoàn tất khi tất cả các tiêu chí sau được thỏa mãn:

| # | Tiêu chí | Verification |
|---|----------|-------------|
| 1 | Tài liệu nền đã chốt | Review bởi PO / Tech Lead |
| 2 | Repo structure rõ ràng | Team có thể clone và navigate |
| 3 | Backend skeleton chạy được | `dotnet build` + `dotnet run` pass |
| 4 | Frontend skeleton chạy được | `npm run build` + `npm run dev` pass |
| 5 | Mobile skeleton chạy được | Build + run trên emulator pass |
| 6 | API conventions rõ | Documented + áp dụng trong skeleton |
| 7 | Shared contracts baseline rõ | DTO, error, paging contracts đã define |
| 8 | Auth / ownership baseline rõ | JWT + Refresh Token flow documented + baseline code |
| 9 | DB baseline rõ | EF Core + PostgreSQL connection + migration setup |
| 10 | Unit / integration / architecture test baseline rõ | Test projects created, sample tests pass |
| 11 | CI baseline chạy được | Build + test pipeline chạy tự động |
| 12 | Notes / Tasks backlog Phase 2 sẵn sàng | Backlog items đã tạo + ước lượng effort |

---

## 10. Immediate Next Actions

1. **Review và chốt tài liệu nền:**
   - `api-conventions.md`
   - `solution-structure.md`
   - `delivery-plan.phase-1.md` (tài liệu này)
   - `module-map.md`
   - `mvp-scope.md`

2. **Tạo repo structure:**
   - Backend: `src/backend/` — .NET 8 solution
   - Frontend: `src/frontend/` — React + TypeScript
   - Mobile: `src/mobile/` — React Native
   - Docs: `docs/` — architecture, conventions, backlog, product
   - Tests: `tests/` — unit, integration, architecture

3. **Tạo backlog implementation:**
   - Backend skeleton tasks
   - Frontend skeleton tasks
   - Mobile skeleton tasks
   - Shared contracts tasks
   - Auth baseline tasks
   - DB baseline tasks
   - Test baseline tasks
   - CI baseline tasks

4. **Kickoff Phase 1 implementation**

---

## 11. Backend Foundation — Detailed Breakdown

> Phần này mô tả chi tiết các deliverables backend cần hoàn thành trong Phase 1.

### 11.1 Solution Structure

```
src/backend/
├── NotesCool.sln
├── src/
│   ├── NotesCool.Host/                    # ASP.NET Core Web API host
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── appsettings.Development.json
│   ├── NotesCool.Shared/                  # Shared kernel (primitives, abstractions)
│   │   ├── Contracts/                     # Public contracts
│   │   ├── Abstractions/                  # Cross-cutting abstractions
│   │   └── Primitives/                    # Value objects, base entities
│   ├── NotesCool.Modules.Notes/           # Notes business module
│   │   ├── Domain/
│   │   ├── Application/
│   │   ├── Infrastructure/
│   │   └── Presentation/
│   └── NotesCool.Modules.Tasks/           # Tasks business module
│       ├── Domain/
│       ├── Application/
│       ├── Infrastructure/
│       └── Presentation/
└── tests/
    ├── NotesCool.Shared.Tests/
    ├── NotesCool.Modules.Notes.Tests/
    ├── NotesCool.Modules.Tasks.Tests/
    ├── NotesCool.IntegrationTests/
    └── NotesCool.ArchitectureTests/
```

### 11.2 Shared Contracts Baseline

- `ApiResponse<T>` — envelope response cho tất cả endpoints
- `PagedRequest` / `PagedResponse<T>` — pagination contract
- `ErrorResponse` — error envelope (code, message, details)
- `SortRequest` — sorting contract
- `FilterRequest` — filtering contract (nếu cần ở mức baseline)

### 11.3 Middleware / Error Handling

- Global exception handler middleware
- Request validation middleware
- Correlation ID middleware
- Request / response logging (structured logging)

### 11.4 Auth Baseline

- JWT Bearer authentication setup
- Refresh token direction (documented, chưa cần full implementation)
- Ownership baseline: resource ownership check abstraction
- `[Authorize]` attribute + policy-based auth direction

### 11.5 DB Baseline

- EF Core DbContext configuration
- PostgreSQL provider setup
- Migration workflow documented
- Base entity conventions: `Id`, `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy`
- Soft delete policy direction (documented)
- Module-level DbContext separation hoặc shared DbContext với schema separation

---

## 12. Suggested Command After Review

Sau khi review xong bộ tài liệu Phase 1, có thể bắt đầu bằng:

```text
Đọc kickoff-prompt.foundation.md, module-map.md, api-conventions.md,
solution-structure.md và delivery-plan.phase-1.md.
Sau đó lập kế hoạch implementation Phase 1 theo backlog.foundation.md.
```

---

## Appendix A — Document References

| Tài liệu | Mục đích | Trạng thái |
|-----------|----------|------------|
| `kickoff-prompt.mvp-v1.md` | MVP kickoff prompt | Cần review |
| `module-map.md` | Module boundaries và dependency rules | Cần review |
| `mvp-scope.md` | MVP scope definition | Cần review |
| `api-conventions.md` | REST API conventions | Cần review |
| `solution-structure.md` | Solution / folder structure | Cần review |
| `delivery-plan.phase-1.md` | Tài liệu này | Draft |
| `backlog.foundation.md` | Foundation backlog items | Cần tạo |

---

## Appendix B — Tech Stack Summary

| Layer | Technology | Version |
|-------|-----------|---------|
| Backend | .NET / ASP.NET Core | 8.0 |
| Frontend | React + TypeScript | Latest stable |
| Mobile | React Native | Latest stable |
| Database | PostgreSQL | Latest stable |
| ORM | Entity Framework Core | 8.x |
| Auth | JWT Bearer + Refresh Token | — |
| Testing (BE) | xUnit / NUnit | Latest |
| Testing (FE) | React Testing Library + Jest / Vitest | Latest |
| API Style | REST-first | — |
| Architecture | Modular Monolith | — |
