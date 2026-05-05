import { MoreHorizontal, Star } from 'lucide-react'
import type { TaskDto, TaskStatus, TaskPriority } from '../../../types/task'
import { cn } from '../../../utils/cn'

interface TaskTableViewProps {
  tasks: TaskDto[]
  onStatusChange: (task: TaskDto, status: TaskStatus) => void
  onTaskClick: (task: TaskDto) => void
  onFavoriteToggle: (task: TaskDto) => void
}

const statusColors: Record<TaskStatus, string> = {
  Todo: 'bg-gray-100 text-gray-700',
  InProgress: 'bg-blue-100 text-blue-700',
  InReview: 'bg-purple-100 text-purple-700',
  Done: 'bg-green-100 text-green-700',
  Blocked: 'bg-red-100 text-red-700',
  Archived: 'bg-orange-100 text-orange-700',
}

const priorityColors: Record<TaskPriority, string> = {
  Low: 'text-gray-500',
  Medium: 'text-blue-500',
  High: 'text-orange-500',
  Urgent: 'text-red-600 font-bold',
}

export function TaskTableView({ tasks, onStatusChange, onTaskClick, onFavoriteToggle }: TaskTableViewProps) {
  return (
    <div className="overflow-x-auto rounded-xl border border-gray-200 bg-white shadow-sm">
      <table className="w-full text-left text-sm text-gray-500">
        <thead className="bg-gray-50 text-xs font-semibold uppercase text-gray-600">
          <tr>
            <th className="px-4 py-3">Title</th>
            <th className="px-4 py-3">Status</th>
            <th className="px-4 py-3">Priority</th>
            <th className="px-4 py-3">Assignees</th>
            <th className="px-4 py-3">Subtasks</th>
            <th className="px-4 py-3">Due Date</th>
            <th className="px-4 py-3 text-right">Actions</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-gray-100">
          {tasks.map((task) => (
            <tr key={task.id} className="hover:bg-gray-50">
              <td className="px-4 py-3">
                <div className="flex items-center gap-2">
                  <button onClick={() => onFavoriteToggle(task)} className="focus:outline-none">
                    <Star className={cn("h-4 w-4", task.isFavorite ? "fill-amber-400 text-amber-400" : "text-gray-300")} />
                  </button>
                  <span 
                    className="cursor-pointer font-medium text-gray-900 hover:text-indigo-600"
                    onClick={() => onTaskClick(task)}
                  >
                    {task.title}
                  </span>
                </div>
              </td>
              <td className="px-4 py-3">
                <select
                  value={task.status}
                  onChange={(e) => onStatusChange(task, e.target.value as TaskStatus)}
                  className={cn("rounded-full px-2.5 py-0.5 text-xs font-medium border-none focus:ring-0 cursor-pointer", statusColors[task.status])}
                >
                  <option value="Todo">Todo</option>
                  <option value="InProgress">In Progress</option>
                  <option value="InReview">In Review</option>
                  <option value="Done">Done</option>
                  <option value="Blocked">Blocked</option>
                </select>
              </td>
              <td className="px-4 py-3">
                <span className={cn("text-xs", task.priority ? priorityColors[task.priority] : "text-gray-400")}>
                  {task.priority || 'None'}
                </span>
              </td>
              <td className="px-4 py-3">
                <div className="flex -space-x-2">
                  {task.assignees?.map((a) => (
                    <div key={a.id} className="flex h-6 w-6 items-center justify-center rounded-full bg-indigo-100 text-[10px] font-medium text-indigo-700 ring-2 ring-white" title={a.displayName}>
                      {a.displayName.slice(0, 2).toUpperCase()}
                    </div>
                  ))}
                  {!task.assignees?.length && <span className="text-gray-300">Unassigned</span>}
                </div>
              </td>
              <td className="px-4 py-3">
                {task.subTasksCount ? (
                  <div className="flex items-center gap-1.5">
                    <div className="h-1.5 w-16 overflow-hidden rounded-full bg-gray-100">
                      <div 
                        className="h-full bg-indigo-500" 
                        style={{ width: `${(task.subTasksCompleted || 0) / task.subTasksCount * 100}%` }}
                      />
                    </div>
                    <span className="text-xs">{task.subTasksCompleted}/{task.subTasksCount}</span>
                  </div>
                ) : <span className="text-gray-300">-</span>}
              </td>
              <td className="px-4 py-3 text-gray-400">
                {task.dueDate ? new Date(task.dueDate).toLocaleDateString() : '-'}
              </td>
              <td className="px-4 py-3 text-right">
                <button className="text-gray-400 hover:text-gray-600">
                  <MoreHorizontal className="h-5 w-5" />
                </button>
              </td>
            </tr>
          ))}
          {tasks.length === 0 && (
            <tr>
              <td colSpan={7} className="px-4 py-8 text-center text-gray-400">No tasks found matching criteria</td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  )
}
