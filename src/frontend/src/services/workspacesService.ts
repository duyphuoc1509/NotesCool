import api from './api';

export interface Workspace {
  id: string;
  name: string;
  description: string;
  createdAt: string;
  role: 'admin' | 'member';
}

export interface WorkspaceMember {
  id: string;
  userId: string;
  email: string;
  role: 'admin' | 'member';
}

export interface Project {
  id: string;
  name: string;
}

export const workspacesService = {
  getWorkspaces: async (): Promise<Workspace[]> => {
    const { data } = await api.get<Workspace[]>('/api/workspaces');
    return data;
  },
  createWorkspace: async (payload: { name: string; description: string }): Promise<Workspace> => {
    const { data } = await api.post<Workspace>('/api/workspaces', payload);
    return data;
  },
  getWorkspace: async (id: string): Promise<Workspace> => {
    const { data } = await api.get<Workspace>(`/api/workspaces/${id}`);
    return data;
  },
  getMembers: async (id: string): Promise<WorkspaceMember[]> => {
    const { data } = await api.get<WorkspaceMember[]>(`/api/workspaces/${id}/members`);
    return data;
  },
  addMember: async (id: string, payload: { email: string; role: string }): Promise<void> => {
    await api.post(`/api/workspaces/${id}/members`, payload);
  },
  updateMemberRole: async (workspaceId: string, userId: string, role: string): Promise<void> => {
    await api.put(`/api/workspaces/${workspaceId}/members/${userId}`, { role });
  },
  removeMember: async (workspaceId: string, userId: string): Promise<void> => {
    await api.delete(`/api/workspaces/${workspaceId}/members/${userId}`);
  },
  getProjects: async (id: string): Promise<Project[]> => {
    const { data } = await api.get<Project[]>(`/api/workspaces/${id}/projects`);
    return data;
  },
  archiveWorkspace: async (id: string): Promise<void> => {
    await api.delete(`/api/workspaces/${id}`);
  }
};
