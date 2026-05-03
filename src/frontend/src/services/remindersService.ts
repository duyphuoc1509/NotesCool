import api from './api'
import type { ReminderConfigDto, ReminderDto } from '../types/task'

const REMINDERS_BASE_PATH = (taskId: string) => `/api/tasks/${taskId}/reminders`

interface UpsertTaskRemindersRequest {
  taskTitle: string
  dueDateUtc: string
  reminders: ReminderConfigDto[]
}

export const remindersService = {
  async getReminders(taskId: string): Promise<ReminderDto[]> {
    const { data } = await api.get<ReminderDto[]>(REMINDERS_BASE_PATH(taskId))
    return data
  },

  async upsertReminders(taskId: string, payload: UpsertTaskRemindersRequest): Promise<ReminderDto[]> {
    const { data } = await api.put<ReminderDto[]>(REMINDERS_BASE_PATH(taskId), payload)
    return data
  },

  async deleteReminders(taskId: string): Promise<void> {
    await api.delete(REMINDERS_BASE_PATH(taskId))
  },
}

export default remindersService
