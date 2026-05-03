# Requirement: Reminder Calendar Plugin

## 1. Overview

Thiết kế module nhắc lịch gần tới hạn task dưới dạng plugin độc lập. Plugin này nhận thông tin từ Task module, tạo nhiều reminder cho mỗi task và đồng bộ các reminder vào ứng dụng Lịch native trên iPhone/Android theo phạm vi account.

Issue: OSI-153

## 2. Business Goal

Giúp user không bỏ lỡ task gần đến hạn bằng cách đưa reminder vào lịch native của thiết bị. Việc tách thành plugin giúp hệ thống dễ bật/tắt, mở rộng và bảo trì mà không làm Task module phụ thuộc trực tiếp vào logic lịch/notification của mobile OS.

## 3. Actors

- User: tạo/sửa task, cấu hình reminder, nhận nhắc lịch trên thiết bị.
- System: phát hiện thay đổi task, đồng bộ reminder, hủy/reschedule khi cần.
- Task Module: nguồn dữ liệu task và due date.
- Reminder Plugin: quản lý reminder, sync lịch native, trạng thái đồng bộ.
- Mobile Device: iPhone/Android cài app và có quyền truy cập lịch.
- Native Calendar App: Calendar trên iOS/Android nhận event/reminder được sync.

## 4. Current Flow

Hiện tại task có due date nhưng chưa có plugin riêng để:

1. Theo dõi task sắp đến hạn.
2. Tạo nhiều mốc nhắc cho cùng một task.
3. Đồng bộ nhắc lịch sang ứng dụng Lịch native trên iPhone/Android.
4. Hủy/reschedule lịch khi task thay đổi hoặc hoàn thành.

## 5. Proposed Flow

### 5.1 Create task with reminders

1. User tạo task có due date.
2. User chọn một hoặc nhiều reminder offsets, ví dụ: 1 ngày trước hạn, 1 giờ trước hạn, đúng giờ đến hạn.
3. Task Module lưu task.
4. Task Module phát event `TaskCreated` hoặc `TaskDueDateChanged`.
5. Reminder Plugin nhận event.
6. Reminder Plugin tính các reminder time từ due date theo mốc gốc UTC+7.
7. Reminder Plugin lưu reminder records theo account.
8. Mobile app đồng bộ reminder records và tạo/sync event vào Lịch native trên từng thiết bị cùng account.

### 5.2 Update due date or reminders

1. User sửa due date hoặc danh sách reminder.
2. Task Module phát event thay đổi.
3. Reminder Plugin recalculates reminder times.
4. Existing native calendar events/reminders được update hoặc replace.
5. Tất cả thiết bị cùng account phải nhận trạng thái sync mới.

### 5.3 Complete/delete task or remove due date

1. User hoàn thành/xóa task hoặc xóa due date.
2. Task Module phát event.
3. Reminder Plugin cancel tất cả pending reminders của task.
4. Native calendar events/reminders liên quan bị xóa hoặc đánh dấu canceled trên tất cả thiết bị cùng account.

## 6. Functional Requirements

### FR-001: Plugin boundary

Reminder capability phải được triển khai như plugin riêng, không nhúng trực tiếp business logic calendar/reminder vào Task module.

### FR-002: Account-level reminder scope

Reminder được quản lý theo account. Nếu user đăng nhập cùng account trên nhiều thiết bị, tất cả thiết bị đủ quyền calendar phải sync và nhận reminder.

### FR-003: Native calendar sync in Phase 1

Phase 1 bắt buộc đồng bộ vào ứng dụng Lịch native trên iPhone/Android. Chỉ notification trong app là không đủ để đạt scope.

### FR-004: Multiple reminders per task

Một task được phép có nhiều reminder. Mỗi reminder có offset/time riêng.

### FR-005: Due date dependency

Chỉ task có due date hợp lệ mới được tạo reminder. Nếu task không có due date, Reminder Plugin không được tạo calendar reminder.

### FR-006: Timezone policy

Nguồn chuẩn khi tính reminder là UTC+7. Khi sync sang thiết bị, event/reminder phải hiển thị và chạy theo local timezone của thiết bị, nhưng mốc gốc được tạo từ UTC+7.

