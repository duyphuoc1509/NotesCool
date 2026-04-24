# BA-002 — Auth & Ownership Requirement

## 1. Overview

Tài liệu định nghĩa nghiệp vụ xác thực (Authentication) và quyền sở hữu (Ownership) cho NotesCool MVP V1.

## 2. Business Goal

- Đảm bảo người dùng phải đăng nhập hợp lệ để sử dụng hệ thống.
- Đảm bảo dữ liệu (Notes, Tasks) của người dùng được cách ly tuyệt đối, không ai khác truy cập được.

## 3. Actors

- **User**: Cá nhân dùng NotesCool.
- **System**: Xử lý logic, kiểm tra token, kiểm soát access.

## 4. Current Flow

Chưa có. (Dự án bắt đầu từ đầu).

## 5. Proposed Flow

1. User truy cập hệ thống. Nếu chưa có token, chuyển đến Login/Register.
2. User gửi thông tin Register -> Hệ thống lưu, trả JWT.
3. User gửi thông tin Login -> Hệ thống kiểm tra, trả JWT access token & refresh token.
4. User gọi protected API -> Hệ thống trích xuất user ID từ JWT và kiểm tra quyền sở hữu với mọi action.
5. Khi JWT hết hạn, user dùng refresh token lấy JWT mới hoặc login lại.
6. User logout -> Hệ thống hủy/blacklist session (tuỳ thiết kế, hoặc chỉ clear ở client).

## 6. Functional Requirements

### FR-001: User Registration

Hỗ trợ user tạo tài khoản mới bằng Email và Password.

### FR-002: User Login

Hỗ trợ user login bằng Email và Password; trả về JWT access token.

### FR-003: User Logout

Hỗ trợ user kết thúc session hợp lệ.

### FR-004: JWT Access

Mọi API protected đều yêu cầu Access Token hợp lệ; nếu không, trả 401 Unauthorized.

### FR-005: Resource Ownership Filter

Khi GET list (Notes/Tasks), hệ thống chỉ query và trả về records mà owner ID trùng khớp với ID từ token.

### FR-006: Resource Ownership Guard

Khi GET detail, UPDATE, PATCH, DELETE một resource, hệ thống phải kiểm tra owner ID. Nếu resource không tồn tại hoặc thuộc user khác, trả 404 Not Found (để tránh rò rỉ sự tồn tại của dữ liệu) hoặc 403 Forbidden. Đề xuất: 404 cho resource not found hoặc khác owner.

## 7. Business Rules

### BR-001: Email Uniqueness

Mỗi email chỉ được đăng ký một tài khoản.

### BR-002: Token Integrity

Token giả mạo, chỉnh sửa hoặc hết hạn bị coi là không hợp lệ.

### BR-003: Ownership Inferred

Client KHÔNG BAO GIỜ truyền `ownerId` khi tạo Note hoặc Task. Hệ thống phải tự lấy từ auth context.

### BR-004: No Ownership Transfer

User không thể chuyển quyền sở hữu Note/Task cho người khác.

## 8. Validation Rules

| Field | Rule | Error Expectation |
|---|---|---|
| email | Required, format email | Lỗi validation email |
| password | Required, min 6 chars | Lỗi validation password |

## 9. State / Status Rules

- Không áp dụng cho Auth user entity trong MVP (User mặc định Active).

## 10. Permission Rules

| Role | Action | Target | Allowed |
|---|---|---|---|
| Authenticated User | Create | Note/Task | Yes |
| Authenticated User | Read/Update/Delete | Own Note/Task | Yes |
| Authenticated User | Read/Update/Delete | Other User's Note/Task | No |
| Unauthenticated User | Any | Protected API | No |

## 11. Acceptance Criteria

### AC-001: Register successfully

Given User provides valid unique email and password  
When User submits registration  
Then System creates user and returns success auth response.

### AC-002: Login successfully

Given User has an existing account  
When User provides correct email and password  
Then System authenticates and returns access token.

### AC-003: Unauthorized request

Given User is not authenticated  
When User requests a protected API (e.g., /notes)  
Then System returns 401 Unauthorized.

### AC-004: Access denied to other's data

Given User A and User B exist  
When User A requests User B's Note ID directly via API  
Then System denies access without exposing the Note data.

### AC-005: Create data defaults to owner

Given User A is authenticated  
When User A creates a new Task  
Then The Task is owned by User A, without requiring owner ID in request.

## 12. Edge Cases

- User gọi refresh API với token không hợp lệ -> Yêu cầu login lại.
- Dữ liệu bị request bằng ID hợp lệ nhưng của user khác -> Phải giấu sự tồn tại của ID đó với user hiện tại.

## 13. Notes for Dev

- Ownership check phải nằm ở tầng truy xuất (Service hoặc Repository filter).
- KHÔNG tạo endpoint /users/all nếu không có mục đích nội bộ.

## 14. Notes for QC

- Test case phải cover đăng nhập sai, email trùng.
- Test case quan trọng: Dùng Postman login account A, gửi request sửa Note của account B -> Phải lỗi.

## 15. Open Questions

- (None, scope and rules confirmed in BA-001)
