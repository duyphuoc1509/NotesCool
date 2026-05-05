import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useWorkspaces } from '../hooks/useWorkspaces';
import { workspacesService } from '../services/workspacesService';
import { Plus, LayoutGrid } from 'lucide-react';

export function WorkspacesPage() {
  const { workspaces, loading, reload } = useWorkspaces();
  const navigate = useNavigate();
  const [isModalOpen, setModalOpen] = useState(false);
  const [newWsName, setNewWsName] = useState('');
  const [newWsDesc, setNewWsDesc] = useState('');

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    await workspacesService.createWorkspace({ name: newWsName, description: newWsDesc });
    setModalOpen(false);
    setNewWsName('');
    setNewWsDesc('');
    reload();
  };

  if (loading) return <div className="p-8">Loading...</div>;

  return (
    <div className="mx-auto max-w-5xl space-y-8">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-gray-900">Workspaces</h1>
        <button
          onClick={() => setModalOpen(true)}
          className="flex items-center gap-2 rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700"
        >
          <Plus className="h-4 w-4" /> New Workspace
        </button>
      </div>

      <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
        {workspaces.map((ws) => (
          <div
            key={ws.id}
            onClick={() => navigate(`/workspaces/${ws.id}`)}
            className="cursor-pointer rounded-2xl border border-gray-200 bg-white p-6 shadow-sm transition-all hover:border-indigo-300 hover:shadow-md"
          >
            <div className="flex items-center gap-3 mb-4">
              <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-indigo-50 text-indigo-600">
                <LayoutGrid className="h-5 w-5" />
              </div>
              <h2 className="text-lg font-semibold text-gray-900">{ws.name}</h2>
            </div>
            <p className="text-sm text-gray-500 line-clamp-2">{ws.description}</p>
            <div className="mt-4 flex items-center justify-between text-xs text-gray-400">
              <span className="capitalize">{ws.role}</span>
              <span>{new Date(ws.createdAt).toLocaleDateString()}</span>
            </div>
          </div>
        ))}
      </div>

      {isModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
          <div className="w-full max-w-md rounded-2xl bg-white p-6 shadow-xl">
            <h2 className="text-xl font-bold text-gray-900 mb-4">Create Workspace</h2>
            <form onSubmit={handleCreate} className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700">Name</label>
                <input
                  required
                  autoFocus
                  type="text"
                  value={newWsName}
                  onChange={(e) => setNewWsName(e.target.value)}
                  className="mt-1 block w-full rounded-lg border border-gray-300 p-2 focus:border-indigo-500 focus:ring-indigo-500"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">Description</label>
                <textarea
                  value={newWsDesc}
                  onChange={(e) => setNewWsDesc(e.target.value)}
                  className="mt-1 block w-full rounded-lg border border-gray-300 p-2 focus:border-indigo-500 focus:ring-indigo-500"
                  rows={3}
                />
              </div>
              <div className="flex justify-end gap-3 mt-6">
                <button
                  type="button"
                  onClick={() => setModalOpen(false)}
                  className="rounded-lg px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-100"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white hover:bg-indigo-700"
                >
                  Create
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
