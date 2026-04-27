# BA-006 — FE API Integration Requirement (React)

**Issue:** OSI-61  
**Related GitHub Issues:** [#44](https://github.com/duyphuoc1509/NotesCool/issues/44), [#45](https://github.com/duyphuoc1509/NotesCool/issues/45), [#46](https://github.com/duyphuoc1509/NotesCool/issues/46), [#47](https://github.com/duyphuoc1509/NotesCool/issues/47), [#48](https://github.com/duyphuoc1509/NotesCool/issues/48)  
**Multica Tasks:** OSI-1, OSI-2, OSI-3, OSI-4, OSI-5  
**Assignee:** FE 1 (React)  
**Status:** In Review

---

## 1. Overview

Tài liệu này phân tích và mô tả yêu cầu tích hợp toàn bộ API từ backend .NET vào frontend React (NotesCool).

Frontend hiện có cấu trúc cơ bản tại `src/frontend/` nhưng chưa có service layer thực tế. Cần xây dựng lớp API layer tập trung, tích hợp auth flow và CRUD cho Notes + Tasks.

---

## 2. Business Goal

- Người dùng có thể đăng ký, đăng nhập, đăng xuất trên UI.
- Người dùng có thể tạo, xem, sửa, archive Notes.
- Người dùng có thể tạo, xem, sửa, đổi trạng thái, archive Tasks.
- Token JWT được quản lý tự động (attach vào request, tự refresh khi hết hạn).
- UI phản hồi đúng với loading state, error state, empty state.

---

## 3. Actors

- **Guest** — chưa đăng nhập, chỉ truy cập được `/login`, `/register`.
- **Authenticated User** — đã login, có access token hợp lệ.

---

## 4. API Endpoint Reference (Backend .NET)

### 4.1 Auth APIs

> Base: `/api/auth` | Không cần Bearer token

| Method | Endpoint | Request Body | Response |
|--------|----------|--------------|----------|
| POST | `/api/auth/register` | `{ email, password }` | 201 `{ id, email, status }` \| 400 validation \| 409 email exists |
| POST | `/api/auth/login` | `{ email, password }` | 200 `{ accessToken, refreshToken, tokenType, expiresIn }` |
| POST | `/api/auth/refresh` | `{ refreshToken }` | 200 `{ accessToken, refreshToken, tokenType, expiresIn }` \| 401 |
| POST | `/api/auth/logout` | `{ refreshToken }` | 204 NoContent \| 401 |

**AuthTokenResponse fields:**
- `accessToken: string` — JWT Bearer token
- `refreshToken: string`
- `tokenType: string` — `"Bearer"`
- `expiresIn: number` — giây (900 = 15 phút)

**RegisterResponse fields:**
- `id: string` (GUID)
- `email: string`
- `status: string` — `"active"`

---

### 4.2 Notes APIs

> Base: `/api/notes` | Yêu cầu Bearer token

| Method | Endpoint | Query/Body | Response |
|--------|----------|-----------|----------|
| GET | `/api/notes` | `?query=&page=1&pageSize=20` | 200 `PagedResult<NoteResponse>` |
| GET | `/api/notes/{id}` | — | 200 `NoteResponse` \| 401 \| 404 |
| POST | `/api/notes` | `{ title, content }` | 201 `NoteResponse` \| 400 \| 401 |
| PUT | `/api/notes/{id}` | `{ title, content }` | 200 `NoteResponse` \| 400 \| 401 \| 404 |
| DELETE | `/api/notes/{id}` | — | 204 NoContent (archive) \| 401 \| 404 |

**NoteResponse fields:**
- `id: string` (GUID)
- `title: string` — max 200 chars
- `content: string`
- `createdAt: string` (ISO 8601)
- `updatedAt: string | null`

**Lưu ý:** `title` và `content` đều là required khi Create và Update.

---

### 4.3 Tasks APIs

> Base: `/api/tasks` | Yêu cầu Bearer token

| Method | Endpoint | Query/Body | Response |
|--------|----------|-----------|----------|
| GET | `/api/tasks` | `?status=&page=1&pageSize=10` | 200 `PagedResult<TaskDto>` |
| GET | `/api/tasks/{id}` | — | 200 `TaskDto` \| 401 \| 404 |
| POST | `/api/tasks` | `{ title, description?, dueDate? }` | 201 `TaskDto` \| 400 \| 401 |
| PUT | `/api/tasks/{id}` | `{ title, description?, dueDate? }` | 200 `TaskDto` \| 400 \| 401 \| 404 |
| PATCH | `/api/tasks/{id}/status` | `{ status }` | 200 `TaskDto` \| 400 \| 401 \| 404 |
| DELETE | `/api/tasks/{id}` | — | 204 NoContent \| 401 \| 404 |

**TaskDto fields:**
- `id: string` (GUID)
- `title: string` — max 200 chars
- `description: string | null`
- `status: "todo" | "in_progress" | "done" | "archived"`
- `dueDate: string | null` (ISO 8601)
- `createdAt: string`
- `updatedAt: string | null`

**TaskStatus enum:** `todo`, `in_progress`, `done`, `archived`

**Lưu ý:** `status` field khi filter: `?status=todo` hoặc bỏ trống để lấy tất cả.

---

### 4.4 PagedResult<T> structure

```json
{
  "items": [...],
  "page": 1,
  "pageSize": 10,
  "total": 42
}
```

---

## 5. Task Breakdown

### OSI-1 — [FE] Setup API Layer

**GitHub Issue:** https://github.com/duyphuoc1509/NotesCool/issues/44

Tạo nền tảng API layer cho toàn bộ frontend.

**Files cần tạo/sửa:**

```
src/services/api.ts          # axios instance + interceptors
src/services/authService.ts  # auth API functions
src/services/notesService.ts # notes API functions
src/services/tasksService.ts # tasks API functions
src/services/index.ts        # re-export tất cả services
src/types/auth.ts            # TypeScript types cho auth
src/types/note.ts            # TypeScript types cho note
src/types/task.ts            # TypeScript types cho task
src/types/common.ts          # PagedResult<T>, ApiError, etc.
```

**axios instance cần:**
- baseURL từ `import.meta.env.VITE_API_URL`
- `Content-Type: application/json`
- Request interceptor: đính `Authorization: Bearer <accessToken>`
- Response interceptor: nếu 401 → gọi `/api/auth/refresh` → retry request gốc → nếu refresh fail → redirect `/login`

---

### OSI-2 — [FE] Integrate Auth APIs

**GitHub Issue:** https://github.com/duyphuoc1509/NotesCool/issues/45

Tích hợp auth vào UI Register/Login/Logout.

**Luồng Register:**
1. User điền email + password → submit
2. Call `POST /api/auth/register`
3. OK (201) → redirect về `/login` hoặc auto-login
4. 400 → hiển thị lỗi validation theo field
5. 409 → `"Email này đã được sử dụng"`

**Luồng Login:**
1. User điền email + password → submit
2. Call `POST /api/auth/login`
3. OK (200) → lưu `accessToken` (memory/sessionStorage), `refreshToken` (localStorage) → redirect `/`
4. 401 → `"Email hoặc mật khẩu không đúng"`
5. 400 → lỗi validation

**Luồng Logout:**
1. User click Logout
2. Call `POST /api/auth/logout` với refreshToken
3. Xóa cả accessToken và refreshToken khỏi storage
4. Redirect `/login`

**Auth state:**
- Sử dụng React Context hoặc Zustand store
- `isAuthenticated: boolean`, `user: AuthUserResponse | null`, `isLoading: boolean`
- Protected routes: redirect unauthenticated → `/login`
- Redirect authenticated users khỏi `/login`, `/register` → `/`

---

### OSI-3 — [FE] Integrate Notes APIs

**GitHub Issue:** https://github.com/duyphuoc1509/NotesCool/issues/46

Tích hợp Notes CRUD + search + pagination.

**Luồng:**
- Tải danh sách: `GET /api/notes?page=1&pageSize=20`
- Khi search: debounce 300ms → `GET /api/notes?query=<keyword>&page=1&pageSize=20`
- Tạo note: form với `title` (required) + `content` (required) → `POST /api/notes`
- Sửa note: form pre-filled → `PUT /api/notes/{id}`
- Archive note: confirm → `DELETE /api/notes/{id}` → ẩn khỏi list

**UI states cần:**
- Skeleton loading khi tải list
- Empty state: `"Chưa có ghi chú nào. Tạo ghi chú đầu tiên!"`
- Error state: `"Không thể tải ghi chú. Thử lại"`
- Pagination controls: Previous/Next hoặc số trang

---

### OSI-4 — [FE] Integrate Tasks APIs

**GitHub Issue:** https://github.com/duyphuoc1509/NotesCool/issues/47

Tích hợp Tasks CRUD + status change + filter.

**Luồng:**
- Tải danh sách: `GET /api/tasks?page=1&pageSize=10`
- Filter theo status: Tab "All" / "Todo" / "In Progress" / "Done" → `?status=todo|in_progress|done`
- Tạo task: `title` (required), `description` (optional), `dueDate` (optional) → `POST /api/tasks`
- Đổi status: select/button → `PATCH /api/tasks/{id}/status` với `{ status: "..." }`
- Sửa task: form → `PUT /api/tasks/{id}`
- Archive: `PATCH /api/tasks/{id}/status` với `{ status: "archived" }` hoặc `DELETE /api/tasks/{id}`

**UI states cần:**
- Skeleton loading
- Empty state theo tab
- Filter tabs

---

### OSI-5 — [FE] API Error Handling

**GitHub Issue:** https://github.com/duyphuoc1509/NotesCool/issues/48

Xử lý lỗi API tập trung.

**Error map:**

| HTTP Status | Xử lý |
|-------------|--------|
| 400 | Hiển thị lỗi theo field nếu có `details`, ngược lại toast error |
| 401 | Thử refresh → retry. Nếu refresh fail → redirect `/login` |
| 403 | Toast "Không có quyền thực hiện thao tác này" |
| 404 | Toast "Không tìm thấy tài nguyên" |
| 409 | Toast "Dữ liệu đã tồn tại" |
| 500+ | Toast "Lỗi hệ thống. Vui lòng thử lại" + nút Retry |
| Network error | Toast "Không thể kết nối. Kiểm tra mạng" |

---

## 6. Validation Rules

| Field | Rule | Error |
|-------|------|-------|
| email | required, valid email | "Email không hợp lệ" |
| password | required, min 8 chars | "Mật khẩu tối thiểu 8 ký tự" |
| note title | required, max 200 chars | "Tiêu đề không được để trống" / "Tiêu đề không quá 200 ký tự" |
| note content | required | "Nội dung không được để trống" |
| task title | required, max 200 chars | "Tiêu đề không được để trống" / "Tiêu đề không quá 200 ký tự" |
| task status | valid enum value | "Trạng thái không hợp lệ" |

---

## 7. Acceptance Criteria

### Auth

```
AC-AUTH-001
Given: User chưa đăng nhập
When: Truy cập route protected
Then: Redirect về /login

AC-AUTH-002
Given: User điền đúng email và password
When: Submit form login
Then: Redirect về trang chủ, token được lưu

AC-AUTH-003
Given: User điền sai password
When: Submit form login
Then: Hiển thị "Email hoặc mật khẩu không đúng"

AC-AUTH-004
Given: User đã login, access token hết hạn (401)
When: Gọi bất kỳ API protected nào
Then: Hệ thống tự refresh token và retry request, không logout người dùng

AC-AUTH-005
Given: Cả access token và refresh token đều hết hạn
When: Gọi API protected
Then: Redirect về /login

AC-AUTH-006
Given: User click Logout
When: Confirm logout
Then: Xóa token, redirect /login
```

### Notes

```
AC-NOTE-001
Given: User đã login
When: Vào trang Notes
Then: Danh sách notes hiển thị với skeleton khi loading, danh sách khi xong

AC-NOTE-002
Given: User nhập keyword vào search box
When: Sau 300ms debounce
Then: Danh sách lọc theo keyword

AC-NOTE-003
Given: User tạo note với title + content
When: Submit form
Then: Note mới xuất hiện đầu danh sách

AC-NOTE-004
Given: User archive một note
When: Confirm archive
Then: Note biến mất khỏi danh sách (vì archived filter mặc định tắt)
```

### Tasks

```
AC-TASK-001
Given: User đã login
When: Vào trang Tasks
Then: Danh sách tasks hiển thị

AC-TASK-002
Given: User chọn filter "In Progress"
When: Click tab
Then: Chỉ hiển thị tasks có status = in_progress

AC-TASK-003
Given: User tạo task mới
When: Submit form (chỉ cần title)
Then: Task mới có status = todo, xuất hiện trong list

AC-TASK-004
Given: User đổi status task từ todo sang in_progress
When: Click/select new status
Then: Task cập nhật ngay trên UI, không cần reload

AC-TASK-005
Given: User archive task
When: Confirm archive
Then: Task ẩn khỏi list (filter mặc định không show archived)
```

---

## 8. Open Questions

> Các điểm cần BE xác nhận:

1. **Auth endpoints thực tế**: Hiện có 2 file auth endpoint (`AuthEndpoints.cs` - dev mode cho phép login với bất kỳ email/password, và `AuthEndpointExtensions.cs` - register thực). Đâu là endpoint sẽ dùng cho production? Login endpoint có nằm ở `/api/auth/login` không?

2. **Login response fields**: `AuthEndpoints.cs` trả về `AuthTokenResponse` (accessToken, refreshToken, tokenType, expiresIn), nhưng `AuthDtos.cs` có `LoginResponse` (accessToken, tokenType, expiresIn, user). Endpoint login thực sự trả về object nào?

3. **NoteResponse có `archived` field không?** Hiện tại `NoteResponse` không có trường `archived`, FE có cần filter client-side không?

4. **Task status string values**: Enum `TaskStatus` có các values gì (`todo`, `in_progress`, `done`, `archived` hay `Todo`, `InProgress`, `Done`, `Archived`)? Serialize thành camelCase hay PascalCase?

---

## 9. Notes for Dev (FE)

- API base URL: `VITE_API_URL` (xem `.env.example`)
- Dùng `axios` (đã có trong `package.json`)
- Không dùng `any` cho TypeScript types
- AccessToken lifetime: 15 phút (900 giây)
- RefreshToken cần lưu persistent (localStorage) để survive page reload
- AccessToken nên lưu in-memory hoặc sessionStorage
- Base path của Notes endpoints là `/api/notes` (không có `/v1/`)
- `DELETE /api/notes/{id}` là soft archive, không phải hard delete
- `DELETE /api/tasks/{id}` là hard delete theo BE code, khác với Notes

---

## 10. Notes for QC

Sau khi FE tích hợp xong:

**Test cases cần cover:**
- Login với email/password đúng → thành công
- Login với password sai → lỗi
- Register với email đã tồn tại → lỗi 409
- CRUD Notes đầy đủ
- CRUD Tasks đầy đủ
- Đổi Task status: todo → in_progress → done → archived
- Search Notes
- Filter Tasks theo status
- Token refresh: mock API trả 401 → interceptor tự refresh
- Logout → redirect và không thể truy cập protected routes
- Empty state khi danh sách rỗng
- Loading state khi API chậm
