import { tasksApi } from '../infrastructure/tasksApi'
import type {
  ChangeTaskStatusRequest,
  CreateTaskRequest,
  TaskDto,
  TaskStatus,
  TasksPagedResult,
  UpdateTaskRequest,
} from '../types'

export const tasksService = {
  getTasks(status?: TaskStatus, page = 1, pageSize = 10): Promise<TasksPagedResult<TaskDto>> {
    return tasksApi.getTasks(status, page, pageSize)
  },

  getTaskById(id: string): Promise<TaskDto> {
    return tasksApi.getTaskById(id)
  },

  createTask(payload: CreateTaskRequest): Promise<TaskDto> {
    return tasksApi.createTask(payload)
  },

  updateTask(id: string, payload: UpdateTaskRequest): Promise<TaskDto> {
    return tasksApi.updateTask(id, payload)
  },

  changeTaskStatus(id: string, payload: ChangeTaskStatusRequest): Promise<TaskDto> {
    return tasksApi.changeTaskStatus(id, payload)
  },

  deleteTask(id: string): Promise<void> {
    return tasksApi.deleteTask(id)
  },
}
