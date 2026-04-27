export type TaskStatus = 'Todo' | 'InProgress' | 'Done'

export interface Task {
  id: string
  title: string
  description?: string
  status: TaskStatus
  dueDate?: string
  createdAt: string
  updatedAt?: string
}

export interface TaskDto extends Task {}

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
