# ACCEPTANCE CRITERIA — TASKS MODULE

> Mục tiêu: định nghĩa acceptance criteria rõ ràng cho Tasks module để PO, DEV, QC, Designer cùng dùng khi triển khai và review.

---

## 1. Scope

Tài liệu này áp dụng cho MVP của Tasks module:
- create task
- list tasks
- view task detail
- update task
- update task status
- delete/archive task theo policy
- filter/sort cơ bản
- web flow
- mobile flow

---

## 2. General Acceptance Rules

Một item của Tasks chỉ được xem là done khi:
- đúng functional scope
- đúng API contract
- đúng ownership/security expectations
- không vi phạm module boundaries
- UI có loading/empty/error states tối thiểu nếu liên quan
- docs/test liên quan đã được cập nhật nếu cần

---

## 3. Task Create

### AC-T-001 Create task successfully
**Given**
- user đã xác thực hợp lệ

**When**
- user gửi request tạo task với dữ liệu hợp lệ

**Then**
- hệ thống tạo task thành công
- task thuộc đúng owner hiện tại
- response đúng contract
- createdAt và updatedAt được thiết lập hợp lệ

### AC-T-002 Validation on create
**Given**
- user gửi request tạo task sai format hoặc thiếu dữ liệu bắt buộc

**When**
- request được xử lý

**Then**
- hệ thống trả lỗi validation theo format chuẩn
- không tạo task mới
- không lộ internal error details

---

## 4. Tasks List

### AC-T-003 List tasks for current user only
**Given**
- hệ thống có tasks của nhiều user

**When**
- user hiện tại gọi API/list screen

**Then**
- chỉ tasks thuộc user hiện tại được hiển thị
- không lộ tasks của user khác

### AC-T-004 Pagination works
**Given**
- user có nhiều tasks hơn page size

**When**
- user xem danh sách tasks

**Then**
- hệ thống trả dữ liệu phân trang đúng
- metadata paging đúng contract

### AC-T-005 Filter/sort works
**Given**
- user có nhiều tasks với status/priority/due date khác nhau

**When**
- user lọc hoặc sắp xếp theo các tiêu chí đã chốt

**Then**
- kết quả phản ánh đúng điều kiện filter/sort
- không trả dữ liệu ngoài phạm vi owner

---

## 5. Task Detail

### AC-T-006 View task detail
**Given**
- user có một task hợp lệ

**When**
- user mở chi tiết task đó

**Then**
- hệ thống trả đúng dữ liệu task theo contract
- dữ liệu hiển thị nhất quán giữa API và UI

### AC-T-007 Access denied to other user's task
**Given**
- tồn tại task thuộc user khác

**When**
- user cố truy cập task đó

**Then**
- hệ thống chặn truy cập theo policy
- không lộ dữ liệu ngoài phạm vi user

---

## 6. Task Update

### AC-T-008 Update task successfully
**Given**
- user có một task của mình

**When**
- user cập nhật dữ liệu hợp lệ

**Then**
- task được cập nhật đúng
- updatedAt thay đổi hợp lệ
- response đúng contract

### AC-T-009 Invalid update rejected
**Given**
- user gửi cập nhật không hợp lệ

**When**
- request được xử lý

**Then**
- hệ thống trả lỗi validation/domain rule đúng format
- không ghi dữ liệu sai

---

## 7. Task Status Update

### AC-T-010 Update task status successfully
**Given**
- user có một task hợp lệ

**When**
- user đổi trạng thái task theo contract được hỗ trợ

**Then**
- trạng thái task được cập nhật đúng
- response đúng contract
- UI phản ánh trạng thái mới nhất quán

### AC-T-011 Invalid status transition rejected
**Given**
- hệ thống có rule cho status transition không hợp lệ

**When**
- user gửi yêu cầu đổi trạng thái vi phạm rule

**Then**
- hệ thống từ chối với lỗi đúng format
- không cập nhật state sai

---

## 8. Task Delete / Archive

### AC-T-012 Delete or archive task according to policy
**Given**
- user có một task hợp lệ

**When**
- user chọn xóa hoặc archive

**Then**
- hệ thống xử lý đúng policy sản phẩm đã chốt
- hành vi nhất quán giữa API, web, mobile
- danh sách và detail phản ánh đúng trạng thái mới

### AC-T-013 Cannot modify removed/archived task incorrectly
**Given**
- task đã bị archive hoặc delete theo policy

**When**
- user thực hiện hành động không được phép tiếp theo

**Then**
- hệ thống phản hồi đúng theo policy
- không tạo state mâu thuẫn

---

## 9. Web Acceptance Criteria

### AC-T-014 Tasks list page on web
- Hiển thị được danh sách tasks
- Có loading state
- Có empty state
- Có error state
- Có filter/sort/status actions cơ bản
- Điều hướng rõ đến detail/edit flow

### AC-T-015 Task editor/detail on web
- User có thể tạo task mới
- User có thể sửa task hiện có
- User có thể đổi trạng thái task
- Save/update feedback rõ ràng

### AC-T-016 Filter/sort/status on web
- Filter/sort/status flow hoạt động đúng contract
- UX đủ rõ cho MVP

### AC-T-017 Delete/archive on web
- Action rõ ràng
- Kết quả sau action phản ánh đúng trên list/detail

---

## 10. Mobile Acceptance Criteria

### AC-T-018 Tasks list screen on mobile
- Hiển thị được danh sách tasks
- Có loading/empty/error state cơ bản
- Trải nghiệm đọc list đủ rõ trên màn hình di động

### AC-T-019 Task editor/detail on mobile
- Tạo task được
- Sửa task được
- Đổi trạng thái task được
- Điều hướng ổn định

### AC-T-020 Filter/status/delete or archive on mobile
- Hỗ trợ use case cốt lõi của task management
- Hành vi nhất quán với web/backend
- UX đủ đơn giản cho MVP

---

## 11. Security & Quality Acceptance Criteria

### AC-T-021 Ownership enforced
- User không thể đọc/sửa/xóa task của user khác

### AC-T-022 Contract consistency
- API response đúng với `api-conventions.md`
- JSON fields đúng naming convention
- Error response đúng format

### AC-T-023 Module boundaries preserved
- Tasks implementation không phụ thuộc trực tiếp internals của Notes

### AC-T-024 Tests baseline
- Có unit tests cho rules chính
- Có integration tests cho các API chính
- Có ít nhất một case ownership/security
- Có test hoặc check cho status flow chính

---

## 12. Definition of Done for Tasks Stories

Một story của Tasks chỉ được close khi:
- Acceptance criteria tương ứng đều pass
- Demo được nếu là user-facing flow
- Test liên quan pass
- Không có known issue nghiêm trọng bị bỏ qua mà không ghi nhận
