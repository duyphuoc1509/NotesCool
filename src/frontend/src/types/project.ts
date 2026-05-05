export type ProjectVisibility = 'Private' | 'Workspace'
export type ProjectMemberRole = 'Owner' | 'Admin' | 'Member' | string

export interface ProjectSummary {
  id: string
  name: string
  key: string
  description: string | null
  visibility: ProjectVisibility | string
  memberCount: number
  taskCount: number
  createdAtUtc?: string
  updatedAtUtc?: string
  canView: boolean
  canEdit: boolean
  canManageMembers: boolean
  canManageSettings: boolean
}

export interface ProjectDetail extends ProjectSummary {
  members: ProjectMember[]
}

export interface ProjectMember {
  userId: string
  email: string
  displayName: string
  role: ProjectMemberRole
}

export interface CreateProjectPayload {
  name: string
  key: string
  description?: string
  visibility: ProjectVisibility
}

export interface UpdateProjectPayload {
  name: string
  description?: string
  visibility: ProjectVisibility
}

export interface AddProjectMemberPayload {
  userId: string
  role: ProjectMemberRole
}
