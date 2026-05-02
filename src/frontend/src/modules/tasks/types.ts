export type TaskStatus = 'Todo' | 'InProgress' | 'InReview' | 'Done' | 'Blocked' | 'Archived'

export interface TaskDto {
  id: string
  title: string
  description?: string
  status: TaskStatus
  dueDate?: string
  createdAt: string
  updatedAt?: string
}

export interface CreateTaskRequest {
  title: string
  description?: string
  dueDate?: string
}

export interface UpdateTaskRequest {
  title: string
  description?: string
  dueDate?: string
}

export interface ChangeTaskStatusRequest {
  status: TaskStatus
}

export interface TasksPagedResult<T> {
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
