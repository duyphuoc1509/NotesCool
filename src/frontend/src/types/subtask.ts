export type SubTaskStatus = 'Todo' | 'InProgress' | 'Done'

export interface SubTaskDto {
  id: string
  parentTaskId: string
  title: string
  status: SubTaskStatus
  assigneeId?: string
  createdAt: string
  updatedAt?: string
}

export interface CreateSubTaskRequest {
  title: string
  assigneeId?: string
}

export interface UpdateSubTaskRequest {
  title: string
  status: SubTaskStatus
  assigneeId?: string
}
