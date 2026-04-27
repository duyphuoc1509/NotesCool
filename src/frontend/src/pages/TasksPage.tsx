import { useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import { Calendar, CheckCircle2, Loader2, Pencil, Plus, RefreshCw, Trash2 } from 'lucide-react'
import { useTasks } from '../hooks/useTasks'
import type { TaskDto, TaskStatus } from '../types/task'
import { cn } from '../utils/cn'

const statusOptions: Array<{ value: TaskStatus | 'all'; label: string }> = [
  { value: 'all', label: 'All active' },
  { value: 'todo', label: 'To do' },
  { value: 'in_progress', label: 'In progress' },
  { value: 'done', label: 'Done' },
]

const nextStatus: Record<TaskStatus, TaskStatus> = {
  todo: 'in_progress',
  in_progress: 'done',
  done: 'todo',
  archived: 'todo',
}

const statusLabel: Record<TaskStatus, string> = {
  todo: 'To do',
  in_progress: 'In progress',
  done: 'Done',
  archived: 'Archived',
}

interface TaskFormState {
  title: string
  description: string
  dueDate: string
}

const emptyForm: TaskFormState = { title: '', description: '', dueDate: '' }

function toDateInputValue(value?: string | null) {
  return value ? value.slice(0, 10) : ''
}

function toIsoDate(value: string) {
  return value ? new Date(`${value}T00:00:00`).toISOString() : undefined
}

function formatDate(value?: string | null) {
  if (!value) return 'No due date'
  return new Intl.DateTimeFormat('en', { month: 'short', day: 'numeric', year: 'numeric' }).format(
    new Date(value)
  )
}

function statusClasses(status: TaskStatus) {
  return cn(
    'rounded-full px-2.5 py-1 text-xs font-semibold',
    status === 'todo' && 'bg-slate-100 text-slate-700',
    status === 'in_progress' && 'bg-blue-100 text-blue-700',
    status === 'done' && 'bg-emerald-100 text-emerald-700',
    status === 'archived' && 'bg-gray-100 text-gray-500'
  )
}

export function TasksPage() {
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
    deleteTask,
  } = useTasks({ page: 1, pageSize: 20 })
  const [form, setForm] = useState<TaskFormState>(emptyForm)
  const [editingTask, setEditingTask] = useState<TaskDto | null>(null)
  const [submitting, setSubmitting] = useState(false)
  const [actionTaskId, setActionTaskId] = useState<string | null>(null)
  const [formError, setFormError] = useState<string | null>(null)

  const pageTitle = useMemo(() => {
    if (!filter.status) return 'All active tasks'
    return statusLabel[filter.status]
  }, [filter.status])

  const startEdit = (task: TaskDto) => {
    setEditingTask(task)
    setForm({
      title: task.title,
      description: task.description ?? '',
      dueDate: toDateInputValue(task.dueDate),
    })
    setFormError(null)
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
      setFormError('Title is required.')
      return
    }

    setSubmitting(true)
    setFormError(null)

    try {
      const payload = {
        title,
        description: form.description.trim() || undefined,
        dueDate: toIsoDate(form.dueDate),
      }
      if (editingTask) {
        await updateTask(editingTask.id, payload)
      } else {
        await createTask(payload)
      }
      resetForm()
    } catch {
      setFormError('Unable to save task. Please check the fields and try again.')
    } finally {
      setSubmitting(false)
    }
  }

  const handleStatusChange = async (task: TaskDto, status: TaskStatus = nextStatus[task.status]) => {
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

  return (
    <div className="mx-auto flex max-w-7xl flex-col gap-6">
      <section className="rounded-2xl border border-gray-200 bg-white p-6 shadow-sm">
        <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
          <div>
            <p className="text-sm font-semibold uppercase tracking-wide text-indigo-600">Tasks</p>
            <h1 className="mt-2 text-3xl font-bold tracking-tight text-gray-950">{pageTitle}</h1>
            <p className="mt-2 text-sm text-gray-500">
              Create tasks, update progress, filter by status, and archive completed work.
            </p>
          </div>
          <button
            type="button"
            onClick={refresh}
            className="inline-flex items-center justify-center gap-2 rounded-lg border border-gray-300 px-4 py-2 text-sm font-semibold text-gray-700 transition hover:bg-gray-50"
          >
            <RefreshCw className="h-4 w-4" /> Refresh
          </button>
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
      </section>

      <section className="grid gap-6 lg:grid-cols-[360px_minmax(0,1fr)]">
        <form onSubmit={handleSubmit} className="h-fit rounded-2xl border border-gray-200 bg-white p-6 shadow-sm">
          <div className="flex items-center gap-2">
            <Plus className="h-5 w-5 text-indigo-600" />
            <h2 className="text-lg font-semibold text-gray-950">
              {editingTask ? 'Edit task' : 'Create task'}
            </h2>
          </div>

          <div className="mt-5 space-y-4">
            <label className="block">
              <span className="text-sm font-medium text-gray-700">Title</span>
              <input
                value={form.title}
                onChange={(event) => setForm((current) => ({ ...current, title: event.target.value }))}
                maxLength={200}
                className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-100"
                placeholder="Prepare weekly plan"
              />
            </label>

            <label className="block">
              <span className="text-sm font-medium text-gray-700">Description</span>
              <textarea
                value={form.description}
                onChange={(event) =>
                  setForm((current) => ({ ...current, description: event.target.value }))
                }
                rows={4}
                className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-100"
                placeholder="Add details, acceptance notes, or reminders"
              />
            </label>

            <label className="block">
              <span className="text-sm font-medium text-gray-700">Due date</span>
              <input
                type="date"
                value={form.dueDate}
                onChange={(event) => setForm((current) => ({ ...current, dueDate: event.target.value }))}
                className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-sm shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-100"
              />
            </label>
          </div>

          {formError ? <p className="mt-4 rounded-lg bg-red-50 p-3 text-sm text-red-700">{formError}</p> : null}

          <div className="mt-6 flex gap-3">
            <button
              type="submit"
              disabled={submitting}
              className="inline-flex flex-1 items-center justify-center gap-2 rounded-lg bg-indigo-600 px-4 py-2 text-sm font-semibold text-white transition hover:bg-indigo-700 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {submitting ? <Loader2 className="h-4 w-4 animate-spin" /> : null}
              {editingTask ? 'Save changes' : 'Create task'}
            </button>
            {editingTask ? (
              <button
                type="button"
                onClick={resetForm}
                className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-semibold text-gray-700 transition hover:bg-gray-50"
              >
                Cancel
              </button>
            ) : null}
          </div>
        </form>

        <div className="rounded-2xl border border-gray-200 bg-white p-6 shadow-sm">
          <div className="flex items-center justify-between border-b border-gray-100 pb-4">
            <div>
              <h2 className="text-lg font-semibold text-gray-950">Task list</h2>
              <p className="text-sm text-gray-500">{totalCount} total matching task(s)</p>
            </div>
          </div>

          {error ? (
            <div className="mt-6 rounded-xl border border-red-200 bg-red-50 p-4 text-sm text-red-700">
              <p className="font-semibold">Could not load tasks</p>
              <p className="mt-1">{error}</p>
              <button type="button" onClick={refresh} className="mt-3 font-semibold text-red-800 underline">
                Retry
              </button>
            </div>
          ) : null}

          {isLoading ? (
            <div className="mt-6 space-y-3" aria-label="Loading tasks">
              {Array.from({ length: 4 }).map((_, index) => (
                <div key={index} className="h-24 animate-pulse rounded-xl bg-gray-100" />
              ))}
            </div>
          ) : !error && tasks.length === 0 ? (
            <div className="mt-6 rounded-xl border border-dashed border-gray-300 p-8 text-center">
              <CheckCircle2 className="mx-auto h-10 w-10 text-gray-300" />
              <h3 className="mt-3 text-base font-semibold text-gray-950">No tasks found</h3>
              <p className="mt-1 text-sm text-gray-500">
                Create your first task or switch filters to see other statuses.
              </p>
            </div>
          ) : (
            <ul className="mt-6 space-y-3">
              {tasks.map((task) => (
                <li key={task.id} className="rounded-xl border border-gray-200 p-4 transition hover:border-indigo-200 hover:shadow-sm">
                  <div className="flex flex-col gap-4 md:flex-row md:items-start md:justify-between">
                    <div className="min-w-0 flex-1">
                      <div className="flex flex-wrap items-center gap-2">
                        <span className={statusClasses(task.status)}>{statusLabel[task.status]}</span>
                        <span className="inline-flex items-center gap-1 text-xs text-gray-500">
                          <Calendar className="h-3.5 w-3.5" /> {formatDate(task.dueDate)}
                        </span>
                      </div>
                      <h3 className="mt-2 text-base font-semibold text-gray-950">{task.title}</h3>
                      {task.description ? <p className="mt-1 text-sm text-gray-600">{task.description}</p> : null}
                    </div>

                    <div className="flex flex-wrap gap-2">
                      <button
                        type="button"
                        disabled={actionTaskId === task.id}
                        onClick={() => handleStatusChange(task)}
                        className="rounded-lg bg-emerald-50 px-3 py-2 text-xs font-semibold text-emerald-700 transition hover:bg-emerald-100 disabled:opacity-60"
                      >
                        Move to {statusLabel[nextStatus[task.status]]}
                      </button>
                      <button
                        type="button"
                        onClick={() => startEdit(task)}
                        className="inline-flex items-center gap-1 rounded-lg bg-gray-100 px-3 py-2 text-xs font-semibold text-gray-700 transition hover:bg-gray-200"
                      >
                        <Pencil className="h-3.5 w-3.5" /> Edit
                      </button>
                      <button
                        type="button"
                        disabled={actionTaskId === task.id}
                        onClick={() => handleArchive(task)}
                        className="inline-flex items-center gap-1 rounded-lg bg-red-50 px-3 py-2 text-xs font-semibold text-red-700 transition hover:bg-red-100 disabled:opacity-60"
                      >
                        <Trash2 className="h-3.5 w-3.5" /> Archive
                      </button>
                    </div>
                  </div>
                </li>
              ))}
            </ul>
          )}
        </div>
      </section>
    </div>
  )
}
