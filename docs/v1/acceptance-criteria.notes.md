# ACCEPTANCE CRITERIA — NOTES MODULE

> Mục tiêu: định nghĩa acceptance criteria rõ ràng cho Notes module để PO, DEV, QC, Designer cùng dùng khi triển khai và review.

---

## 1. Scope

Tài liệu này áp dụng cho MVP của Notes module:
- create note
- list notes
- view note detail
- update note
- delete/archive note theo policy
- search/filter cơ bản
- web flow
- mobile flow

---

## 2. General Acceptance Rules

Một item của Notes chỉ được xem là done khi:
- đúng functional scope
- đúng API contract
- đúng ownership/security expectations
- không vi phạm module boundaries
- có trạng thái loading/empty/error tối thiểu ở UI nếu liên quan
- docs/test liên quan đã được cập nhật nếu cần

---

## 3. Notes Create

### AC-N-001 Create note successfully
**Given**
- user đã xác thực hợp lệ

**When**
- user gửi request tạo note với dữ liệu hợp lệ

**Then**
- hệ thống tạo note thành công
- note thuộc đúng owner hiện tại
- response đúng contract
- createdAt và updatedAt được thiết lập hợp lệ

### AC-N-002 Validation on create
**Given**
- user gửi request tạo note thiếu dữ liệu bắt buộc hoặc sai format

**When**
- request được xử lý

**Then**
- hệ thống trả lỗi validation theo format chuẩn
- không tạo note mới
- không lộ internal error details

---

## 4. Notes List

### AC-N-003 List notes for current user only
**Given**
- hệ thống có notes của nhiều user

**When**
- user hiện tại gọi API/list screen

**Then**
- chỉ notes thuộc user hiện tại được hiển thị
- không lộ notes của user khác

### AC-N-004 Pagination works
**Given**
- user có nhiều notes hơn page size

**When**
- user xem danh sách notes

**Then**
- hệ thống trả dữ liệu có phân trang đúng
- metadata paging đúng contract

### AC-N-005 Search/filter works
**Given**
- user có nhiều notes với title/content khác nhau

**When**
- user tìm kiếm theo keyword hoặc filter cơ bản đã chốt

**Then**
- kết quả phản ánh đúng điều kiện tìm kiếm
- không trả dữ liệu ngoài phạm vi owner

---

## 5. Note Detail

### AC-N-006 View note detail
**Given**
- user có một note hợp lệ

**When**
- user mở chi tiết note đó

**Then**
- hệ thống trả đúng nội dung note theo contract
- dữ liệu hiển thị nhất quán giữa API và UI

### AC-N-007 Access denied to other user's note
**Given**
- tồn tại note thuộc user khác

**When**
- user cố truy cập note đó

**Then**
- hệ thống chặn truy cập theo policy
- không lộ dữ liệu note ngoài phạm vi user

---

## 6. Note Update

### AC-N-008 Update note successfully
**Given**
- user có một note của mình

**When**
- user cập nhật dữ liệu hợp lệ

**Then**
- note được cập nhật đúng
- updatedAt thay đổi hợp lệ
- response đúng contract

### AC-N-009 Invalid update rejected
**Given**
- user gửi cập nhật không hợp lệ

**When**
- request được xử lý

**Then**
- hệ thống trả lỗi validation/domain rule đúng format
- không ghi dữ liệu sai

---

## 7. Note Delete / Archive

### AC-N-010 Delete or archive note according to policy
**Given**
- user có một note hợp lệ

**When**
- user chọn xóa hoặc archive

**Then**
- hệ thống xử lý đúng theo policy sản phẩm đã chốt
- hành vi nhất quán giữa API, web, mobile
- list/detail phản ánh trạng thái mới đúng cách

### AC-N-011 Cannot modify removed/archived note incorrectly
**Given**
- note đã bị archive hoặc delete theo policy

**When**
- user thực hiện hành động không được phép tiếp theo

**Then**
- hệ thống phản hồi đúng theo policy
- không tạo state mâu thuẫn

---

## 8. Web Acceptance Criteria

### AC-N-012 Notes list page on web
- Hiển thị được danh sách notes
- Có loading state
- Có empty state khi không có dữ liệu
- Có error state khi API lỗi
- Có điều hướng rõ đến create/detail/edit flow

### AC-N-013 Note editor on web
- User có thể tạo note mới
- User có thể sửa note hiện có
- Save/update feedback rõ ràng
- Không mất dữ liệu bất ngờ trong các flow cơ bản đã chốt

### AC-N-014 Search/filter on web
- Search/filter hoạt động theo API contract
- UI phản hồi đủ nhanh và dễ hiểu cho MVP

### AC-N-015 Delete/archive on web
- Action rõ ràng
- Kết quả sau action phản ánh đúng trên danh sách và detail nếu có

---

## 9. Mobile Acceptance Criteria

### AC-N-016 Notes list screen on mobile
- Hiển thị được danh sách notes
- Có loading/empty/error state cơ bản
- Trải nghiệm đọc list đủ rõ trên màn hình di động

### AC-N-017 Note editor/detail on mobile
- Tạo note được
- Sửa note được
- Điều hướng giữa list và detail ổn định

### AC-N-018 Delete/archive on mobile
- Action hoạt động đúng policy
- Hành vi nhất quán với web/backend
- UX đủ đơn giản cho MVP

---

## 10. Security & Quality Acceptance Criteria

### AC-N-019 Ownership enforced
- User không thể đọc/sửa/xóa note của user khác

### AC-N-020 Contract consistency
- API response đúng với `api-conventions.md`
- JSON fields đúng naming convention
- Error response đúng format

### AC-N-021 Module boundaries preserved
- Notes implementation không phụ thuộc trực tiếp internals của Tasks

### AC-N-022 Tests baseline
- Có unit tests cho rules chính
- Có integration tests cho các API chính
- Có ít nhất một case ownership/security

---

## 11. Definition of Done for Notes Stories

Một story của Notes chỉ được close khi:
- Acceptance criteria tương ứng đều pass
- Demo được nếu là user-facing flow
- Test liên quan pass
- Không có known issue nghiêm trọng bị bỏ qua mà không ghi nhận
