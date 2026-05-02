import api from '../../../services/api'
import type { CreateNotePayload, Note, NotesPagedResult, UpdateNotePayload } from '../types'

export const notesApi = {
  async search(params: { query?: string; page?: number; pageSize?: number }): Promise<NotesPagedResult<Note>> {
    const { data } = await api.get<NotesPagedResult<Note>>('/api/notes', {
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

  async archive(id: string): Promise<void> {
    await api.delete(`/api/notes/${id}`)
  },
}
