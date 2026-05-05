import { useState, useCallback, useEffect } from 'react'
import { projectService } from '../services/projectService'
import type { ProjectSummary, CreateProjectPayload } from '../types/project'

function extractErrorMessage(err: unknown): string {
  if (err instanceof Error) return err.message
  return 'An unexpected error occurred'
}

export function useProjects() {
  const [projects, setProjects] = useState<ProjectSummary[]>([])
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const fetchProjects = useCallback(async () => {
    setIsLoading(true)
    setError(null)
    try {
      const result = await projectService.listProjects()
      // Backend might wrap in items or just return array. We'll handle both.
      setProjects(result.items ?? (Array.isArray(result) ? result : []))
    } catch (err: unknown) {
      setError(extractErrorMessage(err))
    } finally {
      setIsLoading(false)
    }
  }, [])

  const createProject = useCallback(async (payload: CreateProjectPayload) => {
    setError(null)
    try {
      await projectService.createProject(payload)
      await fetchProjects()
    } catch (err: unknown) {
      const message = extractErrorMessage(err)
      setError(message)
      throw err
    }
  }, [fetchProjects])

  useEffect(() => {
    void fetchProjects()
  }, [fetchProjects])

  return {
    projects,
    isLoading,
    error,
    refetch: fetchProjects,
    createProject,
  }
}
