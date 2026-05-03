export type TaskStatus = 'Todo' | 'InProgress' | 'InReview' | 'Done' | 'Blocked' | 'Archived'

export type ReminderStatus = 'Pending' | 'Synced' | 'Failed' | 'Canceled'

export interface ReminderDto {
  id: string
  taskId: string
  offsetMinutes: number
  reminderTimeUtc: string
  status: ReminderStatus
}

export interface ReminderConfigDto {
  offsetMinutes: number
}

export interface TaskDto {
  id: string
  title: string
  description?: string
  status: TaskStatus
  dueDate?: string
  createdAt: string
  updatedAt?: string
  reminders?: ReminderDto[]
}

export interface CreateTaskRequest {
  title: string
  description?: string
  dueDate?: string
  reminders?: ReminderConfigDto[]
}

export interface UpdateTaskRequest {
  title: string
  description?: string
  dueDate?: string
  reminders?: ReminderConfigDto[]
}

export interface ChangeTaskStatusRequest {
  status: TaskStatus
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}

export interface TasksFilter {
  status?: TaskStatus
  page?: number
  pageSize?: number
}
