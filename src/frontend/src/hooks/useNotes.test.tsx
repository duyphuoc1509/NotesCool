import { act } from 'react'
import { createRoot, type Root } from 'react-dom/client'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { useNotes } from './useNotes'
import { notesService } from '../services/notesService'
import type { Note } from '../types/note'

// @ts-expect-error IS_REACT_ACT_ENVIRONMENT is used by React internals for act() support
globalThis.IS_REACT_ACT_ENVIRONMENT = true

vi.mock('../services/notesService', () => ({
  notesService: {
    search: vi.fn(),
  },
}))

type UseNotesResult = ReturnType<typeof useNotes>

const sampleNote: Note = {
  id: 'note-1',
  title: 'Planning',
  content: 'Write the notes UI',
  createdAt: '2026-01-01T00:00:00Z',
  updatedAt: null,
}

function HookProbe({ onRender }: { onRender: (value: UseNotesResult) => void }) {
  onRender(useNotes({ page: 1, pageSize: 6, query: 'plan' }))
  return null
}

describe('useNotes', () => {
  let container: HTMLDivElement
  let root: Root

  beforeEach(() => {
    container = document.createElement('div')
    document.body.appendChild(container)
    root = createRoot(container)
    vi.clearAllMocks()
  })

  afterEach(() => {
    act(() => root.unmount())
    container.remove()
  })

  it('loads notes and exposes pagination metadata', async () => {
    const renders: UseNotesResult[] = []
    vi.mocked(notesService.search).mockResolvedValue({
      items: [sampleNote],
      page: 1,
      pageSize: 6,
      totalCount: 1,
    })

    await act(async () => {
      root.render(<HookProbe onRender={(value) => renders.push(value)} />)
      await Promise.resolve()
      await Promise.resolve()
    })

    const latest = renders.at(-1)
    expect(notesService.search).toHaveBeenCalledWith({ query: 'plan', page: 1, pageSize: 6 })
    expect(latest?.notes).toEqual([sampleNote])
    expect(latest?.totalCount).toBe(1)
    expect(latest?.loading).toBe(false)
    expect(latest?.error).toBeNull()
  })

  it('reports load failures', async () => {
    const renders: UseNotesResult[] = []
    vi.mocked(notesService.search).mockRejectedValue(new Error('Network error'))

    await act(async () => {
      root.render(<HookProbe onRender={(value) => renders.push(value)} />)
      await Promise.resolve()
      await Promise.resolve()
    })

    const latest = renders.at(-1)
    expect(latest?.notes).toEqual([])
    expect(latest?.totalCount).toBe(0)
    expect(latest?.loading).toBe(false)
    expect(latest?.error).toBe('Failed to load notes. Please try again.')
  })
})