### FR-007: Reminder lifecycle sync

Reminder Plugin phải xử lý create/update/delete/cancel khi task created, due date changed, reminder list changed, task completed, task deleted hoặc due date removed.

### FR-008: Calendar permission handling

Nếu thiết bị chưa cấp quyền Calendar, app phải hiển thị trạng thái/CTA để user cấp quyền. Không được silent fail.

### FR-009: Deep link behavior

Calendar event/reminder nên chứa thông tin đủ để mở lại đúng task trong app nếu nền tảng hỗ trợ link/URL.

### FR-010: Sync state visibility

Hệ thống cần có trạng thái sync tối thiểu cho reminder/calendar event: pending, synced, failed, canceled.

## 7. Business Rules

### BR-001: Task due date required

Reminder chỉ tồn tại khi task có due date.

### BR-002: Multiple reminders allowed

Một task có thể có nhiều reminder, nhưng mỗi reminder phải có mốc nhắc riêng hoặc định danh riêng để tránh trùng sync.

### BR-003: Account-level propagation

Reminder thay đổi trên một thiết bị phải được áp dụng cho toàn account và đồng bộ sang các thiết bị khác.

### BR-004: UTC+7 source of truth

Due date/reminder calculation lấy UTC+7 làm mốc nghiệp vụ gốc.

### BR-005: Device local display/execution

Sau khi mốc UTC+7 được xác định, lịch native trên thiết bị hiển thị/chạy theo local timezone của thiết bị.

### BR-006: Completed or deleted task cancels reminders

Task completed/deleted phải hủy tất cả reminder/calendar events liên quan.

### BR-007: Due date change reschedules reminders

Khi due date thay đổi, reminder times phải được tính lại và calendar events phải được update/resync.

### BR-008: Removed due date cancels reminders

Nếu due date bị xóa, tất cả reminder của task bị cancel.

### BR-009: Permission denial does not block task usage

User vẫn dùng task bình thường nếu từ chối quyền Calendar; hệ thống chỉ đánh dấu reminder sync failed/pending permission.

### BR-010: Duplicate prevention

Hệ thống phải tránh tạo duplicate calendar events trên cùng thiết bị khi sync lại nhiều lần.

## 8. Validation Rules

| Field | Rule | Error Message |
|---|---|---|
| dueDate | Required if reminders are configured | Due date is required to create reminders. |
| reminders | Optional list; can contain multiple reminder items | Reminder list is invalid. |
| reminderTime/offset | Must resolve to valid datetime | Reminder time is invalid. |
| reminderTime/offset | Should be before or equal to dueDate depending on selected option | Reminder must not be after due date. |
| timezoneSource | Must default to UTC+7 | Timezone source is invalid. |
| taskId | Required | Task is required. |
| accountId | Required | Account is required. |
| calendarPermission | Required on device before native sync | Calendar permission is required to sync reminders. |

## 9. State / Status Rules

| Current Status | Action | Next Status | Note |
|---|---|---|---|
| none | Task with due date + reminders created | pending | Waiting for sync. |
| pending | Device calendar sync succeeds | synced | Per device/calendar sync record. |
| pending | Calendar permission missing | failed | Failure reason: permission_required. |
| synced | Due date/reminder changed | pending | Resync required. |
| synced | Task completed/deleted | canceled | Calendar event should be removed/canceled. |
| synced | Due date removed | canceled | Calendar event should be removed/canceled. |
| failed | User grants permission and retry succeeds | synced | Retry from device/app. |
| failed | Retry fails | failed | Keep reason for troubleshooting. |

## 10. Permission Rules

| Role | Action | Allowed |
|---|---|---|
| Task owner | Configure reminders for own task | Yes |
| Task owner | Sync reminders to own account devices | Yes |
| Other user | Configure reminders for another user's private task | No |
| System | Create/update/cancel reminder from task events | Yes |
| Device without calendar permission | Write native calendar event | No |
| Device with calendar permission | Write native calendar event | Yes |

## 11. Acceptance Criteria

