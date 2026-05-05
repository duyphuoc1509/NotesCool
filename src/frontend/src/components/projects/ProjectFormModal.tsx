import { useState } from 'react'
import { Modal } from '../Modal'
import { useTranslation } from 'react-i18next'
import type { CreateProjectPayload, ProjectVisibility } from '../../types/project'

interface ProjectFormModalProps {
  isOpen: boolean
  onClose: () => void
  onSubmit: (payload: CreateProjectPayload) => Promise<void>
}

export function ProjectFormModal({ isOpen, onClose, onSubmit }: ProjectFormModalProps) {
  const { t } = useTranslation()
  const [name, setName] = useState('')
  const [key, setKey] = useState('')
  const [description, setDescription] = useState('')
  const [visibility, setVisibility] = useState<ProjectVisibility>('Private')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  // Auto-generate key from name if key is empty
  const handleNameChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const val = e.target.value
    setName(val)
    if (!key || key === generateKey(name)) {
      setKey(generateKey(val))
    }
  }

  const generateKey = (str: string) => {
    return str.replace(/[^a-zA-Z0-9]/g, '').substring(0, 5).toUpperCase()
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError(null)
    setIsSubmitting(true)
    
    try {
      await onSubmit({ name, key, description, visibility })
      setName('')
      setKey('')
      setDescription('')
      setVisibility('Private')
      onClose()
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : t('projects.createError', 'Failed to create project'))
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <Modal isOpen={isOpen} onClose={onClose} title={t('projects.createNew', 'Create New Project')}>
      <form onSubmit={handleSubmit} className="space-y-4">
        {error && (
          <div className="rounded-md bg-red-50 p-3 text-sm text-red-700">
            {error}
          </div>
        )}
        
        <div className="grid grid-cols-3 gap-4">
          <div className="col-span-2">
            <label htmlFor="projectName" className="block text-sm font-medium text-gray-700">
              {t('projects.name', 'Project Name')} <span className="text-red-500">*</span>
            </label>
            <input
              type="text"
              id="projectName"
              required
              value={name}
              onChange={handleNameChange}
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
              placeholder={t('projects.namePlaceholder', 'e.g., Marketing Campaign')}
            />
          </div>
          <div>
            <label htmlFor="projectKey" className="block text-sm font-medium text-gray-700">
              {t('projects.key', 'Key')} <span className="text-red-500">*</span>
            </label>
            <input
              type="text"
              id="projectKey"
              required
              maxLength={10}
              value={key}
              onChange={(e) => setKey(e.target.value.toUpperCase())}
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
            />
          </div>
        </div>

        <div>
          <label htmlFor="projectDescription" className="block text-sm font-medium text-gray-700">
            {t('projects.description', 'Description')}
          </label>
          <textarea
            id="projectDescription"
            rows={3}
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            {t('projects.visibility', 'Visibility')}
          </label>
          <div className="space-y-2">
            <label className="flex items-center">
              <input
                type="radio"
                name="visibility"
                value="Private"
                checked={visibility === 'Private'}
                onChange={() => setVisibility('Private')}
                className="h-4 w-4 border-gray-300 text-indigo-600 focus:ring-indigo-500"
              />
              <span className="ml-2 text-sm text-gray-700">
                <strong className="font-medium">{t('projects.visibilityPrivate', 'Private')}</strong> — {t('projects.visibilityPrivateDesc', 'Only members can access this project')}
              </span>
            </label>
            <label className="flex items-center">
              <input
                type="radio"
                name="visibility"
                value="Workspace"
                checked={visibility === 'Workspace'}
                onChange={() => setVisibility('Workspace')}
                className="h-4 w-4 border-gray-300 text-indigo-600 focus:ring-indigo-500"
              />
              <span className="ml-2 text-sm text-gray-700">
                <strong className="font-medium">{t('projects.visibilityWorkspace', 'Workspace')}</strong> — {t('projects.visibilityWorkspaceDesc', 'Anyone in the workspace can view and join')}
              </span>
            </label>
          </div>
        </div>

        <div className="mt-6 flex justify-end space-x-3">
          <button
            type="button"
            onClick={onClose}
            className="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
          >
            {t('common.cancel', 'Cancel')}
          </button>
          <button
            type="submit"
            disabled={isSubmitting || !name || !key}
            className="inline-flex justify-center rounded-md border border-transparent bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {isSubmitting ? t('common.creating', 'Creating...') : t('common.create', 'Create Project')}
          </button>
        </div>
      </form>
    </Modal>
  )
}
