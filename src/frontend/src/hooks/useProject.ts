import { useState, useCallback, useEffect } from 'react'
import { projectService } from '../services/projectService'
import type { ProjectDetail, UpdateProjectPayload, AddProjectMemberPayload } from '../types/project'

function extractErrorMessage(err: unknown): string {
  if (err instanceof Error) return err.message
  return 'An unexpected error occurred'
}

export function useProject(id: string | undefined) {
  const [project, setProject] = useState<ProjectDetail | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const fetchProject = useCallback(async () => {
    if (!id) return
    setIsLoading(true)
    setError(null)
    try {
      const data = await projectService.getProject(id)
      setProject(data)
    } catch (err: unknown) {
      setError(extractErrorMessage(err))
    } finally {
      setIsLoading(false)
    }
  }, [id])

  const updateProject = useCallback(async (payload: UpdateProjectPayload) => {
    if (!id) return
    try {
      await projectService.updateProject(id, payload)
      await fetchProject()
    } catch (err: unknown) {
      setError(extractErrorMessage(err))
      throw err
    }
  }, [id, fetchProject])

  const deleteProject = useCallback(async () => {
    if (!id) return
    try {
      await projectService.deleteProject(id)
    } catch (err: unknown) {
      setError(extractErrorMessage(err))
      throw err
    }
  }, [id])

  const addMember = useCallback(async (payload: AddProjectMemberPayload) => {
    if (!id) return
    try {
      await projectService.addProjectMember(id, payload)
      await fetchProject()
    } catch (err: unknown) {
      setError(extractErrorMessage(err))
      throw err
    }
  }, [id, fetchProject])

  const removeMember = useCallback(async (userId: string) => {
    if (!id) return
    try {
      await projectService.removeProjectMember(id, userId)
      await fetchProject()
    } catch (err: unknown) {
      setError(extractErrorMessage(err))
      throw err
    }
  }, [id, fetchProject])

  useEffect(() => {
    void fetchProject()
  }, [fetchProject])

  return {
    project,
    isLoading,
    error,
    refetch: fetchProject,
    updateProject,
    deleteProject,
    addMember,
    removeMember,
  }
}
