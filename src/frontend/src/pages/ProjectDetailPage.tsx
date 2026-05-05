import { useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { ArrowLeft, CheckSquare, Users, Settings as SettingsIcon } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import { useProject } from '../hooks/useProject'
import { ProjectMembersTab } from '../components/projects/ProjectMembersTab'
// import { ProjectTasksTab } from '../components/projects/ProjectTasksTab'
// import { ProjectSettingsTab } from '../components/projects/ProjectSettingsTab'

export function ProjectDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const { t } = useTranslation()
  const { project, isLoading, error } = useProject(id)
  const [activeTab, setActiveTab] = useState<'tasks' | 'members' | 'settings'>('tasks')

  if (isLoading) {
    return <div className="py-12 text-center text-gray-500">{t('common.loading', 'Loading...')}</div>
  }

  if (error || !project) {
    return (
      <div className="py-12 text-center">
        <h2 className="text-xl font-semibold text-gray-900">{t('projects.notFound', 'Project not found')}</h2>
        <p className="mt-2 text-sm text-gray-500">{error || t('projects.notFoundDesc', 'The project you are looking for does not exist or you do not have permission.')}</p>
        <button onClick={() => navigate('/workspace')} className="mt-4 text-indigo-600 hover:text-indigo-800">
          {t('projects.backToWorkspace', 'Back to Workspace')}
        </button>
      </div>
    )
  }

  const tabs = [
    { id: 'tasks', name: t('projects.tabs.tasks', 'Tasks'), icon: CheckSquare },
    { id: 'members', name: t('projects.tabs.members', 'Members'), icon: Users },
    { id: 'settings', name: t('projects.tabs.settings', 'Settings'), icon: SettingsIcon },
  ] as const

  return (
    <div className="mx-auto max-w-6xl space-y-6">
      <div className="flex items-center gap-4">
        <button
          onClick={() => navigate('/workspace')}
          className="flex h-10 w-10 items-center justify-center rounded-xl border border-gray-200 bg-white text-gray-500 transition-colors hover:bg-gray-50 hover:text-gray-900"
        >
          <ArrowLeft className="h-5 w-5" />
        </button>
        <div>
          <div className="flex items-center gap-2">
            <h1 className="text-2xl font-bold tracking-tight text-gray-900">{project.name}</h1>
            <span className="inline-flex items-center rounded-full bg-gray-100 px-2.5 py-0.5 text-xs font-medium text-gray-800">
              {project.key}
            </span>
          </div>
          <p className="text-sm text-gray-500">{project.description}</p>
        </div>
      </div>

      <div className="rounded-2xl border border-gray-200 bg-white shadow-sm overflow-hidden">
        <div className="border-b border-gray-200 px-6 pt-4">
          <nav className="-mb-px flex space-x-8" aria-label="Tabs">
            {tabs.map((tab) => {
              const isActive = activeTab === tab.id
              // hide settings if not allowed
              if (tab.id === 'settings' && !project.canManageSettings) return null

              return (
                <button
                  key={tab.id}
                  onClick={() => setActiveTab(tab.id)}
                  className={`
                    group inline-flex items-center border-b-2 py-4 px-1 text-sm font-medium
                    ${isActive 
                      ? 'border-indigo-500 text-indigo-600' 
                      : 'border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700'
                    }
                  `}
                >
                  <tab.icon
                    className={`
                      -ml-0.5 mr-2 h-5 w-5
                      ${isActive ? 'text-indigo-500' : 'text-gray-400 group-hover:text-gray-500'}
                    `}
                    aria-hidden="true"
                  />
                  {tab.name}
                </button>
              )
            })}
          </nav>
        </div>

        <div className="p-6">
          {activeTab === 'tasks' && (
            <div className="py-8 text-center text-gray-500">
              {/* <ProjectTasksTab projectId={project.id} /> */}
              <CheckSquare className="mx-auto h-12 w-12 text-gray-300 mb-4" />
              <p>{t('projects.tasksPlaceholder', 'Task management for projects coming soon.')}</p>
            </div>
          )}
          {activeTab === 'members' && (
            <ProjectMembersTab project={project} />
          )}
          {activeTab === 'settings' && project.canManageSettings && (
            <div className="py-8 text-center text-gray-500">
              {/* <ProjectSettingsTab project={project} /> */}
              <SettingsIcon className="mx-auto h-12 w-12 text-gray-300 mb-4" />
              <p>{t('projects.settingsPlaceholder', 'Project settings coming soon.')}</p>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
