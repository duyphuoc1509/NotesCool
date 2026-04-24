# DEFINITION OF DONE

> Mục tiêu: thống nhất tiêu chuẩn “done” cho dự án Notes - Tasks để tránh hiểu khác nhau giữa PO, DEV, QC, Designer.

---

## 1. Purpose

Tài liệu này áp dụng cho:
- stories
- backlog items
- module milestones
- sprint deliverables
- MVP readiness review

“Done” nghĩa là:
- không chỉ code xong
- mà phải đúng scope, đúng quality, đúng behavior, và review được

---

## 2. Core Principles

- Done phải kiểm chứng được
- Done phải gắn với acceptance criteria
- Done phải bao gồm cả quality tối thiểu
- Done phải phản ánh trải nghiệm thật của user nếu là user-facing flow
- Không dùng “gần xong” như “done”

---

## 3. General Definition of Done for Any Work Item

Một work item chỉ được xem là done khi:
- [ ] Scope của item đã hoàn thành đúng như mô tả
- [ ] Acceptance criteria liên quan đã pass
- [ ] Không còn ambiguity lớn về behavior của item
- [ ] Code/doc/UI liên quan đã được cập nhật nếu cần
- [ ] Item đã được review ở mức phù hợp
- [ ] Không còn blocker hoặc defect nghiêm trọng chưa được ghi nhận

---

## 4. Definition of Done for Backend Items

Một item backend được xem là done khi:
- [ ] Logic triển khai đúng contract đã chốt
- [ ] Validation hoạt động đúng
- [ ] Error handling theo chuẩn chung
- [ ] Ownership/security expectations đúng
- [ ] Không vi phạm module boundaries
- [ ] Code đã được review
- [ ] Tests liên quan đã pass
- [ ] Docs/API samples được cập nhật nếu cần

---

## 5. Definition of Done for Frontend/Mobile Items

Một item frontend/mobile được xem là done khi:
- [ ] UI phản ánh đúng scope đã chốt
- [ ] Dùng đúng public API contracts
- [ ] Loading state có
- [ ] Empty state có nếu applicable
- [ ] Error state có nếu applicable
- [ ] Flow chính hoạt động end-to-end
- [ ] Không phụ thuộc undocumented/internal data
- [ ] Đã được review về UX/functionality
- [ ] Không còn blocker rõ trong flow chính

---

## 6. Definition of Done for QA Items

Một item QA được xem là done khi:
- [ ] Test scenarios đã được xác định rõ
- [ ] Kết quả test được ghi nhận
- [ ] Bug reports có repro steps rõ
- [ ] Severity rõ
- [ ] Blockers/high issues đã được nêu rõ cho team
- [ ] QA sign-off theo checklist nếu applicable

---

## 7. Definition of Done for Documentation Items

Một doc item được xem là done khi:
- [ ] Nội dung phản ánh đúng quyết định hiện tại
- [ ] Không mâu thuẫn với docs liên quan
- [ ] Đủ rõ để team sử dụng
- [ ] Có cấu trúc dễ đọc
- [ ] Đã được review bởi role liên quan nếu cần

---

## 8. Definition of Done for Sprint Items

Một sprint item được xem là done khi:
- [ ] Đạt acceptance criteria của item
- [ ] Không còn blocker ngăn demo hoặc handoff
- [ ] Dependencies liên quan đã được xử lý hoặc ghi nhận
- [ ] Có thể báo cáo trạng thái rõ ràng là done mà không cần caveat lớn

---

## 9. Definition of Done for Notes Module Stories

Một story của Notes được xem là done khi:
- [ ] `acceptance-criteria.notes.md` tương ứng đã pass
- [ ] Notes API/UI behavior đúng contract
- [ ] Ownership đúng
- [ ] Loading/empty/error states đủ dùng nếu là UI flow
- [ ] Có tests/check liên quan
- [ ] Không vi phạm module boundaries

---

## 10. Definition of Done for Tasks Module Stories

Một story của Tasks được xem là done khi:
- [ ] `acceptance-criteria.tasks.md` tương ứng đã pass
- [ ] Tasks API/UI behavior đúng contract
- [ ] Ownership đúng
- [ ] Status flow đúng theo rule đã chốt
- [ ] Loading/empty/error states đủ dùng nếu là UI flow
- [ ] Có tests/check liên quan
- [ ] Không vi phạm module boundaries

---

## 11. Definition of Done for Sprint Completion

Một sprint chỉ được xem là completed tốt khi:
- [ ] Sprint goal đạt hoặc chênh lệch đã được giải thích rõ
- [ ] Các item done thật sự đạt DoD
- [ ] Có demo được phần đã hoàn thành
- [ ] Risks/gaps còn lại được ghi nhận rõ
- [ ] Backlog đã được cập nhật cho sprint tiếp theo

---

## 12. Definition of Done for MVP Readiness

MVP v1 chỉ được xem là ready khi:
- [ ] PRD/MVP scope must-have đã đạt
- [ ] Notes flow chính hoạt động
- [ ] Tasks flow chính hoạt động
- [ ] Web usable
- [ ] Mobile usable cho flow cốt lõi
- [ ] Auth/ownership đúng
- [ ] API contracts ổn định
- [ ] QA checklist quan trọng đã pass
- [ ] Không còn blocker severity mở
- [ ] Known issues còn lại đã được PO/CEO chấp thuận rõ

---

## 13. What Is NOT Done

Một item **không** được xem là done nếu:
- code đã viết nhưng chưa review
- flow chỉ chạy được một phần
- acceptance criteria chưa pass
- còn phụ thuộc undocumented assumptions
- còn bug nghiêm trọng chưa được ghi nhận rõ
- docs cần thiết chưa cập nhật
- chưa test gì nhưng nói “chắc là ổn”

---

## 14. Practical Rule

Nếu team không thể:
- demo được,
- giải thích behavior rõ ràng,
- hoặc chỉ ra acceptance criteria nào đã pass,

thì item đó chưa nên được coi là done.
