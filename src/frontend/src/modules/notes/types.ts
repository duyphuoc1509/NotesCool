export interface Note {
  id: string
  title: string
  content: string
  createdAt: string
  updatedAt: string | null
  tags?: string[]
  isFavorite?: boolean
  isArchived?: boolean
}

export interface NotesPagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
}

export interface CreateNotePayload {
  title: string
  content: string
  tags?: string[]
  isFavorite?: boolean
}

export interface UpdateNotePayload {
  title: string
  content: string
  tags?: string[]
  isFavorite?: boolean
}

export interface UseNotesOptions {
  page: number
  pageSize: number
  query: string
}
