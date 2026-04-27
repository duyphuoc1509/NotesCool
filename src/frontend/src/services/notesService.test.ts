import { beforeEach, describe, expect, it, vi } from 'vitest'
import { notesService } from './notesService'
import api from './api'

vi.mock('./api', () => ({
  default: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}))

describe('notesService', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('searches notes with default pagination params', async () => {
    const response = {
      data: {
        items: [{ id: '1', title: 'First', content: 'Body', createdAt: '2026-01-01T00:00:00Z' }],
        page: 1,
        pageSize: 20,
        totalCount: 1,
      },
    }

    vi.mocked(api.get).mockResolvedValue(response)

    await expect(notesService.search({})).resolves.toEqual(response.data)
    expect(api.get).toHaveBeenCalledWith('/api/notes', {
      params: {
        query: undefined,
        page: 1,
        pageSize: 20,
      },
    })
  })

  it('creates a note', async () => {
    const payload = { title: 'New note', content: 'Details' }
    const response = {
      data: { id: '2', ...payload, createdAt: '2026-01-01T00:00:00Z' },
    }

    vi.mocked(api.post).mockResolvedValue(response)

    await expect(notesService.create(payload)).resolves.toEqual(response.data)
    expect(api.post).toHaveBeenCalledWith('/api/notes', payload)
  })

  it('updates a note', async () => {
    const payload = { title: 'Updated', content: 'Changed' }
    const response = {
      data: { id: '3', ...payload, createdAt: '2026-01-01T00:00:00Z' },
    }

    vi.mocked(api.put).mockResolvedValue(response)

    await expect(notesService.update('3', payload)).resolves.toEqual(response.data)
    expect(api.put).toHaveBeenCalledWith('/api/notes/3', payload)
  })

  it('archives a note', async () => {
    vi.mocked(api.delete).mockResolvedValue({})

    await expect(notesService.archive('4')).resolves.toBeUndefined()
    expect(api.delete).toHaveBeenCalledWith('/api/notes/4')
  })
})
