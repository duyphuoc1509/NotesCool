import { act } from 'react'
import { createRoot, type Root } from 'react-dom/client'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { TasksKanban } from './TasksKanban'
import type { TaskDto } from '../../types/task'

// @ts-expect-error IS_REACT_ACT_ENVIRONMENT is used by React internals for act() support
globalThis.IS_REACT_ACT_ENVIRONMENT = true

const mockTasks: TaskDto[] = [
  { id: '1', title: 'Task 1', status: 'Todo', createdAt: '' },
]

describe('TasksKanban', () => {
  let container: HTMLDivElement
  let root: Root

  beforeEach(() => {
    container = document.createElement('div')
    document.body.appendChild(container)
    root = createRoot(container)
  })

  afterEach(() => {
    act(() => root.unmount())
    container.remove()
  })

  it('renders task cards with draggable attribute', async () => {
    await act(async () => {
      root.render(
        <TasksKanban
          tasks={mockTasks}
          isLoading={false}
          error={null}
          onStatusChange={vi.fn()}
          onTaskClick={vi.fn()}
        />
      )
    })

    const taskCard = container.querySelector('article')
    expect(taskCard?.getAttribute('draggable')).toBe('true')
  })

  it('calls onStatusChange when dropped on a column', async () => {
    const onStatusChange = vi.fn()
    await act(async () => {
      root.render(
        <TasksKanban
          tasks={mockTasks}
          isLoading={false}
          error={null}
          onStatusChange={onStatusChange}
          onTaskClick={vi.fn()}
        />
      )
    })

    const taskCard = container.querySelector('article')
    // Drop on "InProgress" column (index 1)
    const columns = container.querySelectorAll('.flex.min-h-\\[500px\\]')
    const inProgressColumn = columns[1]

    await act(async () => {
      /* eslint-disable @typescript-eslint/no-explicit-any */
      // Simulate drag start
      const dragStartEvent = new Event('dragstart', { bubbles: true })
      ;(dragStartEvent as any).dataTransfer = {
        setData: vi.fn(),
        effectAllowed: '',
      }
      taskCard?.dispatchEvent(dragStartEvent)

      // Simulate drop
      const dropEvent = new Event('drop', { bubbles: true })
      ;(dropEvent as any).dataTransfer = {
        getData: (key: string) => (key === 'application/json' ? JSON.stringify(mockTasks[0]) : ''),
      }
      inProgressColumn?.dispatchEvent(dropEvent)
      /* eslint-enable @typescript-eslint/no-explicit-any */

      await Promise.resolve()
    })

    expect(onStatusChange).toHaveBeenCalledWith(mockTasks[0], 'InProgress')
  })
})
