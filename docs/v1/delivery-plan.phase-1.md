# DELIVERY PLAN — PHASE 1

> Mục tiêu: kế hoạch thực thi phase 1 để chuyển từ planning sang implementation readiness cho dự án Notes - Tasks.

---

## 1. Phase 1 Goal

Phase 1 tập trung vào:
- dựng nền kỹ thuật và delivery foundation
- khóa các quyết định quan trọng để team không đi sai hướng
- chuẩn bị đủ điều kiện để bắt đầu implementation Notes và Tasks một cách kiểm soát được

Phase 1 **chưa** nhằm hoàn tất toàn bộ MVP feature.

---

## 2. Phase 1 Outcomes

Kết thúc phase 1, team phải có:
- repo structure rõ
- docs nền đủ dùng
- backend skeleton chạy được
- frontend skeleton chạy được
- mobile skeleton chạy được
- API conventions đã chốt
- module boundaries đã rõ
- auth/ownership baseline đã có quyết định
- test baseline đã dựng xong
- backlog Notes/Tasks sẵn sàng cho phase tiếp theo

---

## 3. Workstreams

## 3.1 Product & Architecture
**Owner đề xuất**
- CEO/PO + Tech Lead + BA

**Deliverables**
- MVP scope chốt
- Module map chốt
- API conventions chốt
- Solution structure chốt

## 3.2 Backend Foundation
**Owner đề xuất**
- Backend Lead + Backend Dev

**Deliverables**
- .NET host skeleton
- Shared kernel baseline
- Notes/Tasks module skeleton
- Middleware/error handling base
- DB baseline

## 3.3 Frontend Web Foundation
**Owner đề xuất**
- Frontend Lead + Frontend Dev + Designer

**Deliverables**
- React app shell
- Routing/layout base
- Feature folders
- API client baseline
- Shared UI state patterns

## 3.4 Mobile Foundation
**Owner đề xuất**
- Mobile Dev + Designer

**Deliverables**
- React Native app shell
- Navigation base
- Feature folders
- API/auth baseline
- Shared mobile utilities

## 3.5 Quality & Delivery
**Owner đề xuất**
- QC + Tech Lead + Devs

**Deliverables**
- Unit test baseline
- Integration test baseline
- Architecture tests baseline
- CI baseline
- Runbook/getting started docs

---

## 4. Recommended Sequence

## Step 1 — Lock decisions
- Review `kickoff-prompt.mvp-v1.md`
- Review `module-map.md`
- Review `mvp-scope.md`
- Review `api-conventions.md`
- Review `solution-structure.md`

**Exit criteria**
- Không còn ambiguity lớn về scope phase đầu
- Team thống nhất boundaries và conventions

## Step 2 — Create repo/skeleton
- Tạo monorepo structure
- Dựng backend/frontend/mobile skeleton
- Dựng docs folders và tests folders

**Exit criteria**
- Repo chạy được ở mức skeleton
- Team có thể clone và khởi động môi trường cơ bản

## Step 3 — Implement technical baseline
- Middleware/error handling
- Shared contracts
- Auth baseline
- DB baseline
- API client baseline

**Exit criteria**
- Có nền để Notes/Tasks bắt đầu cắm vào
- Không phải quay lại sửa foundation liên tục

## Step 4 — Add quality gates
- Unit test harness
- Integration test baseline
- Architecture checks
- CI pipeline cơ bản

**Exit criteria**
- Build/test tự động chạy được
- Dependency sai chiều có cơ chế phát hiện

## Step 5 — Prepare feature execution
- Chuyển Notes/Tasks thành backlog thực thi
- Ước lượng effort
- Chốt phase 2 kickoff

**Exit criteria**
- Team sẵn sàng bắt đầu implementation feature

---

## 5. Suggested Timeline

## Week 1
- Lock product/architecture decisions
- Setup repo structure
- Setup backend skeleton
- Setup frontend skeleton
- Setup mobile skeleton

