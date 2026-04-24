# RISK REGISTER — MVP V1

> Mục tiêu: theo dõi các rủi ro chính của MVP v1 để CEO/PO, Tech Lead và team chủ động quản trị thay vì xử lý bị động.

---

## 1. Purpose

Tài liệu này giúp:
- nhận diện rủi ro sớm
- theo dõi mức độ ảnh hưởng
- xác định owner theo dõi
- chốt mitigation
- hỗ trợ quyết định ưu tiên

---

## 2. Risk Rating Model

### Impact
- High
- Medium
- Low

### Likelihood
- High
- Medium
- Low

### Status
- Open
- Monitoring
- Mitigated
- Accepted
- Closed

---

## 3. Risk Register

## R-01 Scope creep in MVP
**Description**
- MVP dễ bị mở rộng thêm nhiều tính năng “hay” nhưng chưa cần.

**Impact**
- High

**Likelihood**
- High

**Owner**
- CEO/PO

**Mitigation**
- Bám `mvp-scope.md`
- Mọi scope mới phải đi qua review
- Phân biệt rõ Must-have / Should-have / Not-now

**Status**
- Open

---

## R-02 Over-engineering architecture too early
**Description**
- Team có thể xây nền quá phức tạp, làm chậm delivery MVP.

**Impact**
- High

**Likelihood**
- Medium

**Owner**
- Tech Lead

**Mitigation**
- Modular monolith first
- Plugin-ready ở mức direction, không xây plugin framework hoàn chỉnh ngay
- Timebox các quyết định kỹ thuật lớn

**Status**
- Open

---

## R-03 Cross-module coupling between Notes and Tasks
**Description**
- Notes và Tasks có thể vô tình phụ thuộc implementation của nhau.

**Impact**
- High

**Likelihood**
- Medium

**Owner**
- Tech Lead

**Mitigation**
- Bám `module-map.md`
- Review dependency direction
- Architecture tests/checks

**Status**
- Open

---

## R-04 API contract drift between backend, web, and mobile
**Description**
- Web/mobile có thể bám contract khác nhau hoặc backend thay contract không kiểm soát.

**Impact**
- High

**Likelihood**
- Medium

**Owner**
- Tech Lead + Frontend Lead + Mobile Dev

**Mitigation**
- Contract-first
- Bám `api-conventions.md`
- Contract checks
- Review impact trước khi đổi contract

**Status**
- Open

---

## R-05 Foundation incomplete before feature work starts
**Description**
- Team bắt đầu làm Notes/Tasks khi foundation chưa đủ, gây rework.

**Impact**
- High

**Likelihood**
- Medium

**Owner**
- CEO/PO + Tech Lead

**Mitigation**
- Bám exit criteria phase 1
- Không kick off feature implementation nếu chưa sẵn sàng

**Status**
- Open

---

## R-06 Auth/ownership unclear in MVP
**Description**
- Chưa chốt rõ auth hoặc ownership có thể gây lỗi bảo mật và rework lớn.

**Impact**
- High

**Likelihood**
- Medium

**Owner**
- Backend Lead + CEO/PO

**Mitigation**
- Chốt auth baseline sớm: local JWT bearer + refresh token direction cho MVP
- Test ownership cases từ đầu
- Không để ownership là “làm sau”

**Status**
- Open

---

## R-07 Mobile parity assumptions are unrealistic
**Description**
- Kỳ vọng mobile ngang web quá sớm có thể làm trễ MVP.

**Impact**
- Medium

**Likelihood**
- Medium

**Owner**
- CEO/PO + Mobile Dev + Designer

**Mitigation**
- Chốt rõ mobile MVP flows
- Không đòi parity 100% với web trong MVP nếu không cần

**Status**
- Open

---

## R-08 Search/filter complexity expands unexpectedly
**Description**
- Search/filter cho Notes/Tasks có thể mở rộng quá mức cần thiết.

**Impact**
- Medium

**Likelihood**
- Medium

**Owner**
- PO + Backend Lead

**Mitigation**
- Chỉ làm basic search/filter cho MVP
- Đưa advanced cases sang release 1.1+

**Status**
- Open

---

