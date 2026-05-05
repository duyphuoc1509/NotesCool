import { useState, useEffect } from 'react'
import { UserPlus, Shield, Users, Search } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { Modal } from '../../components/Modal'
import { useProject } from '../../hooks/useProject'
import { userManagementService } from '../../services/userManagementService'
import type { ProjectDetail, ProjectMemberRole } from '../../types/project'
import type { ManagedUser } from '../../services/userManagementService'

interface ProjectMembersTabProps {
  project: ProjectDetail
}

export function ProjectMembersTab({ project }: ProjectMembersTabProps) {
  const { t } = useTranslation()
  const { addMember, removeMember, isLoading: isProjectLoading, error: projectError } = useProject(project.id)
  
  const [isAddModalOpen, setIsAddModalOpen] = useState(false)
  const [workspaceUsers, setWorkspaceUsers] = useState<ManagedUser[]>([])
  const [isLoadingUsers, setIsLoadingUsers] = useState(false)
  
  const [selectedUserId, setSelectedUserId] = useState('')
  const [selectedRole, setSelectedRole] = useState<ProjectMemberRole>('Member')
  const [searchQuery, setSearchQuery] = useState('')

  const members = project.members || []

  const loadWorkspaceUsers = async () => {
    setIsLoadingUsers(true)
    try {
      const users = await userManagementService.listUsers()
      setWorkspaceUsers(users)
    } catch (err) {
      console.error('Failed to load workspace users', err)
    } finally {
      setIsLoadingUsers(false)
    }
  }

  useEffect(() => {
    if (isAddModalOpen) {
      void loadWorkspaceUsers()
    }
  }, [isAddModalOpen])

  const handleAddMember = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!selectedUserId) return
    
    await addMember({ userId: selectedUserId, role: selectedRole })
    setIsAddModalOpen(false)
    setSelectedUserId('')
    setSelectedRole('Member')
  }

  // Filter out users already in the project
  const availableUsers = workspaceUsers.filter(
    wu => !members.some(pm => pm.userId === wu.id) && 
          (wu.displayName?.toLowerCase().includes(searchQuery.toLowerCase()) || 
           wu.email?.toLowerCase().includes(searchQuery.toLowerCase()))
  )

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-lg font-medium text-gray-900">{t('projects.members.title', 'Project Members')}</h2>
          <p className="mt-1 text-sm text-gray-500">
            {t('projects.members.desc', 'Manage who has access to this project and their roles.')}
          </p>
        </div>
        {project.canManageMembers && (
          <button
            onClick={() => setIsAddModalOpen(true)}
            className="inline-flex items-center justify-center gap-2 rounded-lg bg-indigo-600 px-4 py-2 text-sm font-semibold text-white shadow-sm transition hover:bg-indigo-500"
          >
            <UserPlus className="h-4 w-4" />
            {t('projects.members.add', 'Add Member')}
          </button>
        )}
      </div>

      {projectError && (
        <div className="rounded-md bg-red-50 p-4 text-sm text-red-700">{projectError}</div>
      )}

      <div className="overflow-hidden rounded-lg border border-gray-200">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                {t('projects.members.user', 'User')}
              </th>
              <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                {t('projects.members.role', 'Role')}
              </th>
              <th scope="col" className="relative px-6 py-3">
                <span className="sr-only">{t('common.actions', 'Actions')}</span>
              </th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200 bg-white">
            {members.length === 0 ? (
              <tr>
                <td colSpan={3} className="px-6 py-12 text-center">
                  <Users className="mx-auto h-12 w-12 text-gray-300" />
                  <p className="mt-4 text-sm text-gray-500">{t('projects.members.empty', 'No members found.')}</p>
                </td>
              </tr>
            ) : (
              members.map((member) => (
                <tr key={member.userId}>
                  <td className="whitespace-nowrap px-6 py-4">
                    <div className="flex items-center">
                      <div className="h-10 w-10 flex-shrink-0">
                        <div className="flex h-10 w-10 items-center justify-center rounded-full bg-indigo-100 text-indigo-600 font-semibold">
                          {member.displayName?.charAt(0) || member.email?.charAt(0) || 'U'}
                        </div>
                      </div>
                      <div className="ml-4">
                        <div className="text-sm font-medium text-gray-900">{member.displayName}</div>
                        <div className="text-sm text-gray-500">{member.email}</div>
                      </div>
                    </div>
                  </td>
                  <td className="whitespace-nowrap px-6 py-4">
                    <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${
                      member.role === 'Owner' ? 'bg-purple-100 text-purple-800' :
                      member.role === 'Admin' ? 'bg-indigo-100 text-indigo-800' :
                      'bg-gray-100 text-gray-800'
                    }`}>
                      <Shield className="mr-1 h-3 w-3" />
                      {member.role}
                    </span>
                  </td>
                  <td className="whitespace-nowrap px-6 py-4 text-right text-sm font-medium">
                    {project.canManageMembers && member.role !== 'Owner' && (
                      <button
                        onClick={() => removeMember(member.userId)}
                        className="text-red-600 hover:text-red-900"
                      >
                        {t('common.remove', 'Remove')}
                      </button>
                    )}
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      <Modal isOpen={isAddModalOpen} onClose={() => setIsAddModalOpen(false)} title={t('projects.members.addTitle', 'Add Member to Project')}>
        <form onSubmit={handleAddMember} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700">
              {t('projects.members.selectUser', 'Select Workspace User')}
            </label>
            
            <div className="mt-1 relative">
              <div className="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-3">
                <Search className="h-4 w-4 text-gray-400" />
              </div>
              <input
                type="text"
                placeholder={t('projects.members.searchUsers', 'Search by name or email...')}
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm pl-10 mb-2"
              />
            </div>
            
            <div className="mt-2 max-h-48 overflow-y-auto rounded-md border border-gray-200 bg-white">
              {isLoadingUsers ? (
                <div className="p-4 text-center text-sm text-gray-500">{t('common.loading', 'Loading...')}</div>
              ) : availableUsers.length === 0 ? (
                <div className="p-4 text-center text-sm text-gray-500">{t('projects.members.noUsers', 'No available users found.')}</div>
              ) : (
                <ul className="divide-y divide-gray-200">
                  {availableUsers.map(user => (
                    <li 
                      key={user.id}
                      onClick={() => setSelectedUserId(user.id)}
                      className={`cursor-pointer px-4 py-3 hover:bg-indigo-50 flex items-center ${selectedUserId === user.id ? 'bg-indigo-50 ring-1 ring-inset ring-indigo-500' : ''}`}
                    >
                      <div className="flex h-8 w-8 items-center justify-center rounded-full bg-indigo-100 text-xs font-semibold text-indigo-600 mr-3">
                        {user.displayName?.charAt(0) || user.email?.charAt(0) || 'U'}
                      </div>
                      <div className="flex-1 min-w-0">
                        <p className="text-sm font-medium text-gray-900 truncate">{user.displayName}</p>
                        <p className="text-xs text-gray-500 truncate">{user.email}</p>
                      </div>
                    </li>
                  ))}
                </ul>
              )}
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700">
              {t('projects.members.role', 'Role')}
            </label>
            <select
              value={selectedRole}
              onChange={(e) => setSelectedRole(e.target.value as ProjectMemberRole)}
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
            >
              <option value="Member">{t('projects.roles.member', 'Member')}</option>
              <option value="Admin">{t('projects.roles.admin', 'Admin')}</option>
            </select>
          </div>

          <div className="mt-6 flex justify-end space-x-3">
            <button
              type="button"
              onClick={() => setIsAddModalOpen(false)}
              className="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm hover:bg-gray-50"
            >
              {t('common.cancel', 'Cancel')}
            </button>
            <button
              type="submit"
              disabled={isProjectLoading || !selectedUserId}
              className="inline-flex justify-center rounded-md border border-transparent bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-indigo-700 disabled:opacity-50"
            >
              {isProjectLoading ? t('common.adding', 'Adding...') : t('projects.members.add', 'Add Member')}
            </button>
          </div>
        </form>
      </Modal>
    </div>
  )
}
