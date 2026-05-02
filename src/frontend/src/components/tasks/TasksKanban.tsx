import { useState, type DragEvent } from 'react'
import { Calendar, MoreVertical } from 'lucide-react'
import type { TaskDto, TaskStatus } from '../../types/task'
import { cn } from '../../utils/cn'

const COLUMNS: { id: TaskStatus; label: string }[] = [
  { id: 'Todo', label: 'Todo' },
  { id: 'InProgress', label: 'In Progress' },
  { id: 'InReview', label: 'In Review' },
  { id: 'Done', label: 'Done' },
  { id: 'Blocked', label: 'Blocked / Cancelled' },
]

interface TasksKanbanProps {
  tasks: TaskDto[]
  isLoading: boolean
  error: string | null
  onStatusChange: (task: TaskDto, newStatus: TaskStatus) => Promise<void>
  onTaskClick: (task: TaskDto) => void
}

function formatDate(value?: string | null) {
  if (!value) return 'No due date'
  return new Intl.DateTimeFormat('en', { month: 'short', day: 'numeric' }).format(new Date(value))
}

export function TasksKanban({ tasks, isLoading, error, onStatusChange, onTaskClick }: TasksKanbanProps) {
  const [movingTaskId, setMovingTaskId] = useState<string | null>(null)
  const [draggingOverColumn, setDraggingOverColumn] = useState<TaskStatus | null>(null)

  if (error) {
    return (
      <div className="rounded-xl border border-red-200 bg-red-50 p-4 text-sm text-red-700">
        <p className="font-semibold">Could not load tasks</p>
        <p className="mt-1">{error}</p>
      </div>
    )
  }

  if (isLoading) {
    return (
      <div className="flex gap-4 overflow-x-auto pb-4">
        {COLUMNS.map((column) => (
          <div key={column.id} className="w-80 shrink-0 space-y-3 rounded-xl bg-gray-50 p-4">
            <h3 className="font-semibold text-gray-700">{column.label}</h3>
            {Array.from({ length: 3 }).map((_, index) => (
              <div key={index} className="h-24 animate-pulse rounded-lg bg-gray-200" />
            ))}
          </div>
        ))}
      </div>
    )
  }

  const tasksByColumn = COLUMNS.reduce(
    (acc, column) => {
      acc[column.id] = tasks.filter((task) => task.status === column.id)
      return acc
    },
    {} as Partial<Record<TaskStatus, TaskDto[]>>,
  )

  const handleStatusChange = async (task: TaskDto, newStatus: TaskStatus) => {
    if (task.status === newStatus) return
    setMovingTaskId(task.id)
    try {
      await onStatusChange(task, newStatus)
    } finally {
      setMovingTaskId(null)
    }
  }

  // ── drag-and-drop handlers ──────────────────────────────────────────────────

  const handleDragStart = (event: DragEvent<HTMLElement>, task: TaskDto) => {
    event.dataTransfer.setData('application/json', JSON.stringify(task))
    event.dataTransfer.effectAllowed = 'move'
  }

  const handleDragOver = (event: DragEvent<HTMLDivElement>, columnId: TaskStatus) => {
    event.preventDefault()
    event.dataTransfer.dropEffect = 'move'
    setDraggingOverColumn(columnId)
  }

  const handleDragLeave = () => {
    setDraggingOverColumn(null)
  }

  const handleDrop = (event: DragEvent<HTMLDivElement>, targetStatus: TaskStatus) => {
    event.preventDefault()
    setDraggingOverColumn(null)
    try {
      const task = JSON.parse(event.dataTransfer.getData('application/json')) as TaskDto
      void handleStatusChange(task, targetStatus)
    } catch {
      // ignore malformed drag data
    }
  }

  // ───────────────────────────────────────────────────────────────────────────

  return (
    <div className="flex gap-4 overflow-x-auto pb-4 snap-x">
      {COLUMNS.map((column) => (
        <div
          key={column.id}
          onDragOver={(event) => handleDragOver(event, column.id)}
          onDragLeave={handleDragLeave}
          onDrop={(event) => handleDrop(event, column.id)}
          className={cn(
            'flex min-h-[500px] w-80 shrink-0 snap-center flex-col rounded-xl border p-4 transition-colors',
            draggingOverColumn === column.id
              ? 'border-indigo-400 bg-indigo-50/60'
              : 'border-gray-200/60 bg-gray-50/50',
          )}
        >
          <div className="mb-4 flex items-center justify-between">
            <h3 className="flex items-center gap-2 font-semibold text-gray-700">
              {column.label}
              <span className="rounded-full bg-gray-200 px-2 py-0.5 text-xs text-gray-600">
                {tasksByColumn[column.id]?.length || 0}
              </span>
            </h3>
          </div>

          <div className="flex-1 space-y-3">
            {tasksByColumn[column.id]?.length === 0 ? (
              <div className={cn(
                'flex h-32 flex-col items-center justify-center rounded-lg border-2 border-dashed text-gray-400 transition-colors',
                draggingOverColumn === column.id ? 'border-indigo-300 bg-indigo-50' : 'border-gray-200',
              )}>
                <span className="text-sm">Drop tasks here</span>
              </div>
            ) : (
              tasksByColumn[column.id]?.map((task) => (
                <article
                  key={task.id}
                  draggable
                  onDragStart={(event) => handleDragStart(event, task)}
                  onClick={() => onTaskClick(task)}
                  className={cn(
                    'group relative flex cursor-grab active:cursor-grabbing flex-col gap-2 rounded-lg border border-gray-200 bg-white p-3 shadow-sm transition hover:border-indigo-300 hover:shadow-md',
                    movingTaskId === task.id && 'pointer-events-none opacity-50',
                  )}
                >
                  <div className="flex items-start justify-between gap-2">
                    <h4 className="line-clamp-2 text-sm font-medium leading-snug text-gray-900">{task.title}</h4>
                    <div className="relative">
                      <select
                        value={task.status}
                        onChange={(event) => {
                          event.stopPropagation()
                          void handleStatusChange(task, event.target.value as TaskStatus)
                        }}
                        onClick={(event) => event.stopPropagation()}
                        className="absolute inset-0 h-full w-full cursor-pointer opacity-0"
                        title="Change status"
                      >
                        {COLUMNS.map((status) => (
                          <option key={status.id} value={status.id}>
                            {status.label}
                          </option>
                        ))}
                      </select>
                      <button type="button" className="-mr-1 p-1 text-gray-400 transition hover:text-indigo-600">
                        <MoreVertical className="h-4 w-4" />
                      </button>
                    </div>
                  </div>

                  {task.description ? <p className="line-clamp-2 text-xs text-gray-500">{task.description}</p> : null}

                  <div className="mt-1 flex items-center gap-1.5 text-xs font-medium text-gray-500">
                    <Calendar className="h-3.5 w-3.5 text-gray-400" />
                    <span>{formatDate(task.dueDate)}</span>
                  </div>
                </article>
              ))
            )}
          </div>
        </div>
      ))}
    </div>
  )
}
