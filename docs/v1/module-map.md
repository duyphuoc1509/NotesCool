# MODULE MAP

> Mục tiêu: định nghĩa ranh giới kiến trúc, dependency rules, và cách các module giao tiếp với nhau trong dự án Notes - Tasks.

---

## 1. Purpose

Tài liệu này mô tả:
- Các module cốt lõi của hệ thống
- Trách nhiệm của từng module
- Quy tắc dependency giữa các module
- Cách giao tiếp nội bộ giữa modules
- Những gì được phép và không được phép

---

## 2. Architecture Style

- Modular monolith first
- Plugin-ready
- Contract-first
- Domain ownership rõ ràng theo module
- Có thể tách service về sau nếu sản phẩm phát triển, nhưng hiện tại ưu tiên đơn giản và boundary sạch

---

## 3. Core Modules

## 3.1 Shared Kernel

**Vai trò**
- Chứa primitives, shared abstractions, common result/error model, paging/filtering contracts, audit interfaces, common utilities thật sự dùng chung

**Được phép chứa**
- Base types
- Common enums thật sự generic
- Result/Error contracts
- Paging contracts
- Authentication abstractions
- Time/user context interfaces
- Domain event abstraction nếu cần

**Không được chứa**
- Business rules riêng của Notes
- Business rules riêng của Tasks
- Logic “dùng chung” nhưng thực chất chỉ thuộc một module
- Persistence logic đặc thù

---

## 3.2 Notes Module

**Vai trò**
- Quản lý lifecycle của Notes
- Chịu trách nhiệm toàn bộ domain logic liên quan đến Notes
- Cung cấp contracts/API cho note CRUD và các luồng note-related

**Sở hữu**
- Note entity
- Note business rules
- Note repository abstraction
- Note application services
- Note API contracts
- Note persistence mappings

**Không nên làm**
- Chứa logic trực tiếp của Tasks
- Gọi internals của Tasks module
- Tự quyết định cross-module flow không qua contract

---

## 3.3 Tasks Module

**Vai trò**
- Quản lý lifecycle của Tasks
- Chịu trách nhiệm toàn bộ domain logic liên quan đến Tasks
- Cung cấp contracts/API cho task CRUD, status update, filtering

**Sở hữu**
- Task entity
- Task status/priority rules
- Task repository abstraction
- Task application services
- Task API contracts
- Task persistence mappings

**Không nên làm**
- Chứa logic trực tiếp của Notes
- Gọi internals của Notes module
- Đọc thẳng persistence model của Notes

---

## 3.4 Host / API Composition Layer

**Vai trò**
- Khởi động ứng dụng
- Đăng ký modules
- Compose DI/container
- Expose HTTP endpoints
- Áp cross-cutting concerns như auth, logging, middleware, exception handling

**Không nên làm**
- Chứa business logic
- Chứa domain rules
- Chứa logic đặc thù chỉ thuộc Notes hoặc Tasks

---

## 3.5 Frontend Web App

**Vai trò**
- Hiển thị UI cho Notes và Tasks
- Gọi API contracts đã được chốt
- Quản lý state presentation
- Tổ chức theo feature boundaries

**Feature boundaries**
- `features/notes`
- `features/tasks`
- `shared`

**Không nên làm**
- Encode business rules phức tạp thay backend
- Dùng data shape nội bộ không qua public contract
- Cross-feature coupling không cần thiết

---

## 3.6 Mobile App

**Vai trò**
- Cung cấp trải nghiệm di động cho các use case MVP
- Dùng cùng public contracts với web/backend
- Tổ chức theo feature boundaries tương tự web

**Không nên làm**
- Diverge contract tùy ý so với web
- Encode domain logic phức tạp thay backend
- Tạo flow không đồng bộ với product scope đã chốt

---

## 4. Dependency Rules

## Allowed dependencies

- Host -> Shared
- Host -> Notes
- Host -> Tasks
- Notes -> Shared
- Tasks -> Shared
- Frontend Notes feature -> frontend shared
- Frontend Tasks feature -> frontend shared
- Mobile Notes feature -> mobile shared
- Mobile Tasks feature -> mobile shared

## Forbidden dependencies

- Notes -> Tasks implementation
- Tasks -> Notes implementation
- Shared -> Notes
- Shared -> Tasks
- Frontend Notes feature -> backend internals
- Frontend Tasks feature -> backend internals
- Mobile Notes feature -> backend internals
- Mobile Tasks feature -> backend internals

---

## 5. Cross-Module Communication

Nếu Notes và Tasks cần giao tiếp:
- ưu tiên public contract / application interface
- hoặc event-driven internal contract nếu cần loose coupling
- không gọi trực tiếp persistence layer của nhau
- không chia sẻ internal entity giữa modules

**Ví dụ đúng**
- Tasks lưu `LinkedNoteId` như external reference
- Tasks gọi note existence check qua abstraction/contract
- Notes publish event khi bị archive nếu Tasks cần phản ứng

**Ví dụ sai**
- Tasks import trực tiếp NoteDbContext
- Notes đọc trực tiếp Task table bằng internal repository của Tasks
- Shared kernel chứa NoteTaskBusinessService

---

## 6. Plugin Direction

Về lâu dài, hệ thống hướng tới plugin/module-safe.

**Extension points có thể có sau này**
- Search providers
- Reminder providers
- Tagging extensions
- Export/import extensions
- Notification handlers

**Rule**
- Plugin chỉ đi qua abstraction/contracts
- Không được inject vào internals của module khác
- Không làm rò rỉ implementation details

---

## 7. Suggested Folder Structure

```text
src/
  backend/
    host/
    shared/
    modules/
      notes/
      tasks/
  frontend/
    app/
    features/
      notes/
      tasks/
    shared/
  mobile/
    app/
    features/
      notes/
      tasks/
    shared/
tests/
  backend/
    unit/
    integration/
    architecture/
  frontend/
  mobile/
```

---

## 8. Architecture Guardrails

Để giữ boundary sạch, cần có:
- Architecture tests hoặc rule checks
- Review checklist về dependency
- PR template có mục impact modules
- Không merge nếu tạo coupling sai

Checklist review:
- Có import sai chiều module không?
- Có đưa business rule vào shared sai chỗ không?
- Có để UI phụ thuộc persistence/internal DTO không?
- Có tạo contract ngoài scope module không?

---

## 9. Decision Summary

- Modular monolith first
- Notes và Tasks độc lập
- Shared kernel tối giản
- Giao tiếp qua contract
- UI theo feature boundaries
- Plugin-ready nhưng không over-engineer ở MVP
