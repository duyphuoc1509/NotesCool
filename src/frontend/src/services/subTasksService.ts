import api from './api'
import type {
  CreateSubTaskRequest,
  SubTaskDto,
  UpdateSubTaskRequest,
} from '../types/subtask'

const SUBTASKS_BASE_PATH = (taskId: string) => `/api/tasks/${taskId}/subtasks`

export const subTasksService = {
  async getSubTasks(taskId: string): Promise<SubTaskDto[]> {
    const { data } = await api.get<SubTaskDto[]>(SUBTASKS_BASE_PATH(taskId))
    return data
  },

  async createSubTask(taskId: string, payload: CreateSubTaskRequest): Promise<SubTaskDto> {
    const { data } = await api.post<SubTaskDto>(SUBTASKS_BASE_PATH(taskId), payload)
    return data
  },

  async updateSubTask(taskId: string, subTaskId: string, payload: UpdateSubTaskRequest): Promise<SubTaskDto> {
    const { data } = await api.put<SubTaskDto>(`${SUBTASKS_BASE_PATH(taskId)}/${subTaskId}`, payload)
    return data
  },

  async deleteSubTask(taskId: string, subTaskId: string): Promise<void> {
    await api.delete(`${SUBTASKS_BASE_PATH(taskId)}/${subTaskId}`)
  },
}

export default subTasksService
