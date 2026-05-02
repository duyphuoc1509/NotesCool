import api from './api'

export type UserStatus = 'Active' | 'Suspended'

export interface ManagedUser {
  id: string
  email: string
  displayName: string
  status: UserStatus | string
  roles: string[]
}

export interface UpdateUserStatusPayload {
  status: UserStatus
}

export const userManagementService = {
  async listUsers(): Promise<ManagedUser[]> {
    const { data } = await api.get<ManagedUser[]>('/api/admin/users')
    return data
  },

  async updateUserStatus(id: string, payload: UpdateUserStatusPayload): Promise<ManagedUser> {
    const { data } = await api.put<ManagedUser>(`/api/admin/users/${id}/status`, payload)
    return data
  },
}
