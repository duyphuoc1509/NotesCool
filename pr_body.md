## Issue

Issue code: OSI-55

## Summary

- Triển khai cơ chế ghi log kiểm tra bảo mật (security audit logging) cho các sự kiện xác thực.
- Tích hợp ghi log vào luồng Login, Logout, Refresh Token và SSO Callback/Link/Unlink.

## Changes

- Thêm \`ISecurityAuditService\` và \`SecurityAuditService\` trong \`NotesCool.Shared.Security\`.
- Cập nhật \`AuthEndpoints\` để ghi log sự kiện \`LOGIN_SUCCESS\`, \`LOGIN_FAILED\`, \`LOGOUT\`, \`REFRESH_TOKEN\`.
- Cập nhật \`SsoEndpoints\` để ghi log sự kiện \`SSO_CALLBACK\`, \`SSO_LINK\`, \`SSO_UNLINK\`.
- Đồng bộ code từ các feature branch auth (\`OSI-41\`, \`OSI-43\`) vào \`dev\` (trong phạm vi task này) để đảm bảo tính nhất quán của hệ thống.

## Test

- Đã chạy unit test \`AuthEndpointsTests\` và kiểm tra log output:
  - \`LOGIN_SUCCESS\` -> Passed
  - \`LOGOUT\` -> Passed
  - \`REFRESH_TOKEN\` (success/failed) -> Passed
- Lệnh chạy: \`dotnet test --filter AuthEndpointsTests\`

## Review Note

PR này merge vào \`dev\` và đang chờ review. Vì \`dev\` hiện tại chưa có đầy đủ logic auth (đang nằm ở các PR khác), PR này đã bao gồm các file auth cần thiết để đảm bảo build success và test chạy được.
