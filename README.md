# Notes - Tasks Foundation Kickoff

## Task Summary

This repository starts the foundation architecture setup for the Notes - Tasks product.

Main goal:
- Establish a technical baseline and architecture principles for a modular monolith first, plugin-ready, contract-first system.
- Ensure Product Owner, Business Analyst, Developer, QA, and Designer can work consistently from the same foundation.

Expected outcomes:
- Foundation architecture documents
- Project skeleton for backend, frontend, and mobile
- Module map and dependency rules
- Initial coding and working conventions
- Prioritized foundation backlog
- Definition of Done and quality gates for the startup phase

## Working Mode

Current mode:
- Discovery
- Planning
- Documentation

Preferred approach:
- TDD-first
- Contract-first
- Minimal diff
- Modular/plugin-safe
- Focus on docs and foundation direction first

## Project Direction

### Tech Stack

- Backend: .NET 8, ASP.NET Core Web API
- Frontend: ReactJS
- Mobile: React Native
- Database: PostgreSQL
- ORM: EF Core
- API style: REST-first
- Auth baseline: JWT + Refresh Token
- Testing: xUnit/NUnit (backend), React testing stack (web), incremental strategy by phase

### Architecture Direction

- Modular monolith first
- Plugin-ready without over-engineering in early phase
- Contract-first between modules
- Domain-oriented boundaries
- Minimal shared kernel
- Prioritize maintainability, testability, clear ownership, and team scalability
- Keep upgrade path for future module/service extraction

### Boundary Rules

- Notes and Tasks are independent business modules
- Shared/Common contains only true shared primitives, abstractions, and cross-cutting concerns
- No direct access to other module internals
- Cross-module communication through contracts, integration events, or approved facades
- Plugins depend on contracts/abstractions, not implementation details
- UI must not own core business rules from domain/application layers
- Persistence concerns must not leak into API contracts

## Scope

Priority paths:
- `README.md`
- `docs/`
- `docs/architecture/`
- `docs/delivery/` (Phase 1 Delivery Plan: `docs/delivery/delivery-plan.phase-1.md`)
- `docs/product/`
- `src/backend/`
- `src/frontend/`
- `src/mobile/`
- `tests/`

Target update areas in foundation phase:
- `docs/architecture/`
- `docs/delivery/` (Phase 1 Delivery Plan: `docs/delivery/delivery-plan.phase-1.md`)
- `docs/conventions/`
- `docs/backlog/`
- `src/backend/`
- `src/frontend/`
- `src/mobile/`
- `tests/`

## Requirements

### Business / Functional

- REQ-01: Foundation supports independent Notes and Tasks development
- REQ-02: Architecture supports future module/plugin extensions
- REQ-03: Web and mobile consume consistent API/contracts
- REQ-04: Project structure is clear for cross-role collaboration
- REQ-05: Foundation enables feature implementation without major core redesign

### Technical

- TECH-01: Backend on .NET 8
- TECH-02: Frontend on ReactJS
- TECH-03: Mobile on React Native
- TECH-04: Backend architecture follows modular monolith module-first model
- TECH-05: Clear boundary, dependency direction, and cross-module communication rules
- TECH-06: Baseline API/DTO/error/paging conventions
- TECH-07: Consistent backend/frontend/mobile structure
- TECH-08: Baseline strategy for unit, contract, integration, and architecture tests
- TECH-09: Coding conventions, naming, decision log, and workflow documentation

### Security / Performance / Compliance

- SEC-01: Secure-by-default authn/authz baseline
- SEC-02: Boundary-level input validation
- SEC-03: No unauthorized cross-module data access
- SEC-04: Error responses avoid sensitive leak
- PERF-01: List endpoints designed for paging/filter/sort
- PERF-02: Avoid N+1 query design patterns
- PERF-03: Stable contracts and controlled breaking changes
- COMP-01: Architectural decisions documented and traceable

## Non-Goals (Current Phase)

- Full business feature implementation
- Advanced production infra, CI/CD, and observability
- Microservices split
- AI/ML features
- Complex realtime collaboration
- Enterprise-grade deep permission matrix
- Deep performance optimization without baseline measurements

## Definition of Done (Foundation)

- Output aligns with requirements
- Related tests pass (when available in scope)
- No module boundary violations
- Documentation updated
- Review completed

## First Startup Command

When ready, start with:

```text
Read kickoff-prompt.foundation.md and execute accordingly.
```
