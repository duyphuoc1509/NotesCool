# QC Test Report — US-017 SSO Flows (OSI-58)

## Scope

Validate MVP2 Identity/SSO flows covering:

- SSO provider login success and callback failure handling
- Provider identity to local account mapping
- Account linking and unlinking rules
- Duplicate provider identity protection
- Provider configuration success/failure behavior

## Execution Status

**Status:** Blocked for runtime execution.

The repository branch available to QC does not currently contain implemented SSO backend endpoints or account-management UI to execute against. The related implementation issues are still not complete at the time of this report:

- OSI-43 — SSO login callback backend: `todo`
- OSI-44 — SSO account linking: `todo`
- OSI-45 — SSO account unlinking rules: `todo`
- OSI-46 — SSO provider configuration validation: `in_progress`
- OSI-48 — Login page with SSO options: `in_progress`
- OSI-51 — Account SSO management UI: `in_progress`

No executable SSO code paths were found in `src/backend` for OAuth/OIDC callback, provider identity persistence, link, or unlink operations.

## Test Matrix

| ID | Area | Scenario | Expected Result | Result |
| --- | --- | --- | --- | --- |
| SSO-001 | Provider login success | Start Google/OIDC login and complete callback with valid state/code | Local user is created or matched; access/refresh tokens issued; user lands in authenticated app | Blocked — endpoint/UI unavailable |
| SSO-002 | Provider login existing mapping | Login with a provider identity already mapped to a local account | Same local account is used; no duplicate user is created | Blocked — mapping store unavailable |
| SSO-003 | Invalid callback state | Callback with missing/invalid/tampered `state` | Request rejected with safe 400/401; no account mapping created | Blocked — callback unavailable |
| SSO-004 | Invalid callback code | Callback with missing/invalid/replayed `code` | Request rejected; provider error handled without leaking secrets | Blocked — callback unavailable |
| SSO-005 | Provider error callback | Callback includes provider `error` / `error_description` | User sees clear failure; server logs sanitized event | Blocked — callback unavailable |
| SSO-006 | Provider config missing | Provider client id/secret/redirect config missing | Provider is disabled or app fails fast clearly; no secrets exposed | Blocked — config validation in progress |
| SSO-007 | Link provider success | Authenticated user links a new provider identity | Provider appears on account; mapping points to current user | Blocked — link endpoint/UI unavailable |
| SSO-008 | Duplicate provider link | User B attempts to link provider identity already linked to User A | Rejected with conflict/validation error; original mapping unchanged | Blocked — link endpoint unavailable |
| SSO-009 | Re-link same provider | Same user links an already-linked provider identity | Idempotent success or clear duplicate message; no duplicate mapping row | Blocked — link endpoint unavailable |
| SSO-010 | Unlink provider success | User with password or another provider unlinks one provider | Provider removed; remaining login method still works | Blocked — unlink endpoint/UI unavailable |
| SSO-011 | Unlink last login method | User tries to unlink only login method | Rejected; account cannot be locked out | Blocked — unlink endpoint unavailable |
| SSO-012 | Account mapping integrity | Execute login/link/unlink sequence and inspect identity mappings | No orphaned, duplicate, or cross-user mappings | Blocked — persistence model unavailable |

## Findings

### F-001 — SSO flows cannot be executed yet

- **Severity:** High
- **Category:** Test blocker / dependency
- **Observed:** SSO login callback, link, unlink, and provider identity mapping implementations are not available in the current repo state.
- **Impact:** Acceptance criteria for US-017 cannot be fully validated until the SSO backend and UI work is completed.
- **Recommendation:** Re-run this QC matrix after OSI-43, OSI-44, OSI-45, OSI-46, OSI-48, and OSI-51 are merged into `dev`.

## Regression Areas to Re-test Once Unblocked

- Existing email/password login still works after SSO login additions.
- Existing protected Notes/Tasks APIs still use the same local user identity after SSO login.
- Token issuance and refresh behavior is identical for password and SSO-authenticated users.
- Error responses do not leak provider secrets, auth codes, refresh tokens, or stack traces.
- Audit logs include auth event type, user id/provider, timestamp, and outcome without sensitive values.

## Exit Criteria for Re-run

US-017 can be marked passed only when:

1. At least one configured provider happy path succeeds end-to-end.
2. Invalid callback state/code/provider-error paths are rejected safely.
3. Provider identity maps to exactly one local account.
4. Duplicate provider identity linking is rejected.
5. Unlinking is allowed only when another login method remains.
6. UI reflects linked providers and unlink constraints clearly.
7. Regression checks for password auth and protected API access pass.
