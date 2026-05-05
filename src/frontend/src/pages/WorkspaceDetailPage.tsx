import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useWorkspace } from '../hooks/useWorkspaces';
import { workspacesService } from '../services/workspacesService';
import { useAuth } from '../hooks/useAuth';
import { ArrowLeft, Users, FolderKanban, Settings, Trash2, UserPlus } from 'lucide-react';

export function WorkspaceDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { workspace, members, projects, loading, reload } = useWorkspace(id!);
  const { user } = useAuth();
  const [activeTab, setActiveTab] = useState<'projects' | 'members' | 'settings'>('projects');
  const [newMemberEmail, setNewMemberEmail] = useState('');
  const [newMemberRole, setNewMemberRole] = useState('member');

  if (loading) return <div className="p-8">Loading...</div>;
  if (!workspace) return <div className="p-8">Workspace not found</div>;

  const isAdmin = workspace.role === 'admin';

  const handleAddMember = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!id) return;
    await workspacesService.addMember(id, { email: newMemberEmail, role: newMemberRole });
    setNewMemberEmail('');
    reload();
  };

  const handleRoleChange = async (memberId: string, role: string) => {
    if (!id) return;
    await workspacesService.updateMemberRole(id, memberId, role);
    reload();
  };

  const handleRemoveMember = async (memberId: string) => {
    if (!id) return;
    await workspacesService.removeMember(id, memberId);
    reload();
  };

  const handleArchive = async () => {
    if (!id || !confirm('Are you sure you want to archive this workspace?')) return;
    await workspacesService.archiveWorkspace(id);
    navigate('/workspaces');
  };

  return (
    <div className="mx-auto max-w-5xl space-y-6">
      <button 
        onClick={() => navigate('/workspaces')}
        className="flex items-center text-sm font-medium text-gray-500 hover:text-gray-900 mb-4"
      >
        <ArrowLeft className="mr-1 h-4 w-4" /> Back to Workspaces
      </button>

      <div>
        <h1 className="text-3xl font-bold text-gray-900">{workspace.name}</h1>
        <p className="mt-2 text-gray-500">{workspace.description}</p>
      </div>

      <div className="border-b border-gray-200">
        <nav className="-mb-px flex space-x-8">
          {[
            { id: 'projects', icon: FolderKanban, label: 'Projects' },
            { id: 'members', icon: Users, label: 'Members' },
            { id: 'settings', icon: Settings, label: 'Settings', adminOnly: true }
          ].map((tab) => (
            (!tab.adminOnly || isAdmin) && (
              <button
                key={tab.id}
                onClick={() => setActiveTab(tab.id as 'projects' | 'members' | 'settings')}
                className={`
                  whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm flex items-center
                  ${activeTab === tab.id 
                    ? 'border-indigo-500 text-indigo-600' 
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                  }
                `}
              >
                <tab.icon className={`mr-2 h-5 w-5 ${activeTab === tab.id ? 'text-indigo-500' : 'text-gray-400'}`} />
                {tab.label}
              </button>
            )
          ))}
        </nav>
      </div>

      <div className="py-4">
        {activeTab === 'projects' && (
          <div className="grid gap-4 sm:grid-cols-2">
            {projects.map(p => (
              <div key={p.id} className="rounded-xl border border-gray-200 p-4 bg-white shadow-sm">
                <h3 className="font-semibold text-gray-900">{p.name}</h3>
              </div>
            ))}
            {projects.length === 0 && <p className="text-gray-500">No projects yet.</p>}
          </div>
        )}

        {activeTab === 'members' && (
          <div className="space-y-6">
            {isAdmin && (
              <form onSubmit={handleAddMember} className="flex gap-4 items-end bg-gray-50 p-4 rounded-xl border border-gray-200">
                <div className="flex-1">
                  <label className="block text-sm font-medium text-gray-700">Email</label>
                  <input
                    required
                    type="email"
                    value={newMemberEmail}
                    onChange={(e) => setNewMemberEmail(e.target.value)}
                    className="mt-1 block w-full rounded-md border border-gray-300 p-2"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700">Role</label>
                  <select
                    value={newMemberRole}
                    onChange={(e) => setNewMemberRole(e.target.value)}
                    className="mt-1 block w-full rounded-md border border-gray-300 p-2"
                  >
                    <option value="member">Member</option>
                    <option value="admin">Admin</option>
                  </select>
                </div>
                <button type="submit" className="bg-indigo-600 text-white px-4 py-2 rounded-md hover:bg-indigo-700 flex items-center">
                  <UserPlus className="mr-2 h-4 w-4" /> Add
                </button>
              </form>
            )}

            <div className="rounded-xl border border-gray-200 bg-white overflow-hidden">
              <table className="min-w-full divide-y divide-gray-200">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">User</th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Role</th>
                    {isAdmin && <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>}
                  </tr>
                </thead>
                <tbody className="bg-white divide-y divide-gray-200">
                  {members.map(m => (
                    <tr key={m.id}>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{m.email}</td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm">
                        {isAdmin && m.userId !== user?.id ? (
                          <select
                            value={m.role}
                            onChange={(e) => handleRoleChange(m.id, e.target.value)}
                            className="rounded border border-gray-300 text-sm p-1"
                          >
                            <option value="member">Member</option>
                            <option value="admin">Admin</option>
                          </select>
                        ) : (
                          <span className="capitalize text-gray-500">{m.role}</span>
                        )}
                      </td>
                      {isAdmin && (
                        <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                          {m.userId !== user?.id && (
                            <button onClick={() => handleRemoveMember(m.id)} className="text-red-600 hover:text-red-900">
                              Remove
                            </button>
                          )}
                        </td>
                      )}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}

        {activeTab === 'settings' && isAdmin && (
          <div className="rounded-xl border border-red-200 bg-red-50 p-6">
            <h3 className="text-lg font-medium text-red-800">Danger Zone</h3>
            <p className="mt-2 text-sm text-red-600">
              Archiving a workspace will make it read-only for all members. This action can be undone by an administrator.
            </p>
            <button
              onClick={handleArchive}
              className="mt-4 flex items-center bg-red-600 text-white px-4 py-2 rounded-md hover:bg-red-700"
            >
              <Trash2 className="mr-2 h-4 w-4" /> Archive Workspace
            </button>
          </div>
        )}
      </div>
    </div>
  );
}
