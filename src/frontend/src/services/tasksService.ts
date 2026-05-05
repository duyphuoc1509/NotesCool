import api from './api'
import type {
  ActivityLogDto,
  ChangeTaskStatusRequest,
  CreateTaskRequest,
  SetTaskFavoriteRequest,
  TaskDto,
  TaskStatus,
  UpdateTaskRequest,
} from '../types/task'
import type { PagedResult } from '../types/common'

const TASKS_BASE_PATH = '/api/tasks'

export const tasksService = {
  async getTasks(
    status?: TaskStatus,
    priority?: string,
    assigneeId?: string,
    keyword?: string,
    page = 1,
    pageSize = 10,
  ): Promise<PagedResult<TaskDto>> {
    const { data } = await api.get<PagedResult<TaskDto>>(TASKS_BASE_PATH, {
      params: { status, priority, assigneeId, keyword, page, pageSize },
    })
    return data
  },

  async getTaskById(id: string): Promise<TaskDto> {
    const { data } = await api.get<TaskDto>(`${TASKS_BASE_PATH}/${id}`)
    return data
  },

  async createTask(payload: CreateTaskRequest): Promise<TaskDto> {
    const { data } = await api.post<TaskDto>(TASKS_BASE_PATH, payload)
    return data
  },

  async updateTask(id: string, payload: UpdateTaskRequest): Promise<TaskDto> {
    const { data } = await api.put<TaskDto>(`${TASKS_BASE_PATH}/${id}`, payload)
    return data
  },

  async changeTaskStatus(id: string, payload: ChangeTaskStatusRequest): Promise<TaskDto> {
    const { data } = await api.patch<TaskDto>(`${TASKS_BASE_PATH}/${id}/status`, payload)
    return data
  },

  async setFavorite(id: string, payload: SetTaskFavoriteRequest): Promise<TaskDto> {
    const { data } = await api.patch<TaskDto>(`${TASKS_BASE_PATH}/${id}/favorite`, payload)
    return data
  },

  async deleteTask(id: string): Promise<void> {
    await api.delete(`${TASKS_BASE_PATH}/${id}`)
  },

  async getActivityLog(id: string): Promise<ActivityLogDto[]> {
    const { data } = await api.get<ActivityLogDto[]>(`${TASKS_BASE_PATH}/${id}/activity`)
    return data
  },
}

export default tasksService
