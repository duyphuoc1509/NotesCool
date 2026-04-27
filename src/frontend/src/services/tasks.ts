import api from './api'
import type {
  ChangeTaskStatusRequest,
  CreateTaskRequest,
  PagedResult,
  TaskDto,
  TasksFilter,
  UpdateTaskRequest,
} from '../types/task'

const TASKS_PATH = '/api/tasks'

export async function getTasks(filter: TasksFilter = {}): Promise<PagedResult<TaskDto>> {
  const response = await api.get<PagedResult<TaskDto>>(TASKS_PATH, {
    params: {
      status: filter.status,
      page: filter.page ?? 1,
      pageSize: filter.pageSize ?? 20,
    },
  })

  return response.data
}

export async function createTask(payload: CreateTaskRequest): Promise<TaskDto> {
  const response = await api.post<TaskDto>(TASKS_PATH, payload)
  return response.data
}

export async function updateTask(id: string, payload: UpdateTaskRequest): Promise<TaskDto> {
  const response = await api.put<TaskDto>(`${TASKS_PATH}/${id}`, payload)
  return response.data
}

export async function changeTaskStatus(
  id: string,
  payload: ChangeTaskStatusRequest
): Promise<TaskDto> {
  const response = await api.patch<TaskDto>(`${TASKS_PATH}/${id}/status`, payload)
  return response.data
}

export async function deleteTask(id: string): Promise<void> {
  await api.delete(`${TASKS_PATH}/${id}`)
}
