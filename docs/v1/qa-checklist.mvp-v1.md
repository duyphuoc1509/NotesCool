# QA CHECKLIST — MVP V1

> Mục tiêu: checklist QA tổng cho MVP v1 để kiểm tra readiness trước demo nội bộ, pilot, hoặc release.

---

## 1. QA Objectives

Checklist này giúp xác nhận rằng:
- MVP hoạt động đúng theo PRD và scope
- Các flow chính trên web/mobile dùng được
- APIs ổn định
- Ownership/security được áp dụng
- Không có lỗi nghiêm trọng cản trở demo/pilot

---

## 2. Scope Under Test

- Foundation readiness
- Auth/ownership baseline
- Notes module
- Tasks module
- Web app
- Mobile app
- API contracts
- Basic docs/runbook readiness

---

## 3. Test Environment Checklist

- [ ] Có môi trường local/dev ổn định
- [ ] Backend chạy được
- [ ] Frontend chạy được
- [ ] Mobile chạy được
- [ ] Database baseline đã được apply
- [ ] Test user/test data sẵn sàng
- [ ] API docs hoặc sample requests sẵn sàng

---

## 4. Foundation Checklist

- [ ] Repo structure khớp `solution-structure.md`
- [ ] Module boundaries không bị vi phạm rõ ràng
- [ ] API tuân theo `api-conventions.md`
- [ ] Error response format nhất quán
- [ ] Pagination contracts nhất quán
- [ ] CI/build baseline chạy được
- [ ] Run local theo docs không có blocker lớn

---

## 5. Auth & Ownership Checklist

- [ ] User hợp lệ có thể đăng nhập hoặc dùng auth flow đã chốt
- [ ] User A không xem được dữ liệu của User B
- [ ] User A không sửa được dữ liệu của User B
- [ ] User A không xóa/archive được dữ liệu của User B
- [ ] Unauthorized request bị chặn đúng
- [ ] Error codes cho auth/permission nhất quán

---

## 6. Notes API Checklist

- [ ] Create note thành công với input hợp lệ
- [ ] Create note fail đúng khi input không hợp lệ
- [ ] Get note detail thành công cho owner
- [ ] Get note detail bị chặn cho non-owner
- [ ] List notes có pagination đúng
- [ ] Search/filter notes cơ bản hoạt động đúng
- [ ] Update note thành công
- [ ] Invalid update bị chặn đúng
- [ ] Delete/archive note hoạt động đúng policy
- [ ] Response JSON đúng contract
- [ ] Error response đúng format

---

## 7. Tasks API Checklist

- [ ] Create task thành công với input hợp lệ
- [ ] Create task fail đúng khi input không hợp lệ
- [ ] Get task detail thành công cho owner
- [ ] Get task detail bị chặn cho non-owner
- [ ] List tasks có pagination đúng
- [ ] Filter/sort tasks cơ bản hoạt động đúng
- [ ] Update task thành công
- [ ] Invalid update bị chặn đúng
- [ ] Update task status thành công
- [ ] Invalid status transition bị chặn đúng nếu rule áp dụng
- [ ] Delete/archive task hoạt động đúng policy
- [ ] Response JSON đúng contract
- [ ] Error response đúng format

---

## 8. Web UI Checklist

### General
- [ ] App khởi động ổn định
- [ ] Navigation cơ bản hoạt động
- [ ] Loading states có hiển thị
- [ ] Empty states dễ hiểu
- [ ] Error states dễ hiểu
- [ ] Không có lỗi blocker rõ ràng trong flow chính

### Notes on web
- [ ] Xem danh sách notes
- [ ] Tạo note
- [ ] Sửa note
- [ ] Xóa/archive note
- [ ] Search/filter notes cơ bản
- [ ] Hiển thị dữ liệu nhất quán với API

### Tasks on web
- [ ] Xem danh sách tasks
- [ ] Tạo task
- [ ] Sửa task
- [ ] Đổi trạng thái task
- [ ] Xóa/archive task
- [ ] Filter/sort tasks cơ bản
- [ ] Hiển thị dữ liệu nhất quán với API

---

## 9. Mobile UI Checklist

### General
- [ ] App mobile khởi động ổn định
- [ ] Navigation cơ bản hoạt động
- [ ] Loading/empty/error states có
- [ ] Không có lỗi blocker rõ ràng trong flow chính

### Notes on mobile
- [ ] Xem danh sách notes
- [ ] Tạo note
- [ ] Sửa note
- [ ] Xóa/archive note theo policy
- [ ] Điều hướng list/detail ổn định

### Tasks on mobile
- [ ] Xem danh sách tasks
- [ ] Tạo task
- [ ] Sửa task
- [ ] Đổi trạng thái task
- [ ] Xóa/archive task theo policy
- [ ] Điều hướng list/detail ổn định

---

## 10. Contract Consistency Checklist

- [ ] Web và mobile dùng cùng contract shape
- [ ] JSON field names đúng convention
- [ ] Error codes nhất quán giữa modules
- [ ] Pagination shape nhất quán giữa Notes và Tasks
- [ ] Không có response trả internal-only fields ngoài ý muốn

---

## 11. Data Integrity Checklist

- [ ] createdAt/updatedAt được set đúng
- [ ] ownerId hoặc ownership mapping đúng
- [ ] Delete/archive behavior nhất quán
- [ ] Status task lưu và đọc lại đúng
- [ ] Search/filter/list không trả sai phạm vi dữ liệu

---

## 12. Quality Gates Checklist

- [ ] Unit tests pass
- [ ] Integration tests pass
- [ ] Architecture tests pass
- [ ] Contract checks pass
- [ ] Không có blocker severity mở
- [ ] Các known issues medium/low đã được ghi nhận rõ

---

## 13. Demo Readiness Checklist

- [ ] Có test account/demo data
- [ ] Có flow demo Notes
- [ ] Có flow demo Tasks
- [ ] Có flow demo ownership/security cơ bản nếu cần
- [ ] App đủ ổn định để trình diễn không bị gián đoạn lớn

---

## 14. MVP Release Recommendation

MVP chỉ nên được coi là ready khi:
- Tất cả checklist quan trọng ở mục 4–13 đã pass hoặc có ngoại lệ được PO/CEO chấp thuận rõ
- Không còn blocker ở Notes/Tasks flow chính
- Demo nội bộ chạy trơn tru
- Team hiểu rõ known gaps còn lại cho release 1.1

---

## 15. Defect Severity Guidance

### Blocker
- Không dùng được flow chính
- Không login được
- Không tạo/sửa/xem notes/tasks được
- Crash app ở flow chính
- Lộ dữ liệu user khác

### High
- Một chức năng chính hoạt động sai rõ ràng
- Task status sai contract
- Delete/archive gây state sai
- API contract mismatch làm UI hỏng

### Medium
- UX khó dùng nhưng vẫn hoàn thành được flow
- Search/filter có lỗi giới hạn
- Empty/error state chưa tốt

### Low
- Lỗi text/UI nhỏ
- Layout minor issue
- Improvement suggestions không cản trở MVP

---

## 16. Sign-off Recommendation

Nên có sign-off từ:
- PO/CEO: scope và trải nghiệm tổng thể
- QC: chất lượng và checklist
- Tech Lead: technical readiness
- Designer: flow/UX consistency cơ bản
