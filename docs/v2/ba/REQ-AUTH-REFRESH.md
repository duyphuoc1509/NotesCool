# Requirement: Xử lý AccessToken khi hết hạn (Refresh Token Flow)

## 1. Overview

Yêu cầu thực hiện luồng cấp lại Access Token khi hết hạn thông qua Refresh Token.
Nếu Refresh Token cũng hết hạn hoặc không hợp lệ, hệ thống sẽ tự động đăng xuất và điều hướng người dùng về trang Login.
Tính năng này đòi hỏi sự phối hợp giữa Frontend (React) và Backend (.NET).

## 2. Business Goal

Đảm bảo trải nghiệm người dùng không bị gián đoạn khi Access Token (có thời gian sống ngắn) hết hạn, đồng thời giữ vững tính bảo mật bằng cách yêu cầu đăng nhập lại khi Refresh Token (có thời gian sống dài) hết hạn.

## 3. Actors

- User (đã đăng nhập)
- System (Frontend, Backend)

## 4. Current Flow

- Frontend đang có interceptor bắt lỗi 401 và gọi `POST /api/auth/refresh-token`.
- Backend đã triển khai endpoint refresh token tại `POST /api/auth/refresh` và trả về `AuthResponse` gồm `AccessToken`, `RefreshToken`.
- Đang có sự sai lệch về URL endpoint giữa FE (`/api/auth/refresh-token`) và BE (`/api/auth/refresh`).
- Quản lý phiên bản trên FE trong `AuthContext.tsx` cần được đảm bảo hoạt động trơn tru sau khi token được làm mới.

## 5. Proposed Flow

1. Người dùng thao tác, gọi API cần xác thực.
2. Access Token hết hạn, Backend trả về lỗi `401 Unauthorized`.
3. Frontend (Axios Interceptor) bắt lỗi `401`, chặn các request đang chờ (queue).
4. Frontend gửi request làm mới token đến `POST /api/auth/refresh` với payload là `refreshToken` hiện tại đang lưu.
5. Backend kiểm tra `refreshToken`:
   - Nếu hợp lệ: Trả về HTTP 200 kèm `AccessToken` và `RefreshToken` mới.
   - Nếu không hợp lệ/hết hạn: Trả về HTTP 401.
6. Frontend nhận phản hồi:
   - Nếu thành công: Lưu token mới vào localStorage, cập nhật header Authorization, gọi lại các API bị chặn trong queue và tiếp tục.
   - Nếu thất bại (lỗi 401 hoặc lỗi refresh): Xóa trắng localStorage (xóa tokens), cập nhật state về unauthenticated, chuyển hướng người dùng về trang `/login`.

## 6. Functional Requirements

### FR-001 (Frontend): Đồng bộ URL Endpoint Refresh Token
Frontend cần gọi đúng endpoint của backend là `POST /api/auth/refresh` (thay vì `/api/auth/refresh-token`) trong file `src/services/api.ts`.

### FR-002 (Frontend): Xử lý Refresh Token bị lỗi hoặc hết hạn
Khi gọi API `/api/auth/refresh` bị lỗi (VD: 401, timeout), axios interceptor cần xóa session (gọi `clearStoredSession`), và ép trình duyệt chuyển hướng về trang đăng nhập `/login`.

### FR-003 (Frontend): Ngăn chặn refresh vô hạn
Cần đảm bảo logic interceptor không tạo ra vòng lặp vô hạn (infinite loop) nếu API `/api/auth/refresh` cũng trả về 401.

### FR-004 (Backend): Xác minh Backend Refresh Endpoint hoạt động chính xác
Đảm bảo API `/api/auth/refresh` đã sẵn sàng, nhận đúng payload `{"refreshToken": "..."}` và trả về đúng JSON structure được mong đợi:
```json
{
  "accessToken": "...",
  "refreshToken": "...",
  "tokenType": "Bearer",
  "accessTokenExpiresInSeconds": 900,
  "accessTokenExpiresAtUtc": "..."
}
```

## 7. Business Rules

### BR-001
Access Token có thời hạn sử dụng ngắn (15 phút). Refresh Token có thời hạn dài hơn, dùng để đổi lấy Access Token mới.

### BR-002
Chỉ thực hiện refresh token khi có request API thực tế bị trả về 401. Các request khác xảy ra trong quá trình đang refresh token phải được đưa vào hàng đợi (queue) và thực thi lại sau khi có token mới.

## 8. State / Status Rules

| Current Status | Action | Next Status | Note |
|---|---|---|---|
| Authenticated | Receive 401 | Refreshing | Interceptor pauses incoming requests |
| Refreshing | Refresh Success | Authenticated | Queued requests are retried |
| Refreshing | Refresh Failed | Unauthenticated | Redirect to /login, data cleared |

## 9. Acceptance Criteria

### AC-001: Refresh Token thành công
Given Người dùng đang đăng nhập và Access Token đã hết hạn, Refresh Token vẫn còn hạn
When Người dùng gọi một API có yêu cầu xác thực
Then Hệ thống gọi ngầm API làm mới token thành công
And Người dùng không bị đăng xuất
And Request API ban đầu tiếp tục thành công với token mới.

### AC-002: Refresh Token thất bại (hết hạn hoặc không hợp lệ)
Given Refresh Token đã hết hạn hoặc bị thu hồi (revoked) trên server
When Người dùng gọi API và hệ thống cố gắng làm mới token
Then API làm mới token trả về 401
And Hệ thống xóa thông tin token trên local
And Người dùng bị chuyển hướng về màn hình đăng nhập.

### AC-003: Xử lý nhiều request cùng lúc
Given Access token hết hạn
When Hệ thống gửi nhiều request API xác thực cùng một lúc
Then Chỉ có MỘT request làm mới token được gửi đi
And Các request xác thực khác chờ request làm mới token hoàn thành rồi mới dùng token mới để gọi lại thành công.

## 10. Notes for Dev

- **Frontend (React)**: 
  - File chính cần sửa: `src/frontend/src/services/api.ts` (Sửa lại đường dẫn API từ `/api/auth/refresh-token` thành `/api/auth/refresh`).
  - Kiểm tra hàm error handler trong axios interceptor: Cần đảm bảo nếu API refresh trả về lỗi thì phải gọi `window.location.assign('/login')` hoặc tương tự sau khi clear session.
- **Backend (.NET)**:
  - Code BE đã có sẵn API `POST /api/auth/refresh`. Tuy nhiên, BE Developer cần review qua `AuthEndpoints.cs` và `InMemoryRefreshTokenStore` xem logic thu hồi token và khởi tạo token đã được tích hợp đúng với luồng FE mong đợi hay chưa. 
  - Yêu cầu đảm bảo khi đăng nhập thành công trả về đúng object DTO chứa `RefreshToken` để FE lưu lại.

## 11. Notes for QC

- Cần mô phỏng việc Access Token hết hạn trên trình duyệt (thay đổi token bằng tay trong `localStorage` hoặc chỉnh lại thời gian máy).
- Sử dụng Network tab (DevTools) để kiểm chứng interceptor bắt 401, gọi API `/api/auth/refresh`, sau đó retry API cũ (với status 200).
- Cố tình sửa đổi Refresh Token thành chuỗi sai và gửi request, kiểm chứng hệ thống phải redirect về `/login`.
