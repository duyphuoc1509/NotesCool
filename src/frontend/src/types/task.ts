export type TaskStatus = 'todo' | 'in_progress' | 'done' | 'archived'

export interface TaskDto {
  id: string
  title: string
  description?: string | null
  status: TaskStatus
  dueDate?: string | null
  createdAt: string
  updatedAt?: string | null
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
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

export interface TasksFilter {
  status?: TaskStatus
  page?: number
  pageSize?: number
}
