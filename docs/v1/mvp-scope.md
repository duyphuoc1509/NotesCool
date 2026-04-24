# MVP SCOPE

> Mục tiêu: chốt phạm vi release đầu tiên để team có thể bắt đầu delivery nhanh, rõ, không lan scope.

---

## 1. Product Goal for MVP

MVP v1 nhằm kiểm chứng rằng:
- người dùng có nhu cầu quản lý ghi chú và công việc trong cùng một sản phẩm
- trải nghiệm Notes + Tasks cơ bản là đủ hữu ích để demo/pilot
- kiến trúc module-first có thể hỗ trợ phát triển lâu dài mà không làm chậm pha đầu

---

## 2. Target User

**Đối tượng ban đầu**
- Cá nhân hoặc nhóm nhỏ cần ghi chú nhanh và theo dõi việc cần làm
- Người dùng muốn một công cụ gọn hơn các hệ thống project management lớn
- Người dùng ưu tiên tốc độ, rõ ràng, và quản lý việc cá nhân

---

## 3. MVP Success Criteria

MVP được xem là đạt nếu:
- Một user có thể đăng nhập và quản lý dữ liệu của riêng mình
- Có thể tạo/sửa/xóa/xem Notes
- Có thể tạo/sửa/xóa/xem Tasks
- Có thể đổi trạng thái task
- Có thể dùng web app để hoàn thành flow chính
- Có thể dùng mobile app cho các flow chính
- Demo nội bộ trơn tru
- Team có thể tiếp tục mở rộng mà không phải rewrite kiến trúc nền

---

## 4. Must-have Scope

## 4.1 Foundation
- Project skeleton cho backend, web, mobile
- Module boundaries rõ
- Shared contracts cơ bản
- Auth/ownership tối thiểu
- Error handling thống nhất
- Logging/middleware cơ bản
- Local setup/run guide

## 4.2 Notes
- Create note
- Update note
- Delete hoặc archive note theo policy
- Get note detail
- List notes
- Pagination cơ bản
- Search/filter cơ bản theo title/keyword
- Web UI flow cơ bản
- Mobile UI flow cơ bản

## 4.3 Tasks
- Create task
- Update task
- Delete hoặc archive task theo policy
- Get task detail
- List tasks
- Pagination cơ bản
- Filter theo status / due date / priority ở mức tối thiểu
- Update task status
- Web UI flow cơ bản
- Mobile UI flow cơ bản

## 4.4 Quality
- Unit tests cho domain/application rules chính
- Integration tests cho API chính
- Contract checks cơ bản
- Architecture checks cho module boundary
- README đủ để chạy local

---

## 5. Should-have Scope

- Archive thay vì hard delete nếu effort hợp lý
- Basic tags cho notes hoặc tasks
- Link task với note ở mức đơn giản
- Search tốt hơn basic keyword
- Empty/loading/error states chỉn chu hơn ở UI
- Basic dashboard/home summary rất nhẹ

---

## 6. Not-now Scope

- Collaboration realtime
- Multi-user shared workspace phức tạp
- Reminder delivery thực
- Push notifications
- Realtime sync
- Recurrence phức tạp
- Kanban board nâng cao
- Timeline/calendar nâng cao
- AI features
- Plugin marketplace
- Analytics/reporting nâng cao
- Enterprise RBAC phức tạp

---

## 7. Web MVP Flows

Web phải hỗ trợ tối thiểu:
- Login
- Xem danh sách notes
- Tạo/chỉnh sửa/xóa note
- Xem danh sách tasks
- Tạo/chỉnh sửa/xóa task
- Đổi trạng thái task
- Search/filter cơ bản
- Điều hướng đơn giản giữa Notes và Tasks

---

## 8. Mobile MVP Flows

Mobile phải hỗ trợ tối thiểu:
- Login
- Xem danh sách notes
- Tạo/chỉnh sửa note
- Xem danh sách tasks
- Tạo/chỉnh sửa task
- Đổi trạng thái task
- Trải nghiệm đủ tốt cho use case hàng ngày

Không bắt buộc ở MVP:
- Toàn bộ flow quản trị
- Mọi edge case desktop-level
- Mọi tối ưu UI nâng cao

---

## 9. Product Constraints

- Không lan scope ngoài Must-have nếu chưa có quyết định PO rõ
- Không hy sinh boundary kiến trúc để đổi lấy tốc độ ngắn hạn
- Không làm trước các tính năng “có vẻ hay” nhưng chưa chứng minh giá trị cho MVP
- Không tạo divergence contracts giữa web và mobile

---

## 10. Release Recommendation

**Release 1 nội bộ**
- Backend foundation
- Web full MVP
- Mobile partial-but-usable MVP

**Release 1.1**
- Polish mobile flow
- Link note-task đơn giản
- Archive/tag/search tốt hơn

---

## 11. Acceptance Criteria at Product Level

- User A không thấy dữ liệu của User B
- Note CRUD hoạt động ổn định
- Task CRUD hoạt động ổn định
- Task status update rõ và nhất quán
- APIs đủ ổn định để web/mobile cùng dùng
- App chạy local đơn giản
- Demo được luồng end-to-end cơ bản
