export interface Note {
  id: string
  title: string
  content: string
  createdAt: string
  updatedAt: string | null
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
}

export interface CreateNotePayload {
  title: string
  content: string
}

export interface UpdateNotePayload {
  title: string
  content: string
}
