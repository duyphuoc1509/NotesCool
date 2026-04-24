# FOUNDATION BACKLOG

> Mục tiêu: danh sách backlog ưu tiên cao cho phase 1 để dựng nền dự án Notes - Tasks.

---

## 1. Backlog Principles

- Ưu tiên foundation trước feature polish
- Chỉ làm những gì cần để mở đường cho Notes/Tasks MVP
- Contract-first
- Boundary-safe
- Mỗi item phải có đầu ra rõ, review được, test được

---

## 2. Epic List

1. Project Setup & Repository Structure
2. Backend Host & Module Composition
3. Shared Kernel & Common Contracts
4. Authentication & Ownership Baseline
5. API Conventions & Error Handling
6. Database Baseline & Migration Setup
7. Frontend Web Skeleton
8. Mobile Skeleton
9. Quality Gates & Test Harness
10. Documentation & Delivery Runbook

---

## 3. Prioritized Backlog

## EPIC 1 — Project Setup & Repository Structure

### FB-001 Create monorepo structure
**Goal**
- Tạo cấu trúc thư mục chuẩn cho backend / frontend / mobile / docs / tests

**Acceptance criteria**
- Repo có structure thống nhất
- Có README root mô tả structure
- Team biết bắt đầu từ đâu

### FB-002 Define naming conventions
**Goal**
- Chốt quy ước đặt tên project, module, folder, contracts, DTOs, tests

**Acceptance criteria**
- Có doc conventions
- Team dùng chung một cách đặt tên

---

## EPIC 2 — Backend Host & Module Composition

### FB-003 Create backend solution skeleton
**Goal**
- Dựng solution .NET 8 cho host, shared, notes, tasks

**Acceptance criteria**
- Build được
- Chạy được host
- Modules được đăng ký rõ

### FB-004 Define module registration pattern
**Goal**
- Chuẩn hóa cách module expose services/endpoints

**Acceptance criteria**
- Có pattern nhất quán cho Notes/Tasks
- Không cần copy-paste ngẫu hứng sau này

### FB-005 Add base middleware pipeline
**Goal**
- Có exception handling, request logging, auth hooks cơ bản

**Acceptance criteria**
- Pipeline khởi động ổn định
- Lỗi trả về theo format chung

---

## EPIC 3 — Shared Kernel & Common Contracts

### FB-006 Create shared primitives
**Goal**
- Tạo common result/error/paging/audit/context abstractions

**Acceptance criteria**
- Shared kernel tối giản
- Không chứa business logic của Notes/Tasks

### FB-007 Define API response conventions
**Goal**
- Chuẩn hóa success/error response, paging envelope, validation error model

**Acceptance criteria**
- Notes/Tasks có thể reuse cùng convention
- Docs rõ ràng

### FB-008 Define time/user context abstractions
**Goal**
- Có abstraction cho current user và current time

**Acceptance criteria**
- Không hard-code time/user access trong domain/application

---

## EPIC 4 — Authentication & Ownership Baseline

### FB-009 Choose auth approach for MVP
**Goal**
- Chốt cơ chế auth tối thiểu cho MVP

**Decision**
- MVP backend dùng local JWT bearer access token
- Có refresh token flow trong baseline auth direction
- User identity lấy từ auth context để phục vụ ownership

**Acceptance criteria**
- Có quyết định rõ: local JWT bearer + refresh token baseline
- Không mơ hồ giữa nhiều hướng

### FB-010 Implement ownership baseline
**Goal**
- Mọi entity MVP có owner/user scope rõ

**Acceptance criteria**
- Có cách áp ownership thống nhất
- Không rò dữ liệu giữa users

---

## EPIC 5 — API Conventions & Error Handling

### FB-011 Create API conventions doc
**Goal**
- Chuẩn hóa endpoint naming, HTTP semantics, DTO naming, versioning rule

**Acceptance criteria**
- Có doc dùng chung cho Notes/Tasks

### FB-012 Implement validation/error handling base
**Goal**
- Có cơ chế validate request và map lỗi domain/application -> HTTP response

**Acceptance criteria**
- Error response nhất quán
- Validation errors dễ dùng cho web/mobile

---