## Week 2
- Add shared contracts
- Add API conventions into codebase/docs
- Setup DB baseline
- Setup auth/ownership baseline (local JWT bearer + refresh token direction)
- Setup API client baseline

## Week 3
- Add tests baseline
- Add CI baseline
- Polish runbook/docs
- Review readiness for Notes/Tasks implementation

Nếu team nhỏ, có thể gộp linh hoạt. Nếu team lớn hơn, có thể chạy song song theo workstream.

---

## 6. Team Cadence Recommendation

### Weekly cadence
- **Monday:** plan tuần + chốt scope
- **Mid-week:** architecture/dev sync + blocker review
- **Friday:** demo progress + retro + adjust backlog

### Daily cadence
- Standup rất ngắn
- Focus vào blockers, dependency, decision cần escalation

---

## 7. Role Expectations

## CEO/PO
- Chốt scope
- Chống lan scope
- Ra quyết định ưu tiên
- Duy trì nhịp delivery

## BA
- Viết acceptance criteria
- Làm rõ use cases
- Bóc tách Notes/Tasks backlog

## Tech Lead
- Chốt technical choices
- Giữ module boundaries
- Review conventions và implementation direction

## Backend Dev
- Dựng host/shared/modules foundation
- Chốt API/public contracts với team

## Frontend Dev
- Dựng web shell
- Bám API conventions
- Tổ chức feature boundaries rõ

## Mobile Dev
- Dựng mobile shell
- Bám contracts như web
- Tối ưu flow cốt lõi cho mobile

## QC
- Xây checklist smoke
- Xác định test coverage ưu tiên
- Review acceptance criteria từ sớm

## Designer
- Chốt flow và wireframe MVP
- Giữ consistency web/mobile
- Không mở rộng scope visual quá sớm

---

## 8. Risks & Mitigations

## Risk 1 — Scope creep
**Mitigation**
- Bám `mvp-scope.md`
- Mọi feature mới phải qua PO decision

## Risk 2 — Coupling sai giữa modules
**Mitigation**
- Bám `module-map.md`
- Có architecture tests
- Review checklist theo dependency rules

## Risk 3 — Web/mobile diverge contracts
**Mitigation**
- Dùng `api-conventions.md`
- Không tạo shape response riêng tùy client

## Risk 4 — Over-engineering từ quá sớm
**Mitigation**
- Phase 1 chỉ làm foundation đủ dùng
- Plugin-ready nhưng không làm plugin system hoàn chỉnh

## Risk 5 — Team bắt đầu feature khi foundation chưa ổn
**Mitigation**
- Có exit criteria phase 1
- Không vào phase 2 nếu skeleton/quality gates chưa đủ

---

## 9. Phase 1 Exit Criteria

Phase 1 hoàn tất khi:
- Tài liệu nền đã chốt
- Repo structure rõ
- Backend skeleton chạy được
- Frontend skeleton chạy được
- Mobile skeleton chạy được
- API conventions rõ
- Shared contracts baseline rõ
- Auth/ownership baseline rõ
- DB baseline rõ
- Unit/integration/architecture test baseline rõ
- CI baseline chạy được
- Notes/Tasks backlog phase 2 sẵn sàng

---

## 10. Immediate Next Actions

1. Review và chốt 3 tài liệu:
   - `api-conventions.md`
   - `solution-structure.md`
   - `delivery-plan.phase-1.md`

2. Tạo repo structure thật trong codebase

3. Tạo backlog implementation cho:
   - backend skeleton
   - frontend skeleton
   - mobile skeleton

4. Sau đó mới kickoff phase implementation đầu tiên

---

## 11. Suggested Command After Review

Sau khi review xong bộ tài liệu phase 1, có thể bắt đầu bằng một prompt như:

```text
Đọc kickoff-prompt.foundation.md, module-map.md, api-conventions.md, solution-structure.md và delivery-plan.phase-1.md. Sau đó lập kế hoạch implementation phase 1 theo backlog.foundation.md.
```
