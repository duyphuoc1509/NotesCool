import { useState, useCallback, useEffect } from 'react'
import { projectService } from '../services/projectService'
import { workspacesService } from '../services/workspacesService'
import type { ProjectSummary, CreateProjectPayload } from '../types/project'

function extractErrorMessage(err: unknown): string {
  if (err instanceof Error) return err.message
  return 'An unexpected error occurred'
}

export function useProjects() {
  const [projects, setProjects] = useState<ProjectSummary[]>([])
  const [workspaceId, setWorkspaceId] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const fetchProjects = useCallback(async () => {
    setIsLoading(true)
    setError(null)
    try {
      const workspaces = await workspacesService.getWorkspaces()
      const activeWorkspaceId = workspaces[0]?.id

      if (!activeWorkspaceId) {
        setWorkspaceId(null)
        setProjects([])
        return
      }

      setWorkspaceId(activeWorkspaceId)
      const result = await projectService.listProjects(activeWorkspaceId)
      setProjects(Array.isArray(result) ? result : [])
    } catch (err: unknown) {
      setError(extractErrorMessage(err))
    } finally {
      setIsLoading(false)
    }
  }, [])

  const createProject = useCallback(async (payload: CreateProjectPayload) => {
    setError(null)
    try {
      if (!workspaceId) {
        throw new Error('No workspace found to create project in')
      }
      await projectService.createProject(workspaceId, payload)
      await fetchProjects()
    } catch (err: unknown) {
      const message = extractErrorMessage(err)
      setError(message)
      throw err
    }
  }, [fetchProjects, workspaceId])

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
