import { useEffect, useState } from 'react'
import { X, Plus, Trash2 } from 'lucide-react'
import type { TaskDto, ActivityLogDto, TaskStatus } from '../../../types/task'
import type { SubTaskDto } from '../../../types/subtask'
import { tasksService } from '../../../services/tasksService'
import { subTasksService } from '../../../services/subTasksService'

interface TaskDetailDrawerProps {
  task: TaskDto | null
  isOpen: boolean
  onClose: () => void
  onUpdated: () => void
}

export function TaskDetailDrawer({ task, isOpen, onClose, onUpdated }: TaskDetailDrawerProps) {
  const [subTasks, setSubTasks] = useState<SubTaskDto[]>([])
  const [activity, setActivity] = useState<ActivityLogDto[]>([])
  const [newSubTaskTitle, setNewSubTaskTitle] = useState('')
  const [assigneeIds, setAssigneeIds] = useState('')
  const [loading, setLoading] = useState(false)

  useEffect(() => {
    if (!task || !isOpen) return
    setAssigneeIds(task.assignees?.map((a) => a.id).join(', ') || '')
    setLoading(true)
    Promise.all([
      subTasksService.getSubTasks(task.id).catch(() => []),
      tasksService.getActivityLog(task.id).catch(() => []),
    ])
      .then(([subTaskData, activityData]) => {
        setSubTasks(subTaskData)
        setActivity(activityData)
      })
      .finally(() => setLoading(false))
  }, [task, isOpen])

  if (!isOpen || !task) return null

  const completedCount = subTasks.filter((st) => st.status === 'Done').length
  const progress = subTasks.length ? Math.round((completedCount / subTasks.length) * 100) : 0

  async function handleAssigneesSave() {
    if (!task) return
    await tasksService.updateTask(task.id, {
      title: task.title,
      description: task.description,
      dueDate: task.dueDate,
      priority: task.priority,
      assigneeIds: assigneeIds.split(',').map((id) => id.trim()).filter(Boolean),
    })
    onUpdated()
  }

  async function handleCreateSubTask() {
    if (!task) return
    const title = newSubTaskTitle.trim()
    if (!title) return
    const created = await subTasksService.createSubTask(task.id, { title })
    setSubTasks((current) => [...current, created])
    setNewSubTaskTitle('')
    onUpdated()
  }

  async function handleSubTaskStatus(subTask: SubTaskDto, status: TaskStatus) {
    if (!task) return
    const mappedStatus = status === 'Done' ? 'Done' : status === 'InProgress' ? 'InProgress' : 'Todo'
    const updated = await subTasksService.updateSubTask(task.id, subTask.id, {
      title: subTask.title,
      status: mappedStatus,
      assigneeId: subTask.assigneeId,
    })
    setSubTasks((current) => current.map((item) => item.id === updated.id ? updated : item))
    onUpdated()
  }

  async function handleDeleteSubTask(subTaskId: string) {
    if (!task) return
    await subTasksService.deleteSubTask(task.id, subTaskId)
    setSubTasks((current) => current.filter((item) => item.id !== subTaskId))
    onUpdated()
  }

  return (
    <div className="fixed inset-0 z-50 overflow-hidden">
      <div className="absolute inset-0 bg-gray-900/30" onClick={onClose} />
      <aside className="absolute right-0 top-0 flex h-full w-full max-w-xl flex-col bg-white shadow-2xl">
        <div className="flex items-start justify-between border-b border-gray-200 p-6">
          <div>
            <h2 className="text-xl font-bold text-gray-950">{task.title}</h2>
            <p className="mt-1 text-sm text-gray-500">{task.description || 'No description'}</p>
          </div>
          <button onClick={onClose} className="rounded-lg p-2 text-gray-400 hover:bg-gray-100 hover:text-gray-600">
            <X className="h-5 w-5" />
          </button>
        </div>

        <div className="flex-1 overflow-y-auto p-6 space-y-8">
          <section>
            <h3 className="text-sm font-semibold uppercase tracking-wide text-gray-500">Assignees</h3>
            <div className="mt-3 flex gap-2">
              <input
                value={assigneeIds}
                onChange={(e) => setAssigneeIds(e.target.value)}
                placeholder="Comma-separated user IDs"
                className="flex-1 rounded-lg border border-gray-300 px-3 py-2 text-sm"
              />
              <button onClick={handleAssigneesSave} className="rounded-lg bg-indigo-600 px-4 py-2 text-sm font-semibold text-white hover:bg-indigo-700">
                Save
              </button>
            </div>
          </section>

          <section>
            <div className="flex items-center justify-between">
              <h3 className="text-sm font-semibold uppercase tracking-wide text-gray-500">Subtasks</h3>
              <span className="text-xs text-gray-500">{completedCount}/{subTasks.length} done</span>
            </div>
            <div className="mt-3 h-2 overflow-hidden rounded-full bg-gray-100">
              <div className="h-full bg-indigo-500" style={{ width: `${progress}%` }} />
            </div>
            <div className="mt-4 flex gap-2">
              <input
                value={newSubTaskTitle}
                onChange={(e) => setNewSubTaskTitle(e.target.value)}
                placeholder="Add subtask..."
                className="flex-1 rounded-lg border border-gray-300 px-3 py-2 text-sm"
              />
              <button onClick={handleCreateSubTask} className="rounded-lg border border-gray-300 p-2 hover:bg-gray-50">
                <Plus className="h-5 w-5" />
              </button>
            </div>
            <div className="mt-4 space-y-2">
              {subTasks.map((subTask) => (
                <div key={subTask.id} className="flex items-center gap-3 rounded-lg border border-gray-100 p-3">
                  <input
                    type="checkbox"
                    checked={subTask.status === 'Done'}
                    onChange={(e) => handleSubTaskStatus(subTask, e.target.checked ? 'Done' : 'Todo')}
                    className="h-4 w-4 rounded border-gray-300 text-indigo-600"
                  />
                  <span className={subTask.status === 'Done' ? 'flex-1 text-sm text-gray-400 line-through' : 'flex-1 text-sm text-gray-700'}>
                    {subTask.title}
                  </span>
                  <button onClick={() => handleDeleteSubTask(subTask.id)} className="text-gray-300 hover:text-red-500">
                    <Trash2 className="h-4 w-4" />
                  </button>
                </div>
              ))}
              {!loading && subTasks.length === 0 ? <p className="text-sm text-gray-400">No subtasks yet.</p> : null}
            </div>
          </section>

          <section>
            <h3 className="text-sm font-semibold uppercase tracking-wide text-gray-500">Activity</h3>
            <div className="mt-4 space-y-4 border-l border-gray-200 pl-4">
              {activity.map((item) => (
                <div key={item.id} className="relative">
                  <div className="absolute -left-[21px] top-1 h-2 w-2 rounded-full bg-indigo-500" />
                  <p className="text-sm text-gray-700">{item.message}</p>
                  <p className="text-xs text-gray-400">{item.actorName || 'System'} · {new Date(item.createdAt).toLocaleString()}</p>
                </div>
              ))}
              {!loading && activity.length === 0 ? <p className="text-sm text-gray-400">No activity yet.</p> : null}
            </div>
          </section>
        </div>
      </aside>
    </div>
  )
}
