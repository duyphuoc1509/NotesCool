export type TaskStatus = 'Todo' | 'InProgress' | 'InReview' | 'Done' | 'Blocked' | 'Archived'
export type TaskPriority = 'Low' | 'Medium' | 'High' | 'Urgent'

export type ReminderStatus = 'Pending' | 'Synced' | 'Failed' | 'Canceled'

export interface TaskAssigneeDto {
  id: string
  displayName: string
  email?: string
}

export interface ActivityLogDto {
  id: string
  taskId: string
  actorName?: string
  message: string
  createdAt: string
}

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
  priority?: TaskPriority
  dueDate?: string
  createdAt: string
  updatedAt?: string
  isFavorite?: boolean
  reminders?: ReminderDto[]
  assignees?: TaskAssigneeDto[]
  subTasksCount?: number
  subTasksCompleted?: number
}

export interface CreateTaskRequest {
  title: string
  description?: string
  status?: TaskStatus
  priority?: TaskPriority
  dueDate?: string
  reminders?: ReminderConfigDto[]
  assigneeIds?: string[]
}

export interface UpdateTaskRequest {
  title: string
  description?: string
  status?: TaskStatus
  priority?: TaskPriority
  dueDate?: string
  reminders?: ReminderConfigDto[]
  assigneeIds?: string[]
}

export interface ChangeTaskStatusRequest {
  status: TaskStatus
}

export interface SetTaskFavoriteRequest {
  isFavorite: boolean
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
  priority?: TaskPriority
  assigneeId?: string
  keyword?: string
  page?: number
  pageSize?: number
}
