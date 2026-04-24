# API CONVENTIONS

> Mục tiêu: chuẩn hóa cách thiết kế API cho dự án Notes - Tasks để backend, web, mobile, QC và tài liệu cùng bám theo một chuẩn thống nhất.

---

## 1. Goals

Tài liệu này chuẩn hóa:
- Cách đặt tên endpoints
- HTTP semantics
- Request/response shape
- Error handling
- Validation response
- Pagination/filtering/sorting
- Versioning guidelines
- Auth/ownership expectations

---

## 2. General Principles

- REST-first
- Contract-first
- Tên rõ ràng, dễ đoán
- Ưu tiên consistency hơn cleverness
- Web và mobile dùng cùng public contracts
- Không expose internal model trực tiếp
- Không tạo endpoint đặc thù UI nếu chưa thật sự cần
- Breaking change phải được kiểm soát và ghi nhận rõ

---

## 3. Base URL & Versioning

**Khuyến nghị**
```text
/api/v1
```

**Ví dụ**
```text
/api/v1/notes
/api/v1/tasks
```

### Rule
- Phiên bản public API nằm trên URL path
- MVP dùng `v1`
- Không đổi version nếu chỉ thêm field optional hoặc thêm endpoint mới không phá compatibility
- Nếu có breaking change đáng kể, phải đánh giá nâng version

---

## 4. Resource Naming

### Rule
- Dùng noun số nhiều cho collections
- Không dùng verb trong endpoint nếu CRUD chuẩn đã đủ biểu đạt
- Chỉ dùng sub-resource hoặc action endpoint khi thực sự cần

### Ví dụ đúng
```text
GET    /api/v1/notes
POST   /api/v1/notes
GET    /api/v1/notes/{id}
PATCH  /api/v1/notes/{id}
DELETE /api/v1/notes/{id}

GET    /api/v1/tasks
POST   /api/v1/tasks
GET    /api/v1/tasks/{id}
PATCH  /api/v1/tasks/{id}
DELETE /api/v1/tasks/{id}
PATCH  /api/v1/tasks/{id}/status
```

### Ví dụ tránh dùng
```text
POST /createNote
POST /updateTask
GET  /getAllTasks
```

---

## 5. HTTP Method Semantics

### GET
- Chỉ để đọc dữ liệu
- Không có side effects

### POST
- Tạo resource mới
- Có thể dùng cho action không thuần CRUD nếu cần, nhưng phải hạn chế

### PATCH
- Update một phần resource
- Khuyến nghị dùng cho phần lớn update trong MVP

### PUT
- Chỉ dùng nếu thực sự là full replace
- Với MVP, ưu tiên PATCH để tránh ambiguity

### DELETE
- Xóa resource hoặc soft delete/archive theo policy
- Nếu dùng archive thay vì xóa thực, phải document rõ behavior

---

## 6. Standard Resource Shapes

## 6.1 Note DTO

```json
{
  "id": "note_123",
  "title": "Sprint ideas",
  "content": "Draft scope for MVP",
  "tags": ["mvp", "planning"],
  "archived": false,
  "createdAt": "2026-04-22T10:00:00Z",
  "updatedAt": "2026-04-22T12:30:00Z"
}
```

## 6.2 Task DTO

```json
{
  "id": "task_123",
  "title": "Prepare module map",
  "description": "Draft module boundaries",
  "status": "todo",
  "priority": "medium",
  "dueDate": "2026-04-25T00:00:00Z",
  "linkedNoteId": "note_123",
  "archived": false,
  "createdAt": "2026-04-22T10:00:00Z",
  "updatedAt": "2026-04-22T12:30:00Z"
}
```

### Rule
- Public DTO chỉ chứa field cần cho clients
- Không trả về internal persistence flags hoặc technical-only columns nếu client không cần
- Tên field dùng `camelCase` cho JSON

---

## 7. Request Conventions

### Create request
- Chỉ chứa các field client được phép nhập
- Không nhận audit fields từ client
- Không nhận ownership fields từ client nếu ownership lấy từ auth context

### Update request
- PATCH request chỉ chứa field cần sửa
- Có validation rõ ràng cho fields optional/nullability

### Ví dụ create note
```json
{
  "title": "Sprint ideas",
  "content": "Draft scope for MVP",
  "tags": ["mvp", "planning"]
}
```

### Ví dụ patch task
```json
{
  "title": "Prepare module map",
  "priority": "high",
  "dueDate": "2026-04-25T00:00:00Z"
}
```

---

## 8. Response Conventions

### 8.1 Success responses

MVP chuẩn hóa success response có envelope top-level `data`.

**Detail / create / update**
```json
{
  "data": {
    "id": "note_123",
    "title": "Sprint ideas"
  }
}
```

**List có phân trang**
```json
{
  "data": {
    "items": [],
    "page": {
      "number": 1,
      "size": 20,
      "totalItems": 100,
      "totalPages": 5
    }
  }
}
```

**GET detail**
- Trả về 200 OK + envelope `data`

**POST create**
- Trả về 201 Created
- Có thể trả resource DTO mới tạo trong envelope `data`
- Nên có Location header nếu thuận tiện

