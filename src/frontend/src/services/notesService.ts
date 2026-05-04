import api from './api'
import type { Note, PagedResult, CreateNotePayload, UpdateNotePayload } from '../types/note'

export const notesService = {
  async search(params: { query?: string; page?: number; pageSize?: number }): Promise<PagedResult<Note>> {
    const { data } = await api.get<PagedResult<Note>>('/api/notes', {
      params: {
        query: params.query || undefined,
        page: params.page ?? 1,
        pageSize: params.pageSize ?? 20,
      },
    })
    return data
  },

  async getById(id: string): Promise<Note> {
    const { data } = await api.get<Note>(`/api/notes/${id}`)
    return data
  },

  async create(payload: CreateNotePayload): Promise<Note> {
    const { data } = await api.post<Note>('/api/notes', payload)
    return data
  },

  async update(id: string, payload: UpdateNotePayload): Promise<Note> {
    const { data } = await api.put<Note>(`/api/notes/${id}`, payload)
    return data
  },

  async setFavorite(id: string, isFavorite: boolean): Promise<Note> {
    const { data } = await api.patch<Note>(`/api/notes/${id}/favorite`, { isFavorite })
    return data
  },

  async archive(id: string): Promise<void> {
    await api.delete(`/api/notes/${id}`)
  },
}