## EPIC 6 — Database Baseline & Migration Setup

### FB-013 Setup PostgreSQL integration
**Goal**
- Chuẩn bị kết nối DB local/dev

**Acceptance criteria**
- Host kết nối DB được
- Có config rõ cho local env

### FB-014 Setup EF Core baseline
**Goal**
- Tạo baseline persistence/migration strategy

**Acceptance criteria**
- Tạo migration được
- Có hướng dẫn update DB

### FB-015 Define audit and soft-delete policy
**Goal**
- Chốt strategy cho created/updated/archive/delete

**Acceptance criteria**
- Có quyết định thống nhất cho toàn MVP

---

## EPIC 7 — Frontend Web Skeleton

### FB-016 Create React app skeleton
**Goal**
- Dựng app shell, routing, shared layout, feature folders

**Acceptance criteria**
- App chạy local
- Có feature boundaries Notes/Tasks

### FB-017 Setup API client conventions
**Goal**
- Chuẩn hóa cách gọi API, error mapping, auth token flow

**Acceptance criteria**
- Có shared API layer
- Không để từng feature tự làm khác nhau

### FB-018 Define shared UI states
**Goal**
- Chuẩn hóa loading / empty / error state cơ bản

**Acceptance criteria**
- Có guideline cho UI consistency

---

## EPIC 8 — Mobile Skeleton

### FB-019 Create React Native app skeleton
**Goal**
- Dựng app shell, navigation, feature folders, shared utilities

**Acceptance criteria**
- App mobile chạy local/dev
- Có cấu trúc rõ cho Notes/Tasks

### FB-020 Setup API/auth flow baseline for mobile
**Goal**
- Chuẩn hóa cách mobile gọi API và xử lý auth tối thiểu

**Acceptance criteria**
- Có baseline dùng lại cho các feature

---

## EPIC 9 — Quality Gates & Test Harness

### FB-021 Setup backend unit test projects
**Goal**
- Có test harness cho Notes/Tasks/Shared

**Acceptance criteria**
- Chạy test được local

### FB-022 Setup integration test baseline
**Goal**
- Có cơ chế test API chính với environment kiểm soát được

**Acceptance criteria**
- Có ít nhất một integration smoke test

### FB-023 Add architecture tests
**Goal**
- Giữ module boundaries sạch bằng rule checks

**Acceptance criteria**
- Có test chặn dependency sai chiều

### FB-024 Add CI baseline
**Goal**
- Build + test cơ bản tự động

**Acceptance criteria**
- Có pipeline kiểm tra build/test chính

---

## EPIC 10 — Documentation & Delivery Runbook

### FB-025 Create getting-started docs
**Goal**
- Team onboard nhanh

**Acceptance criteria**
- README mô tả cách run backend/web/mobile

### FB-026 Create architecture overview doc
**Goal**
- Tóm tắt module map, dependency rules, conventions

**Acceptance criteria**
- Dev mới đọc là hiểu nền dự án

### FB-027 Create MVP delivery checklist
**Goal**
- Có checklist theo dõi readiness trước khi bắt đầu feature work

**Acceptance criteria**
- Team biết foundation nào đã done/chưa done

---

## 4. Suggested Phase 1 Order

### Sprint/Phase 1A
- FB-001
- FB-003
- FB-004
- FB-006
- FB-011
- FB-013
- FB-016
- FB-019
- FB-021

### Sprint/Phase 1B
- FB-005
- FB-007
- FB-008
- FB-009
- FB-010
- FB-014
- FB-017
- FB-020
- FB-022
- FB-023

### Sprint/Phase 1C
- FB-012
- FB-015
- FB-018
- FB-024
- FB-025
- FB-026
- FB-027

---

## 5. Definition of Done for Foundation

Foundation được xem là sẵn sàng khi:
- Repo structure rõ
- Backend skeleton chạy được
- Web skeleton chạy được
- Mobile skeleton chạy được
- Auth/ownership baseline rõ
- Shared contracts rõ
- DB baseline và migration strategy rõ
- Unit/integration/architecture test baseline có sẵn
- README/onboarding docs đủ dùng
- Team sẵn sàng bắt đầu Notes/Tasks feature implementation