## R-09 Delete vs archive policy not decided early
**Description**
- Nếu policy mơ hồ, backend/web/mobile/QA sẽ hiểu khác nhau.

**Impact**
- Medium

**Likelihood**
- High

**Owner**
- CEO/PO + BA + Tech Lead

**Mitigation**
- Chốt policy sớm
- Document rõ expected behavior
- Kiểm tra nhất quán qua acceptance criteria

**Status**
- Open

---

## R-10 Inadequate test baseline
**Description**
- Không có test baseline sớm sẽ làm regressions khó kiểm soát.

**Impact**
- High

**Likelihood**
- Medium

**Owner**
- QC + Tech Lead

**Mitigation**
- Dựng unit/integration/architecture tests từ foundation
- Không để test là việc “cuối cùng”

**Status**
- Open

---

## R-11 Team decisions not documented
**Description**
- Quyết định quan trọng không được ghi lại dẫn đến hiểu sai và lặp tranh luận.

**Impact**
- Medium

**Likelihood**
- High

**Owner**
- CEO/PO + BA + Tech Lead

**Mitigation**
- Ghi lại decision log ngắn
- Cập nhật docs ngay sau quyết định quan trọng

**Status**
- Open

---

## R-12 QA engaged too late
**Description**
- Nếu QC tham gia muộn, acceptance criteria và edge cases sẽ bị bỏ sót.

**Impact**
- Medium

**Likelihood**
- Medium

**Owner**
- QC Lead / QC

**Mitigation**
- QC tham gia từ giai đoạn planning
- QA checklist được dùng trước release, không chỉ sau khi code xong

**Status**
- Open

---

## R-13 Demo instability before internal review
**Description**
- MVP có thể chạy được nhưng không đủ ổn định để demo.

**Impact**
- Medium

**Likelihood**
- Medium

**Owner**
- Tech Lead + QC + PO

**Mitigation**
- Có checklist demo readiness
- Có smoke checks cho flows chính
- Chuẩn bị test data/demo account trước

**Status**
- Open

---

## R-14 Team overloaded by parallel workstreams
**Description**
- Nếu cùng lúc làm quá nhiều lane, team nhỏ dễ quá tải và mất focus.

**Impact**
- Medium

**Likelihood**
- Medium

**Owner**
- CEO/PO

**Mitigation**
- Ưu tiên critical path
- Không mở quá nhiều front cùng lúc
- Chia sprint theo readiness thực tế

**Status**
- Open

---

## R-15 Docs diverge from implementation
**Description**
- Docs có thể bị cũ nhanh nếu team không cập nhật song song.

**Impact**
- Medium

**Likelihood**
- Medium

**Owner**
- BA + Tech Lead + module owners

**Mitigation**
- Mỗi sprint có deliverable docs
- Không đóng item nếu docs cần thiết chưa cập nhật

**Status**
- Open

---

## 4. Top Priority Risks to Watch Weekly

Khuyến nghị review hàng tuần ít nhất các risk sau:
- R-01 Scope creep in MVP
- R-03 Cross-module coupling
- R-04 API contract drift
- R-05 Foundation incomplete before feature work
- R-06 Auth/ownership unclear
- R-10 Inadequate test baseline

---

## 5. Risk Review Cadence

### Weekly
- Review top priority risks
- Cập nhật status
- Đánh giá risk nào tăng/giảm

### At sprint planning
- Xem risk nào ảnh hưởng sprint goal
- Gắn risk owner rõ

### Before demo/release
- Review blocker/high-impact risks
- Xác định accepted risks nào còn lại

---

## 6. Risk Escalation Rule

Phải escalate ngay nếu:
- risk đe dọa sprint goal
- risk có khả năng gây rework lớn
- risk liên quan security/ownership
- risk có thể gây breaking contract
- risk làm lệch MVP scope

---

## 7. Risk Register Maintenance Rule

Mỗi risk nên được cập nhật:
- status hiện tại
- owner
- mitigation đã làm
- signal mới nếu có
- quyết định accept/mitigate nếu thay đổi

---

## 8. Summary

Risk register này là công cụ để:
- bảo vệ MVP scope
- giữ chất lượng delivery
- hỗ trợ quyết định ưu tiên
- giảm bất ngờ khi bước vào implementation và release
