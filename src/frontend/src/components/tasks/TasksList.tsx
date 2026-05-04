import { Bell, Calendar, Pencil, Trash2, CheckCircle2, Star } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import type { TaskDto, TaskStatus } from '../../types/task'
import { cn } from '../../utils/cn'

const nextStatus: Record<TaskStatus, TaskStatus> = {
  Todo: 'InProgress',
  InProgress: 'Done',
  Done: 'Todo',
  Blocked: 'Todo',
  Archived: 'Todo',
  InReview: 'Done'
}

// statusLabel is now inside the component to access t()

function statusClasses(status: TaskStatus) {
  return cn(
    'rounded-full px-2.5 py-1 text-xs font-semibold',
    status === 'Todo' && 'bg-slate-100 text-slate-700',
    status === 'InProgress' && 'bg-blue-100 text-blue-700',
    status === 'Done' && 'bg-emerald-100 text-emerald-700',
    status === 'Blocked' && 'bg-red-100 text-red-700',
    status === 'InReview' && 'bg-purple-100 text-purple-700',
    status === 'Archived' && 'bg-gray-100 text-gray-500'
  )
}

function formatDate(value?: string | null, noDueDateLabel = 'No due date') {
  if (!value) return noDueDateLabel
  return new Intl.DateTimeFormat('en', { month: 'short', day: 'numeric', year: 'numeric' }).format(new Date(value))
}

interface TasksListProps {
  tasks: TaskDto[]
  isLoading: boolean
  error: string | null
  totalCount: number
  actionTaskId: string | null
  onStatusChange: (task: TaskDto, newStatus: TaskStatus) => Promise<void>
  onFavoriteToggle: (task: TaskDto) => Promise<void>
  onEdit: (task: TaskDto) => void
  onArchive: (task: TaskDto) => Promise<void>
  onRefresh: () => void
}

export function TasksList({
  tasks,
  isLoading,
  error,
  totalCount,
  actionTaskId,
  onStatusChange,
  onFavoriteToggle,
  onEdit,
  onArchive,
  onRefresh
}: TasksListProps) {
  const { t } = useTranslation()
  const statusLabel: Record<TaskStatus, string> = {
    Todo: t('tasks.statusTodo'),
    InProgress: t('tasks.statusInProgress'),
    Done: t('tasks.statusDone'),
    Blocked: t('tasks.statusBlocked'),
    Archived: t('tasks.statusArchived'),
    InReview: t('tasks.statusInReview'),
  }
  return (
    <div className="rounded-2xl border border-gray-200 bg-white p-4 shadow-sm md:p-6">
      <div className="flex items-center justify-between border-b border-gray-100 pb-4">
        <div>
          <h2 className="text-lg font-semibold text-gray-950">{t('tasks.taskList')}</h2>
          <p className="text-sm text-gray-500">{t('tasks.totalMatchingTasks', { count: totalCount })}</p>
        </div>
      </div>

      {error ? (
        <div className="mt-6 rounded-xl border border-red-200 bg-red-50 p-4 text-sm text-red-700">
          <p className="font-semibold">{t('tasks.couldNotLoad')}</p>
          <p className="mt-1">{error}</p>
          <button type="button" onClick={onRefresh} className="mt-3 font-semibold text-red-800 underline">
            {t('tasks.retry')}
          </button>
        </div>
      ) : null}

      {isLoading ? (
        <div className="mt-6 space-y-3" aria-label={t('tasks.loading')}>
          {Array.from({ length: 4 }).map((_, index) => (
            <div key={index} className="h-24 animate-pulse rounded-xl bg-gray-100" />
          ))}
        </div>
      ) : !error && tasks.length === 0 ? (
        <div className="mt-6 rounded-xl border border-dashed border-gray-300 p-8 text-center">
          <CheckCircle2 className="mx-auto h-10 w-10 text-gray-300" />
          <h3 className="mt-3 text-base font-semibold text-gray-950">{t('tasks.noTasksFound')}</h3>
          <p className="mt-1 text-sm text-gray-500">
            {t('tasks.noTasksFoundDesc')}
          </p>
        </div>
      ) : (
        <ul className="mt-6 space-y-3">
          {tasks.map((task) => (
            <li key={task.id} className="rounded-xl border border-gray-200 p-4 transition hover:border-indigo-200 hover:shadow-sm">
              <div className="flex flex-col gap-4 md:flex-row md:items-start md:justify-between">
                <div className="min-w-0 flex-1">
                  <div className="flex flex-wrap items-center gap-2">
                    <span className={statusClasses(task.status)}>{statusLabel[task.status] || task.status}</span>
                    {task.isFavorite ? (
                      <span className="inline-flex items-center gap-1 rounded-full bg-amber-50 px-2.5 py-1 text-xs font-semibold text-amber-700">
                        <Star className="h-3.5 w-3.5 fill-amber-400 text-amber-400" /> {t('tasks.favorite')}
                      </span>
                    ) : null}
                    <span className="inline-flex items-center gap-1 text-xs text-gray-500">
                      <Calendar className="h-3.5 w-3.5" /> {formatDate(task.dueDate, t('tasks.noDueDate'))}
                    </span>
                    {task.reminders && task.reminders.length > 0 ? (
                      <span className="inline-flex items-center gap-1 text-xs text-indigo-600 font-medium">
                        <Bell className="h-3.5 w-3.5" /> {task.reminders.length}
                      </span>
                    ) : null}
                  </div>
                  <h3 className="mt-2 text-base font-semibold text-gray-950">{task.title}</h3>
                  {task.description ? <p className="mt-1 text-sm text-gray-600">{task.description}</p> : null}
                </div>

                <div className="flex flex-wrap gap-2">
                  <button
                    type="button"
                    disabled={actionTaskId === task.id}
                    onClick={() => void onStatusChange(task, nextStatus[task.status])}
                    className="rounded-lg bg-emerald-50 px-3 py-2 text-xs font-semibold text-emerald-700 transition hover:bg-emerald-100 disabled:opacity-60"
                  >
                    {t('tasks.moveTo', { status: statusLabel[nextStatus[task.status]] })}
                  </button>
                  <button
                    type="button"
                    disabled={actionTaskId === task.id}
                    onClick={() => void onFavoriteToggle(task)}
                    className={cn(
                      'inline-flex items-center gap-1 rounded-lg px-3 py-2 text-xs font-semibold transition disabled:opacity-60',
                      task.isFavorite
                        ? 'bg-amber-50 text-amber-700 hover:bg-amber-100'
                        : 'bg-slate-100 text-slate-700 hover:bg-slate-200'
                    )}
                  >
                    <Star className={cn('h-3.5 w-3.5', task.isFavorite && 'fill-amber-400 text-amber-400')} />
                    {task.isFavorite ? t('tasks.unfavorite') : t('tasks.favorite')}
                  </button>
                  <button
                    type="button"
                    onClick={() => onEdit(task)}
                    className="inline-flex items-center gap-1 rounded-lg bg-gray-100 px-3 py-2 text-xs font-semibold text-gray-700 transition hover:bg-gray-200"
                  >
                    <Pencil className="h-3.5 w-3.5" /> {t('tasks.edit')}
                  </button>
                  <button
                    type="button"
                    disabled={actionTaskId === task.id}
                    onClick={() => void onArchive(task)}
                    className="inline-flex items-center gap-1 rounded-lg bg-red-50 px-3 py-2 text-xs font-semibold text-red-700 transition hover:bg-red-100 disabled:opacity-60"
                  >
                    <Trash2 className="h-3.5 w-3.5" /> {t('tasks.archive')}
                  </button>
                </div>
              </div>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
