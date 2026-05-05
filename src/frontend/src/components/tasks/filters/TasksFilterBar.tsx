import { Search } from 'lucide-react'
import type { TaskPriority, TaskStatus, TasksFilter } from '../../../types/task'

interface TasksFilterBarProps {
  filter: TasksFilter
  onFilterChange: (filter: Partial<TasksFilter>) => void
}

export function TasksFilterBar({ filter, onFilterChange }: TasksFilterBarProps) {
  return (
    <div className="flex flex-wrap items-center gap-4 py-4">
      <div className="relative max-w-xs flex-1 min-w-[200px]">
        <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-gray-400" />
        <input
          type="text"
          placeholder="Search keyword..."
          value={filter.keyword || ''}
          onChange={(e) => onFilterChange({ keyword: e.target.value })}
          className="w-full rounded-md border border-gray-300 pl-9 pr-4 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
        />
      </div>
      
      <select
        value={filter.status || ''}
        onChange={(e) => onFilterChange({ status: e.target.value ? (e.target.value as TaskStatus) : undefined })}
        className="rounded-md border border-gray-300 py-2 pl-3 pr-8 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
      >
        <option value="">All Statuses</option>
        <option value="Todo">Todo</option>
        <option value="InProgress">In Progress</option>
        <option value="InReview">In Review</option>
        <option value="Done">Done</option>
        <option value="Blocked">Blocked</option>
      </select>

      <select
        value={filter.priority || ''}
        onChange={(e) => onFilterChange({ priority: e.target.value ? (e.target.value as TaskPriority) : undefined })}
        className="rounded-md border border-gray-300 py-2 pl-3 pr-8 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
      >
        <option value="">All Priorities</option>
        <option value="Low">Low</option>
        <option value="Medium">Medium</option>
        <option value="High">High</option>
        <option value="Urgent">Urgent</option>
      </select>

      <input
        type="text"
        placeholder="Assignee ID..."
        value={filter.assigneeId || ''}
        onChange={(e) => onFilterChange({ assigneeId: e.target.value })}
        className="rounded-md border border-gray-300 px-4 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500 max-w-[150px]"
      />
    </div>
  )
}
