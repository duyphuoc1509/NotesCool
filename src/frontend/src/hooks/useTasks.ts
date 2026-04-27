import { useState, useEffect, useCallback } from 'react'
import axios from 'axios'
import type { TaskDto, TaskStatus, TasksFilter, CreateTaskRequest, UpdateTaskRequest } from '../types/task'
import * as tasksService from '../services/tasks'

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
      const data = await tasksService.getTasks(filter)
      setTasks(data.items)
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
    // eslint-disable-next-line react-hooks/set-state-in-effect
    fetchTasks()
  }, [fetchTasks])

  const handleCreateTask = async (payload: CreateTaskRequest) => {
    try {
      const newTask = await tasksService.createTask(payload)
      fetchTasks()
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
    setTasks(prev => prev.map(t => t.id === id ? { ...t, ...payload } : t))
    
    try {
      const updated = await tasksService.updateTask(id, payload)
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

  const handleDeleteTask = async (id: string) => {
    const previousTasks = [...tasks]
    setTasks(prev => prev.filter(t => t.id !== id))

    try {
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
    deleteTask: handleDeleteTask
  }
}
