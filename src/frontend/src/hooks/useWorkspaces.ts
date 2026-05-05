import { useState, useEffect, useCallback } from 'react';
import { workspacesService } from '../services/workspacesService';
import type { Workspace, WorkspaceMember, Project } from '../services/workspacesService';

export function useWorkspaces() {
  const [workspaces, setWorkspaces] = useState<Workspace[]>([]);
  const [loading, setLoading] = useState(true);

  const loadWorkspaces = useCallback(async () => {
    try {
      const data = await workspacesService.getWorkspaces();
      setWorkspaces(data);
    } catch (e) {
      console.error(e);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void loadWorkspaces();
  }, [loadWorkspaces]);

  return { workspaces, loading, reload: loadWorkspaces };
}

export function useWorkspace(id: string) {
  const [workspace, setWorkspace] = useState<Workspace | null>(null);
  const [members, setMembers] = useState<WorkspaceMember[]>([]);
  const [projects, setProjects] = useState<Project[]>([]);
  const [loading, setLoading] = useState(true);

  const loadData = useCallback(async () => {
    if (!id) return;
    try {
      const [ws, m, p] = await Promise.all([
        workspacesService.getWorkspace(id),
        workspacesService.getMembers(id),
        workspacesService.getProjects(id)
      ]);
      setWorkspace(ws);
      setMembers(m);
      setProjects(p);
    } catch (e) {
      console.error(e);
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => {
    void loadData();
  }, [loadData]);

  return { workspace, members, projects, loading, reload: loadData };
}
