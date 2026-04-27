import api from './api'
import type { CreateNoteRequest, NoteResponse, UpdateNoteRequest } from '../types/note'
import type { PagedResult } from '../types/common'

const NOTES_BASE_PATH = '/api/notes'

export const notesService = {
  async getNotes(query?: string, page = 1, pageSize = 10): Promise<PagedResult<NoteResponse>> {
    const { data } = await api.get<PagedResult<NoteResponse>>(NOTES_BASE_PATH, {
      params: { query, page, pageSize },
    })
    return data
  },

  async getNoteById(id: string): Promise<NoteResponse> {
    const { data } = await api.get<NoteResponse>(`${NOTES_BASE_PATH}/${id}`)
    return data
  },

  async createNote(payload: CreateNoteRequest): Promise<NoteResponse> {
    const { data } = await api.post<NoteResponse>(NOTES_BASE_PATH, payload)
    return data
  },

  async updateNote(id: string, payload: UpdateNoteRequest): Promise<NoteResponse> {
    const { data } = await api.put<NoteResponse>(`${NOTES_BASE_PATH}/${id}`, payload)
    return data
  },

  async deleteNote(id: string): Promise<void> {
    await api.delete(`${NOTES_BASE_PATH}/${id}`)
  },
}

export default notesService
