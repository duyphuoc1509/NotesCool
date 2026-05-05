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
    const { data } = await api.get<Workspace[]>('/workspaces');
    return data;
  },
  createWorkspace: async (payload: { name: string; description: string }): Promise<Workspace> => {
    const { data } = await api.post<Workspace>('/workspaces', payload);
    return data;
  },
  getWorkspace: async (id: string): Promise<Workspace> => {
    const { data } = await api.get<Workspace>(`/workspaces/${id}`);
    return data;
  },
  getMembers: async (id: string): Promise<WorkspaceMember[]> => {
    const { data } = await api.get<WorkspaceMember[]>(`/workspaces/${id}/members`);
    return data;
  },
  addMember: async (id: string, payload: { email: string; role: string }): Promise<void> => {
    await api.post(`/workspaces/${id}/members`, payload);
  },
  updateMemberRole: async (workspaceId: string, memberId: string, role: string): Promise<void> => {
    await api.put(`/workspaces/${workspaceId}/members/${memberId}`, { role });
  },
  removeMember: async (workspaceId: string, memberId: string): Promise<void> => {
    await api.delete(`/workspaces/${workspaceId}/members/${memberId}`);
  },
  getProjects: async (id: string): Promise<Project[]> => {
    const { data } = await api.get<Project[]>(`/workspaces/${id}/projects`);
    return data;
  },
  archiveWorkspace: async (id: string): Promise<void> => {
    await api.delete(`/workspaces/${id}`);
  }
};