**PATCH**
- Trả về 200 OK + resource DTO mới nhất trong envelope `data`
- Hoặc 204 No Content nếu team thống nhất, nhưng với MVP khuyến nghị trả DTO để client dễ dùng hơn

**DELETE**
- Trả về 204 No Content hoặc 200 nếu cần body mô tả
- Ưu tiên nhất quán toàn hệ thống

### Khuyến nghị MVP
- Detail/list/create/update trả object rõ ràng trong `data`
- Delete dùng 204
- Error luôn theo cùng format
- Paging dùng `items` + `page`

---

## 9. Error Response Format

Khuyến nghị thống nhất một format như sau:

```json
{
  "error": {
    "code": "validation_error",
    "message": "Request validation failed",
    "details": [
      {
        "field": "title",
        "message": "Title is required"
      }
    ],
    "traceId": "00-abc123"
  }
}
```

### Required fields
- `code`: mã lỗi ổn định cho client xử lý
- `message`: thông điệp dễ hiểu
- `details`: danh sách lỗi con nếu có
- `traceId`: hỗ trợ debug/logging

### Error code examples
- `validation_error`
- `unauthorized`
- `forbidden`
- `not_found`
- `conflict`
- `domain_rule_violation`
- `internal_error`

---

## 10. Validation Rules

### Rule
- Validate ở API boundary cho shape/cấu trúc/input cơ bản
- Validate ở application/domain cho business rules
- Validation errors trả về theo format thống nhất
- Không leak stack trace/internal exception details

### Examples
- `title` required
- `title` max length
- `status` must be one of allowed values
- `dueDate` phải hợp lệ theo định dạng hệ thống
- `linkedNoteId` nếu có thì phải đúng format/contract rule

---

## 11. Pagination / Filtering / Sorting

### Query conventions
Khuyến nghị:
```text
?page=1&pageSize=20&search=abc&sortBy=updatedAt&sortOrder=desc
```

### Common parameters
- `page`
- `pageSize`
- `search`
- `sortBy`
- `sortOrder`

### Module-specific filters
**Notes**
- `archived`
- `tag`

**Tasks**
- `status`
- `priority`
- `dueFrom`
- `dueTo`
- `archived`

### Paginated response shape

```json
{
  "data": {
    "items": [],
    "page": {
      "number": 1,
      "size": 20,
      "totalItems": 100,
      "totalPages": 5
    }
  }
}
```

### Rule
- Default pagination bắt buộc cho list endpoints
- `pageSize` phải có upper limit để tránh abuse
- Sorting fields phải whitelist, không cho arbitrary raw DB field injection

---

## 12. Auth & Ownership Rules

### Rule
- User identity lấy từ auth context, không lấy từ request body
- Mọi resource thuộc phạm vi ownership rõ ràng
- User A không thấy/sửa/xóa dữ liệu của User B
- Nếu chưa có permission model phức tạp, MVP chỉ cần owner-based access control

### Expected status codes
- 401 nếu chưa xác thực
- 403 nếu đã xác thực nhưng không có quyền
- 404 nếu resource không tồn tại hoặc hệ thống chọn che giấu existence theo policy

---

## 13. Idempotency & Consistency

- GET phải idempotent
- PATCH/DELETE nên xử lý nhất quán khi resource không tồn tại
- Không được tạo behavior “lúc thì 200 lúc thì 204” cho cùng một action nếu không có lý do rõ
- Status update endpoint phải có contract ổn định

---

## 14. Status & Enum Design

### Task status
Khuyến nghị MVP:
- `todo`
- `in_progress`
- `done`

### Task priority
Khuyến nghị MVP:
- `low`
- `medium`
- `high`

### Rule
- Enum values dùng string
- Tránh dùng integer enum trong public API
- Nếu đổi enum phải đánh giá compatibility impact

---

## 15. Module Endpoint Draft

## Notes
```text
POST   /api/v1/notes
GET    /api/v1/notes
GET    /api/v1/notes/{id}
PATCH  /api/v1/notes/{id}
DELETE /api/v1/notes/{id}
```

## Tasks
```text
POST   /api/v1/tasks
GET    /api/v1/tasks
GET    /api/v1/tasks/{id}
PATCH  /api/v1/tasks/{id}
PATCH  /api/v1/tasks/{id}/status
DELETE /api/v1/tasks/{id}
```

---

## 16. Documentation Requirements

Mỗi endpoint nên được document ít nhất:
- purpose
- request sample
- response sample
- validation rules
- error codes
- ownership/security expectations

---

## 17. Change Management Rules

Trước khi đổi public contract:
- xác định có breaking change không
- cập nhật docs
- đánh giá impact lên web/mobile
- cập nhật tests liên quan
- ghi rõ trong changelog/PR description

---

## 18. Decision Summary

- Base URL: `/api/v1`
- REST-first
- JSON `camelCase`
- Error format thống nhất
- Pagination bắt buộc cho list endpoints
- Ownership lấy từ auth context
- Web/mobile dùng cùng contracts
- Breaking changes phải kiểm soát
