import { useState, useEffect, useCallback } from 'react'
import axios from 'axios'
import type {
  TaskDto,
  TaskStatus,
  TasksFilter,
  CreateTaskRequest,
  UpdateTaskRequest,
  ReminderDto,
} from '../types/task'
import { tasksService } from '../services/tasksService'
import { remindersService } from '../services/remindersService'

function mergeTaskReminders(items: TaskDto[], taskId: string, reminders: ReminderDto[]): TaskDto[] {
  return items.map((task) => (task.id === taskId ? { ...task, reminders } : task))
}

export function useTasks(initialFilter: TasksFilter = {}) {
  const [tasks, setTasks] = useState<TaskDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [totalCount, setTotalCount] = useState(0)
  const [filter, setFilter] = useState<TasksFilter>(initialFilter)

  const fetchTasks = useCallback(async () => {
    setIsLoading(true)
    setError(null)
    try {
      const data = await tasksService.getTasks(filter.status, filter.page, filter.pageSize)
      const itemsWithReminders = await Promise.all(
        data.items.map(async (task) => {
          if (!task.dueDate) {
            return { ...task, reminders: [] }
          }

          try {
            const reminders = await remindersService.getReminders(task.id)
            return { ...task, reminders }
          } catch {
            return { ...task, reminders: [] }
          }
        }),
      )

      setTasks(itemsWithReminders)
      setTotalCount(data.totalCount)
    } catch (err) {
      if (axios.isAxiosError(err)) {
        setError(err.response?.data?.message || err.message)
      } else {
        setError('Failed to fetch tasks')
      }
    } finally {
      setIsLoading(false)
    }
  }, [filter])

  useEffect(() => {
    fetchTasks()
  }, [fetchTasks])

  const handleCreateTask = async (payload: CreateTaskRequest) => {
    try {
      const { reminders, ...taskPayload } = payload
      const newTask = await tasksService.createTask(taskPayload)

      if (reminders?.length && newTask.dueDate) {
        const createdReminders = await remindersService.upsertReminders(newTask.id, {
          taskTitle: newTask.title,
          dueDateUtc: newTask.dueDate,
          reminders,
        })
        newTask.reminders = createdReminders
      }

      await fetchTasks()
      return newTask
    } catch (err) {
      if (axios.isAxiosError(err)) {
        setError(err.response?.data?.message || err.message)
      } else {
        setError('Failed to create task')
      }
      throw err
    }
  }

  const handleUpdateTask = async (id: string, payload: UpdateTaskRequest) => {
    const previousTasks = [...tasks]
    const { reminders, ...taskPayload } = payload
    setTasks((prev) => prev.map((t) => (t.id === id ? { ...t, ...taskPayload } : t)))

    try {
      const updated = await tasksService.updateTask(id, taskPayload)

      if (reminders && updated.dueDate) {
        const updatedReminders = await remindersService.upsertReminders(id, {
          taskTitle: updated.title,
          dueDateUtc: updated.dueDate,
          reminders,
        })
        updated.reminders = updatedReminders
        setTasks((prev) => mergeTaskReminders(prev, id, updatedReminders))
      }

      return updated
    } catch (err) {
      setTasks(previousTasks)
      if (axios.isAxiosError(err)) {
        setError(err.response?.data?.message || err.message)
      } else {
        setError('Failed to update task')
      }
      throw err
    }
  }

  const handleChangeStatus = async (id: string, status: TaskStatus) => {
    const previousTasks = [...tasks]
    setTasks(prev => prev.map(t => t.id === id ? { ...t, status } : t))

    try {
      const updated = await tasksService.changeTaskStatus(id, { status })
      if (filter.status && filter.status !== status) {
        setTasks(prev => prev.filter(t => t.id !== id))
      }
      return updated
    } catch (err) {
      setTasks(previousTasks)
      if (axios.isAxiosError(err)) {
        setError(err.response?.data?.message || err.message)
      } else {
        setError('Failed to change task status')
      }
      throw err
    }
  }

  const handleSetFavorite = async (id: string, isFavorite: boolean) => {
    const previousTasks = [...tasks]
    setTasks((prev) => prev.map((task) => (task.id === id ? { ...task, isFavorite } : task)))

    try {
      const updated = await tasksService.setFavorite(id, { isFavorite })
      setTasks((prev) => prev.map((task) => (task.id === id ? updated : task)))
      return updated
    } catch (err) {
      setTasks(previousTasks)
      if (axios.isAxiosError(err)) {
        setError(err.response?.data?.message || err.message)
      } else {
        setError('Failed to update favorite task')
      }
      throw err
    }
  }

  const handleDeleteTask = async (id: string) => {
    const previousTasks = [...tasks]
    setTasks((prev) => prev.filter((t) => t.id !== id))

    try {
      await remindersService.deleteReminders(id).catch(() => {
        // best-effort: reminder cancellation is not blocking
      })
      await tasksService.deleteTask(id)
    } catch (err) {
      setTasks(previousTasks)
      if (axios.isAxiosError(err)) {
        setError(err.response?.data?.message || err.message)
      } else {
        setError('Failed to delete task')
      }
      throw err
    }
  }

  const updateFilter = (newFilter: Partial<TasksFilter>) => {
    setFilter(prev => ({ ...prev, ...newFilter }))
  }

  return {
    tasks,
    isLoading,
    error,
    totalCount,
    filter,
    updateFilter,
    refresh: fetchTasks,
    createTask: handleCreateTask,
    updateTask: handleUpdateTask,
    changeTaskStatus: handleChangeStatus,
    setTaskFavorite: handleSetFavorite,
    deleteTask: handleDeleteTask
  }
}
