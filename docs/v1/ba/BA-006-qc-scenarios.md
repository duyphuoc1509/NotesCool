# BA-006 — QC Scenario Matrix

## 1. Overview

Ma trận kịch bản kiểm thử (QC Scenario Matrix) nhằm cung cấp các test cases quan trọng cho QC từ góc độ nghiệp vụ của MVP V1.

## 2. Security & Ownership Scenarios (Critical)

| ID | Module | Scenario Description | Type | Expected Result |
|---|---|---|---|---|
| QC-SEC-01 | Auth | Truy cập Notes list không có token | Negative | 401 Unauthorized |
| QC-SEC-02 | Auth | Truy cập Tasks bằng token hết hạn | Negative | 401 Unauthorized |
| QC-SEC-03 | Notes | User A gọi GET note detail của User B bằng ID thực tế | Negative | 404 Not Found (or 403) - không trả data |
| QC-SEC-04 | Tasks | User A dùng PATCH update task của User B | Negative | 404 Not Found (or 403) - không đổi data |
| QC-SEC-05 | Notes | User A gọi DELETE xóa note của User B | Negative | 404 Not Found (or 403) - không xóa |

## 3. Auth Validation & Flow Scenarios

| ID | Module | Scenario Description | Type | Expected Result |
|---|---|---|---|---|
| QC-AUTH-01 | Auth | Đăng ký email hợp lệ, password >6 chars | Positive | User created, trả auth success |
| QC-AUTH-02 | Auth | Đăng ký email đã tồn tại trong DB | Negative | 409 Conflict |
| QC-AUTH-03 | Auth | Đăng nhập sai password | Negative | 401 Unauthorized |
| QC-AUTH-04 | Auth | Đăng nhập đúng email/pass | Positive | Trả token hợp lệ |

## 4. Notes Scenarios

| ID | Module | Scenario Description | Type | Expected Result |
|---|---|---|---|---|
| QC-NOT-01 | Notes | Tạo note đầy đủ title, content | Positive | Note created, thuộc owner |
| QC-NOT-02 | Notes | Tạo note không gửi title | Negative | 400 Validation Error |
| QC-NOT-03 | Notes | Xem list notes mặc định | Positive | Trả các notes active, không có archived notes |
| QC-NOT-04 | Notes | List notes có tổng > size phân trang | Positive | Trả list và metadata phân trang chính xác |
| QC-NOT-05 | Notes | Sửa title và content note | Positive | Note update thành công, updatedAt mới |
| QC-NOT-06 | Notes | Archive/Delete note | Positive | Xóa thành công, không còn hiển thị ở default list |
| QC-NOT-07 | Notes | Search theo keyword có chứa trong title | Positive | Trả notes thỏa mãn, chỉ của owner |
| QC-NOT-08 | Notes | Tạo note có content cực dài (giới hạn thiết kế) | Positive/Neg | Handle thành công hoặc trả validation tùy BE |

## 5. Tasks Scenarios

| ID | Module | Scenario Description | Type | Expected Result |
|---|---|---|---|---|
| QC-TSK-01 | Tasks | Tạo task đầy đủ | Positive | Task created, status: todo |
| QC-TSK-02 | Tasks | Tạo task không gửi title | Negative | 400 Validation Error |
| QC-TSK-03 | Tasks | Xem danh sách default tasks | Positive | Trả các active tasks của owner |
| QC-TSK-04 | Tasks | Filter tasks theo status (VD: done) | Positive | Chỉ trả tasks của owner có status 'done' |
| QC-TSK-05 | Tasks | Update task info (không đổi status) | Positive | Info updated |
| QC-TSK-06 | Tasks | Đổi trạng thái từ todo sang in_progress | Positive | Status updated, updatedAt updated |
| QC-TSK-07 | Tasks | Đổi trạng thái bằng invalid enum (vd: xyz) | Negative | 400 Validation Error |
| QC-TSK-08 | Tasks | Delete/Archive task | Positive | Xóa thành công, ẩn khỏi active list |

## 6. Client Edge Cases Scenarios (Web/Mobile)

| ID | Module | Scenario Description | Type | Expected Result |
|---|---|---|---|---|
| QC-CLI-01 | UI | Xem list trống khi user mới đăng ký | Positive | Hiển thị empty state rõ ràng |
| QC-CLI-02 | UI | Disconnect internet và cố submit data | Negative | Handle lỗi network mượt mà |
| QC-CLI-03 | UI | Token hết hạn, call action | Negative | Auto logout hoặc auto refresh |
| QC-CLI-04 | UI | Submit quá nhanh / double click nút tạo | Edge | API idempotent hoặc UI disable nút chống spam |

## 7. Open Questions Impacting QA

(Không có block. Scope MVP đã chốt ở BA-001).

## 8. Acceptance Criteria

### AC-QC-001: Scenarios mapped to requirements

Given QC reviews the matrix  
When testing Auth, Notes, and Tasks  
Then positive, negative, and security/ownership core flows are covered.

### AC-QC-002: Security priority clear

Given QC creates test suites  
When executing test cases  
Then ownership security tests are explicitly identified and executed first.
