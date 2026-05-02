import { useState, useEffect, useCallback } from 'react'
import { notesService } from '../services/notesService'
import type { Note, UseNotesOptions } from '../types'

export function useNotes({ page, pageSize, query }: UseNotesOptions) {
  const [notes, setNotes] = useState<Note[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [refreshCount, setRefreshCount] = useState(0)

  useEffect(() => {
    let cancelled = false

    async function load() {
      try {
        const result = await notesService.search({ query, page, pageSize })
        if (!cancelled) {
          setNotes(result.items)
          setTotalCount(result.totalCount)
          setError(null)
          setLoading(false)
        }
      } catch {
        if (!cancelled) {
          setError('Failed to load notes. Please try again.')
          setLoading(false)
        }
      }
    }

    setLoading(true)
    void load()

    return () => {
      cancelled = true
    }
  }, [query, page, pageSize, refreshCount])

  const refetch = useCallback(() => {
    setRefreshCount((c) => c + 1)
  }, [])

  return { notes, totalCount, loading, error, refetch }
}
