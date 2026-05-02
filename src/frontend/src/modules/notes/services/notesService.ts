import { notesApi } from '../infrastructure/notesApi'
import type { CreateNotePayload, Note, NotesPagedResult, UpdateNotePayload } from '../types'

export const notesService = {
  search(params: { query?: string; page?: number; pageSize?: number }): Promise<NotesPagedResult<Note>> {
    return notesApi.search(params)
  },

  getById(id: string): Promise<Note> {
    return notesApi.getById(id)
  },

  create(payload: CreateNotePayload): Promise<Note> {
    return notesApi.create(payload)
  },

  update(id: string, payload: UpdateNotePayload): Promise<Note> {
    return notesApi.update(id, payload)
  },

  archive(id: string): Promise<void> {
    return notesApi.archive(id)
  },
}
