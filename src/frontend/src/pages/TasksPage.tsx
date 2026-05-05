import { useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import { Bell, LayoutGrid, List, Loader2, Plus, RefreshCw, Star } from 'lucide-react'
import { useTasks } from '../hooks/useTasks'
import type { TaskDto, TaskStatus } from '../types/task'
import { cn } from '../utils/cn'
import { TasksKanban } from '../components/tasks/TasksKanban'
import { TasksList } from '../components/tasks/TasksList'
import { useTranslation } from 'react-i18next'
import { TaskTableView } from '../components/tasks/table/TaskTableView'
import { TasksFilterBar } from '../components/tasks/filters/TasksFilterBar'
import { TaskDetailDrawer } from '../components/tasks/details/TaskDetailDrawer'

interface TaskFormState {
  title: string
  description: string
  dueDate: string
  priority: string
  reminderOffsets: number[]
}

const emptyForm: TaskFormState = { title: '', description: '', dueDate: '', priority: '', reminderOffsets: [] }

function toDateInputValue(value?: string | null) {
  return value ? value.slice(0, 10) : ''
}

function toIsoDate(value: string) {
  return value ? new Date(`${value}T00:00:00`).toISOString() : undefined
}

export function TasksPage() {
  const { t } = useTranslation()
  const [viewMode, setViewMode] = useState<'list' | 'kanban' | 'table'>('kanban')

  const statusOptions: Array<{ value: TaskStatus | 'all'; label: string }> = [
    { value: 'all', label: t('tasks.statusAll') },
    { value: 'Todo', label: t('tasks.statusTodo') },
    { value: 'InProgress', label: t('tasks.statusInProgress') },
    { value: 'InReview', label: t('tasks.statusInReview') },
    { value: 'Done', label: t('tasks.statusDone') },
    { value: 'Blocked', label: t('tasks.statusBlocked') },
  ]

  const REMINDER_OPTIONS = [
    { value: 5, label: t('tasks.reminder5min') },
    { value: 15, label: t('tasks.reminder15min') },
    { value: 60, label: t('tasks.reminder1hour') },
    { value: 1440, label: t('tasks.reminder1day') },
  ] as const
  const {
    tasks,
    isLoading,
    error,
    totalCount,
    filter,
    updateFilter,
    refresh,
    createTask,
    updateTask,
    changeTaskStatus,
    setTaskFavorite,
    deleteTask,
  } = useTasks({ page: 1, pageSize: 100 })
  const [form, setForm] = useState<TaskFormState>(emptyForm)
  const [editingTask, setEditingTask] = useState<TaskDto | null>(null)
  const [selectedTask, setSelectedTask] = useState<TaskDto | null>(null)
  const [drawerOpen, setDrawerOpen] = useState(false)
  const [submitting, setSubmitting] = useState(false)
  const [actionTaskId, setActionTaskId] = useState<string | null>(null)
  const [formError, setFormError] = useState<string | null>(null)

  const pageTitle = useMemo(() => {
    const statusLabel: Record<string, string> = {
      Todo: t('tasks.statusTodo'),
      InProgress: t('tasks.statusInProgress'),
      InReview: t('tasks.statusInReview'),
      Done: t('tasks.statusDone'),
      Blocked: t('tasks.statusBlocked'),
      Archived: t('tasks.statusArchived'),
    }
    if (!filter.status) return t('tasks.pageTitleActive')
    return statusLabel[filter.status] || filter.status
  }, [filter.status, t])

  const favoriteCount = useMemo(() => tasks.filter((task) => task.isFavorite).length, [tasks])

  const startEdit = (task: TaskDto) => {
    setEditingTask(task)
    setForm({
      title: task.title,
      description: task.description ?? '',
      dueDate: toDateInputValue(task.dueDate),
      priority: task.priority ?? '',
      reminderOffsets: task.reminders?.map((reminder) => reminder.offsetMinutes) ?? [],
    })
    setFormError(null)
  }

  const openDetails = (task: TaskDto) => {
    setSelectedTask(task)
    setDrawerOpen(true)
  }

  const resetForm = () => {
    setEditingTask(null)
    setForm(emptyForm)
    setFormError(null)
  }

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    const title = form.title.trim()
    if (!title) {
      setFormError(t('tasks.errorTitleRequired'))
      return
    }

    if (form.reminderOffsets.length > 0 && !form.dueDate) {
      setFormError(t('tasks.errorDueDateRequired'))
      return
    }

    setSubmitting(true)
    setFormError(null)

    try {
      const payload = {
        title,
        description: form.description.trim() || undefined,
        priority: form.priority ? (form.priority as 'Low' | 'Medium' | 'High' | 'Urgent') : undefined,
        dueDate: toIsoDate(form.dueDate),
        reminders: form.reminderOffsets.map((offsetMinutes) => ({ offsetMinutes })),
      }
      if (editingTask) {
        await updateTask(editingTask.id, payload)
      } else {
        await createTask(payload)
      }
      resetForm()
    } catch {
      setFormError(t('tasks.errorSaveTask'))
    } finally {
      setSubmitting(false)
    }
  }

  const handleStatusChange = async (task: TaskDto, status: TaskStatus) => {
    setActionTaskId(task.id)
    try {
      await changeTaskStatus(task.id, status)
    } finally {
      setActionTaskId(null)
    }
  }

  const handleArchive = async (task: TaskDto) => {
    setActionTaskId(task.id)
    try {
      await deleteTask(task.id)
    } finally {
      setActionTaskId(null)
    }
  }

  const handleFavoriteToggle = async (task: TaskDto) => {
    setActionTaskId(task.id)
    try {
      await setTaskFavorite(task.id, !task.isFavorite)
    } finally {
      setActionTaskId(null)
    }
  }

  return (
    <div className="mx-auto flex max-w-7xl flex-col gap-4 md:gap-6">
      <section className="rounded-2xl border border-gray-200 bg-white p-4 shadow-sm md:p-6">
        <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
          <div>
            <p className="text-sm font-semibold uppercase tracking-wide text-indigo-600">{t('tasks.sectionLabel')}</p>
            <h1 className="mt-2 text-3xl font-bold tracking-tight text-gray-950">{pageTitle}</h1>
            <p className="mt-2 text-sm text-gray-500">
              {t('tasks.subtitle')}
            </p>
            <p className="mt-2 inline-flex items-center gap-1 text-sm text-amber-600">
              <Star className="h-4 w-4 fill-amber-400 text-amber-400" /> {t('tasks.favoriteCount', { count: favoriteCount })}
            </p>
          </div>
          <div className="flex items-center gap-2">
            <div className="flex rounded-lg border border-gray-300 p-1">
              <button
                onClick={() => setViewMode('kanban')}
                className={cn(
                  'inline-flex items-center gap-1.5 rounded-md px-3 py-1.5 text-sm font-medium transition',
                  viewMode === 'kanban' ? 'bg-indigo-50 text-indigo-700' : 'text-gray-500 hover:bg-gray-50'
                )}
              >
                <LayoutGrid className="h-4 w-4" /> {t('tasks.viewKanban')}
              </button>
              <button
                onClick={() => setViewMode('list')}
                className={cn(
                  'inline-flex items-center gap-1.5 rounded-md px-3 py-1.5 text-sm font-medium transition',
                  viewMode === 'list' ? 'bg-indigo-50 text-indigo-700' : 'text-gray-500 hover:bg-gray-50'
                )}
              >
                <List className="h-4 w-4" /> {t('tasks.viewList')}
              </button>
              <button
                onClick={() => setViewMode('table')}
                className={cn(
                  'inline-flex items-center gap-1.5 rounded-md px-3 py-1.5 text-sm font-medium transition',
                  viewMode === 'table' ? 'bg-indigo-50 text-indigo-700' : 'text-gray-500 hover:bg-gray-50'
                )}
              >
                <List className="h-4 w-4" /> Table
              </button>
            </div>
            <button
              type="button"
              onClick={refresh}
              className="inline-flex items-center justify-center gap-2 rounded-lg border border-gray-300 px-4 py-2 text-sm font-semibold text-gray-700 transition hover:bg-gray-50"
            >
              <RefreshCw className="h-4 w-4" /> {t('tasks.refresh')}
            </button>
          </div>
        </div>

        <div className="mt-6 flex flex-wrap gap-2">
          {statusOptions.map((option) => (
            <button
              key={option.value}
              type="button"
              onClick={() => updateFilter({ status: option.value === 'all' ? undefined : option.value })}
              className={cn(
                'rounded-full px-4 py-2 text-sm font-semibold transition',
                (option.value === 'all' ? !filter.status : filter.status === option.value)
                  ? 'bg-indigo-600 text-white shadow-sm'
                  : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
              )}
            >
              {option.label}
            </button>
          ))}
        </div>

        <TasksFilterBar filter={filter} onFilterChange={updateFilter} />
      </section>

      <section className={cn(
        "grid gap-4 pb-20 md:gap-6 md:pb-0",
        (viewMode === 'list' || viewMode === 'table') ? "lg:grid-cols-[360px_minmax(0,1fr)]" : "flex flex-col lg:flex-row gap-6"
      )}>
        <form onSubmit={handleSubmit} className={cn(
          "h-fit rounded-2xl border border-gray-200 bg-white p-4 shadow-sm md:p-6",
          viewMode === 'kanban' ? "lg:w-[360px] shrink-0" : "w-full"
        )}>
          <div className="flex items-center gap-2">
            <Plus className="h-5 w-5 text-indigo-600" />
            <h2 className="text-lg font-semibold text-gray-950">
              {editingTask ? t('tasks.formTitleEdit') : t('tasks.formTitleCreate')}
            </h2>
          </div>

          <div className="mt-5 space-y-4">
            <label className="block">
              <span className="text-sm font-medium text-gray-700">{t('tasks.fieldTitle')}</span>
              <input
                value={form.title}
                onChange={(event) => setForm((current) => ({ ...current, title: event.target.value }))}
                maxLength={200}
                className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-100"
                placeholder={t('tasks.fieldTitlePlaceholder')}
              />
            </label>

            <label className="block">
              <span className="text-sm font-medium text-gray-700">{t('tasks.fieldDescription')}</span>
              <textarea
                value={form.description}
                onChange={(event) =>
                  setForm((current) => ({ ...current, description: event.target.value }))
                }
                rows={4}
                className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-100"
                placeholder={t('tasks.fieldDescriptionPlaceholder')}
              />
            </label>

            <label className="block">
              <span className="text-sm font-medium text-gray-700">Priority</span>
              <select
                value={form.priority}
                onChange={(event) => setForm((current) => ({ ...current, priority: event.target.value }))}
                className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-100"
              >
                <option value="">None</option>
                <option value="Low">Low</option>
                <option value="Medium">Medium</option>
                <option value="High">High</option>
                <option value="Urgent">Urgent</option>
              </select>
            </label>

            <label className="block">
              <span className="text-sm font-medium text-gray-700">{t('tasks.fieldDueDate')}</span>
              <input
                type="date"
                value={form.dueDate}
                onChange={(event) => setForm((current) => ({ ...current, dueDate: event.target.value }))}
                className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-100"
              />
            </label>

            <div className="block">
              <div className="flex items-center gap-2 text-sm font-medium text-gray-700">
                <Bell className="h-4 w-4 text-indigo-600" /> {t('tasks.fieldReminders')}
              </div>
              <p className="mt-1 text-xs text-gray-500">
                {t('tasks.remindersHelp')}
              </p>
              <div className="mt-3 flex flex-wrap gap-2">
                {REMINDER_OPTIONS.map((option) => {
                  const selected = form.reminderOffsets.includes(option.value)
                  return (
                    <button
                      key={option.value}
                      type="button"
                      onClick={() =>
                        setForm((current) => ({
                          ...current,
                          reminderOffsets: selected
                            ? current.reminderOffsets.filter((value) => value !== option.value)
                            : [...current.reminderOffsets, option.value].sort((a, b) => a - b),
                        }))
                      }
                      className={cn(
                        'rounded-full border px-3 py-1.5 text-xs font-semibold transition',
                        selected
                          ? 'border-indigo-600 bg-indigo-50 text-indigo-700'
                          : 'border-gray-300 bg-white text-gray-700 hover:border-indigo-300 hover:text-indigo-700',
                      )}
                    >
                      {option.label}
                    </button>
                  )
                })}
              </div>
            </div>
          </div>

          {formError ? <p className="mt-4 rounded-lg bg-red-50 p-3 text-sm text-red-700">{formError}</p> : null}

          <div className="mt-6 flex gap-3">
            <button
              type="submit"
              disabled={submitting}
              className="inline-flex flex-1 items-center justify-center gap-2 rounded-lg bg-indigo-600 px-4 py-2 text-sm font-semibold text-white transition hover:bg-indigo-700 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {submitting ? <Loader2 className="h-4 w-4 animate-spin" /> : null}
              {editingTask ? t('tasks.saveChanges') : t('tasks.formTitleCreate')}
            </button>
            {editingTask ? (
              <button
                type="button"
                onClick={resetForm}
                className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-semibold text-gray-700 transition hover:bg-gray-50"
              >
                {t('tasks.cancel')}
              </button>
            ) : null}
          </div>
        </form>

        <div className="flex-1 min-w-0">
          {viewMode === 'kanban' ? (
            <TasksKanban
              tasks={tasks}
              isLoading={isLoading}
              error={error}
              onStatusChange={handleStatusChange}
              onFavoriteToggle={handleFavoriteToggle}
              onTaskClick={openDetails}
            />
          ) : viewMode === 'table' ? (
            <TaskTableView
              tasks={tasks}
              onStatusChange={handleStatusChange}
              onTaskClick={openDetails}
              onFavoriteToggle={handleFavoriteToggle}
            />
          ) : (
            <TasksList
              tasks={tasks}
              isLoading={isLoading}
              error={error}
              totalCount={totalCount}
              actionTaskId={actionTaskId}
              onStatusChange={handleStatusChange}
              onFavoriteToggle={handleFavoriteToggle}
              onEdit={startEdit}
              onArchive={handleArchive}
              onRefresh={refresh}
            />
          )}
        </div>
      </section>

      <TaskDetailDrawer
        isOpen={drawerOpen}
        task={selectedTask}
        onClose={() => setDrawerOpen(false)}
        onUpdated={refresh}
      />
    </div>
  )
}
