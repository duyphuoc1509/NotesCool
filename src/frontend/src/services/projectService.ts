import api from './api'
import type {
  ProjectSummary,
  ProjectDetail,
  CreateProjectPayload,
  UpdateProjectPayload,
  AddProjectMemberPayload,
  ProjectMember,
} from '../types/project'

export const projectService = {
  async listProjects(workspaceId: string): Promise<ProjectSummary[]> {
    const { data } = await api.get<ProjectSummary[]>(`/api/workspaces/${workspaceId}/projects`)
    return data
  },

  async getProject(id: string): Promise<ProjectDetail> {
    const { data } = await api.get<ProjectDetail>(`/api/projects/${id}`)
    return data
  },

  async createProject(workspaceId: string, payload: CreateProjectPayload): Promise<ProjectSummary> {
    const { data } = await api.post<ProjectSummary>(`/api/workspaces/${workspaceId}/projects`, {
      name: payload.name,
      description: payload.description,
    })
    return data
  },

  async updateProject(id: string, payload: UpdateProjectPayload): Promise<ProjectSummary> {
    const { data } = await api.put<ProjectSummary>(`/api/projects/${id}`, payload)
    return data
  },

  async deleteProject(id: string): Promise<void> {
    await api.delete(`/api/projects/${id}/archive`)
  },

  async getProjectMembers(projectId: string): Promise<ProjectMember[]> {
    const { data } = await api.get<ProjectMember[]>(`/api/projects/${projectId}/members`)
    return data
  },

  async addProjectMember(projectId: string, payload: AddProjectMemberPayload): Promise<ProjectMember> {
    const { data } = await api.post<ProjectMember>(`/api/projects/${projectId}/members`, payload)
    return data
  },

  async updateProjectMember(projectId: string, userId: string, payload: { role: string }): Promise<ProjectMember> {
    const { data } = await api.put<ProjectMember>(`/api/projects/${projectId}/members/${userId}`, payload)
    return data
  },

  async removeProjectMember(projectId: string, userId: string): Promise<void> {
    await api.delete(`/api/projects/${projectId}/members/${userId}`)
  },
}