### AC-001: Create multiple reminders for a task

Given user has a task with due date
And user adds multiple reminder times
When user saves the task
Then Reminder Plugin creates all reminder records for that task
And each reminder is associated with the user's account.

### AC-002: Sync reminders to native calendar

Given a task has reminders
And the user's iPhone/Android device has calendar permission
When sync runs
Then native calendar events/reminders are created on the device
And each event maps back to the correct task/reminder.

### AC-003: Account-level sync across devices

Given user logs in on multiple devices with the same account
And each device has calendar permission
When a task reminder is created or updated
Then all eligible devices sync the updated reminder.

### AC-004: Timezone behavior

Given due date/reminder is calculated from UTC+7
And device timezone is different from UTC+7
When reminder is synced to native calendar
Then the calendar displays/runs according to the device local timezone
And the underlying source time is derived from UTC+7.

### AC-005: Update due date reschedules reminders

Given a task has existing synced reminders
When user changes the due date
Then Reminder Plugin recalculates reminder times
And existing native calendar reminders are updated, not duplicated.

### AC-006: Complete task cancels reminders

Given a task has pending or synced reminders
When user marks the task as completed
Then all related reminders are canceled
And native calendar events are removed or canceled on synced devices.

### AC-007: Delete task cancels reminders

Given a task has reminders
When user deletes the task
Then all related reminder records are canceled
And native calendar events are removed or canceled.

### AC-008: Remove due date cancels reminders

Given a task has due date and reminders
When user removes the due date
Then all reminders for that task are canceled
And no native calendar reminder remains active.

### AC-009: Missing calendar permission

Given user creates reminders
And device has not granted calendar permission
When sync runs
Then app does not silently fail
And user sees permission guidance
And sync state is marked failed or pending permission.

### AC-010: Duplicate prevention

Given sync retries multiple times for the same reminder
When sync succeeds
Then only one native calendar event exists per reminder per device.

## 12. Edge Cases

- Due date is in the past.
- Reminder offset resolves to time in the past.
- Device timezone differs from UTC+7.
- User changes device timezone after sync.
- Calendar permission revoked after previous successful sync.
- User has multiple devices with mixed permission states.
- App offline during due date/reminder update.
- Sync retries after partial success.
- Task completed before reminder fires.
- Task deleted while device is offline.
- Same reminder synced twice due to retry/race condition.
- Native calendar event manually deleted by user outside app.

## 13. Notes for Dev

- Task module should publish events; Reminder Plugin should own reminder/calendar logic.
- Suggested task events:
  - `TaskCreated`
  - `TaskUpdated`
  - `TaskDueDateChanged`
  - `TaskCompleted`
  - `TaskDeleted`
  - `TaskDueDateRemoved`
- Reminder data should include at minimum:
  - reminderId
  - taskId
  - accountId
  - dueDateSourceTimezone: UTC+7
  - reminderTimeUtc
  - reminderOffset/value
  - status
  - sync states per device if applicable
- Native calendar sync needs idempotency key per account/device/task/reminder.
- Calendar event should include task title, due date, optional description and deep link if supported.
- Backend should not assume every device can sync; permission is device-level.
- Account-level ownership does not mean every device succeeds; sync status may differ per device.

## 14. Notes for QC

QC should test at least:

- Create task with one reminder.
- Create task with multiple reminders.
- Sync to iPhone native calendar.
- Sync to Android native calendar.
- Same account on multiple devices.
- Device with calendar permission denied.
- Device timezone UTC+7.
- Device timezone different from UTC+7.
- Change due date and verify no duplicate events.
- Complete task and verify calendar events removed/canceled.
- Delete task and verify calendar events removed/canceled.
- Remove due date and verify reminders canceled.
- Retry sync after permission granted.
- Offline device sync after reconnect.

## 15. Open Questions

None blocking after user clarification.

Confirmed decisions:

- Reminder scope: account-level.
- Phase 1 channel: native calendar sync on iPhone/Android.
- Timezone source: UTC+7.
- Runtime/display timezone: device local timezone.
- Multiple reminders per task: allowed.
