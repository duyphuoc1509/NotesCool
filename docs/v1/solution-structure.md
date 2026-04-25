# SOLUTION STRUCTURE

> Mục tiêu: định nghĩa cấu trúc solution/repository để team có thể bắt đầu implementation mà không bị lẫn trách nhiệm giữa backend, web, mobile và docs.

---

## 1. Structure Goals

Cấu trúc solution cần đạt:
- Dễ đọc
- Dễ onboarding
- Tách rõ boundaries
- Hỗ trợ phát triển song song backend/web/mobile
- Dễ mở rộng Notes/Tasks về sau
- Không over-engineer ở giai đoạn MVP

---

## 2. Monorepo Recommendation

Khuyến nghị dùng **monorepo** cho giai đoạn đầu vì:
- Dễ đồng bộ contracts giữa backend/web/mobile
- Dễ quản lý docs và backlog
- Tăng tốc collaboration ở pha đầu
- Phù hợp với modular monolith first

---

## 3. Top-Level Repository Layout

```text
/
├── README.md
├── docs/
├── src/
├── tests/
├── scripts/
├── .github/            # hoặc CI folder tương đương
├── docker/
└── .env.example
```

---

## 4. Docs Structure

```text
docs/
├── product/
│   ├── mvp/
│   ├── backlog/
│   └── decisions/
├── architecture/
│   ├── modules/
│   ├── api/
│   └── diagrams/
├── delivery/
└── runbooks/
```

### Gợi ý tài liệu chính
- `docs/product/mvp/mvp-scope.md`
- `docs/product/backlog/backlog.foundation.md`
- `docs/architecture/module-map.md`
- `docs/architecture/api/api-conventions.md`
- `docs/delivery/delivery-plan.phase-1.md`

---

## 5. Source Structure

```text
src/
├── backend/
├── frontend/
└── mobile/
```

---

## 6. Backend Structure

```text
src/backend/
├── host/
├── shared/
└── modules/
    ├── notes/
    └── tasks/
```

### 6.1 Host

**Vai trò**
- Startup/composition root
- DI registration
- Middleware pipeline
- HTTP host
- Auth setup
- Configuration loading

### 6.2 Shared

**Vai trò**
- Shared kernel tối giản
- Common abstractions
- Result/error contracts
- Paging/filtering contracts
- User/time context abstractions

### 6.3 Modules

Mỗi module nên có cấu trúc gần giống nhau:

```text
src/backend/modules/notes/
├── domain/
├── application/
├── infrastructure/
├── api/
└── contracts/
```

Tương tự cho `tasks/`.

### Layer responsibilities

**domain/**
- entity
- value object
- domain rules
- domain services nếu cần

**application/**
- use cases
- command/query handlers
- interfaces/ports
- orchestration logic

**infrastructure/**
- EF Core mappings
- repository implementations
- db-related adapters
- external integrations của module

**api/**
- controllers/endpoints
- request/response mapping
- validation at boundary

**contracts/**
- DTOs public/internal module contracts nếu team chọn tách riêng

---

## 7. Frontend Web Structure

```text
src/frontend/
├── app/
├── features/
│   ├── notes/
│   └── tasks/
├── shared/
└── tests/
```

### app/
- app shell
- routing
- providers
- bootstrapping

### features/notes
- pages
- components
- hooks
- api client bindings
- feature state

### features/tasks
- pages
- components
- hooks
- api client bindings
- feature state

### shared/
- design primitives
- reusable ui
- shared utilities
- api base client
- common types for frontend

### Rule
- Feature không import internals của feature khác nếu không cần
- Shared không chứa business rules riêng của Notes/Tasks

---

## 8. Mobile Structure

```text
src/mobile/
├── app/
├── features/
│   ├── notes/
│   └── tasks/
├── shared/
└── tests/
```

### app/
- navigation root
- providers
- bootstrapping

### features/
- màn hình theo module
- hooks/state
- api bindings
- feature-level components

### shared/
- ui primitives
- API base client
- auth/session helpers
- common utilities

### Rule
- Cấu trúc mobile nên mirror tư duy frontend web để giảm cognitive load cho team

---

## 9. Tests Structure

```text
tests/
├── backend/
│   ├── unit/
│   ├── integration/
│   └── architecture/
├── frontend/
└── mobile/
```

### backend/unit
- domain/application rules

### backend/integration
- API integration tests
- persistence integration tests nếu cần

### backend/architecture
- dependency direction checks
- module boundary checks

### frontend
- component/unit tests mức cần thiết
- feature smoke tests nếu có

### mobile
- component/unit tests mức cần thiết
- flow smoke tests nếu có

---

## 10. Scripts Structure

```text
scripts/
├── setup/
├── dev/
├── test/
└── ci/
```

### Example scripts
- local setup
- start backend/web/mobile
- run test suites
- seed dev data
- format/lint/build wrappers

---

## 11. Docker / Environment Structure

```text
docker/
├── backend/
├── db/
└── local-compose/
```

### Root environment files
- `.env.example`
- `.env.local` (không commit nếu chứa secrets)

---

## 12. Naming Conventions

### Backend
- Projects/folders theo module ownership
- DTO suffix rõ: `CreateNoteRequest`, `TaskResponse`
- Use case names rõ: `CreateTask`, `UpdateTaskStatus`

### Frontend/Mobile
- Feature folders theo business module
- Components theo screen/domain meaning, không quá generic
- API files đặt gần feature hoặc shared API client layer theo rule thống nhất

### Docs
- Dùng kebab-case cho markdown files
- Prefix phase/module nếu cần:
  - `delivery-plan.phase-1.md`
  - `backlog.foundation.md`

---

## 13. Suggested Initial File Tree

```text
/
├── README.md
├── docs/
│   ├── architecture/
│   │   ├── module-map.md
│   │   └── api/
│   │       └── api-conventions.md
│   ├── product/
│   │   ├── mvp/
│   │   │   └── mvp-scope.md
│   │   └── backlog/
│   │       └── backlog.foundation.md
│   └── delivery/
│       └── delivery-plan.phase-1.md
├── src/
│   ├── backend/
│   │   ├── host/
│   │   ├── shared/
│   │   └── modules/
│   │       ├── notes/
│   │       └── tasks/
│   ├── frontend/
│   │   ├── app/
│   │   ├── features/
│   │   │   ├── notes/
│   │   │   └── tasks/
│   │   └── shared/
│   └── mobile/
│       ├── app/
│       ├── features/
│       │   ├── notes/
│       │   └── tasks/
│       └── shared/
├── tests/
│   ├── backend/
│   │   ├── unit/
│   │   ├── integration/
│   │   └── architecture/
│   ├── frontend/
│   └── mobile/
└── scripts/
```

---

## 14. Guardrails

- Không bỏ business logic vào host/app shell
- Không đưa logic module-specific vào shared
- Không để frontend/mobile phụ thuộc backend internals
- Không để tests lẫn tầng/trách nhiệm
- Không thêm folder “misc”, “common” mơ hồ nếu chưa định nghĩa rõ

---

## 15. Decision Summary

- Monorepo cho phase đầu
- Backend: host + shared + modules
- Frontend/mobile: app + features + shared
- Tests tách theo loại
- Docs nằm cùng repo để điều hành delivery
- Cấu trúc tối ưu cho modular monolith first và MVP execution
