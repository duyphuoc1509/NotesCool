import api from '../../../services/api'
import type {
  ChangeTaskStatusRequest,
  CreateTaskRequest,
  TaskDto,
  TaskStatus,
  TasksPagedResult,
  UpdateTaskRequest,
} from '../types'

const TASKS_BASE_PATH = '/api/tasks'

export const tasksApi = {
  async getTasks(status?: TaskStatus, page = 1, pageSize = 10): Promise<TasksPagedResult<TaskDto>> {
    const { data } = await api.get<TasksPagedResult<TaskDto>>(TASKS_BASE_PATH, {
      params: { status, page, pageSize },
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

  async deleteTask(id: string): Promise<void> {
    await api.delete(`${TASKS_BASE_PATH}/${id}`)
  },
}
