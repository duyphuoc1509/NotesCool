import { useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import { AlertCircle, FileText, Loader2, Plus, Search } from 'lucide-react'
import { useNotes } from '../hooks/useNotes'
import { notesService } from '../services/notesService'
import type { Note } from '../types/note'
import { NoteCard } from '../components/notes/NoteCard'
import { NoteForm } from '../components/notes/NoteForm'
import { Modal } from '../components/Modal'

const PAGE_SIZE = 6

export function NotesPage() {
  const [queryInput, setQueryInput] = useState('')
  const [query, setQuery] = useState('')
  const [page, setPage] = useState(1)
  const [isCreateOpen, setIsCreateOpen] = useState(false)
  const [editingNote, setEditingNote] = useState<Note | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [actionError, setActionError] = useState<string | null>(null)

  const { notes, totalCount, loading, error, refetch } = useNotes({ page, pageSize: PAGE_SIZE, query })

  const totalPages = useMemo(() => Math.max(1, Math.ceil(totalCount / PAGE_SIZE)), [totalCount])

  const handleSearchSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setPage(1)
    setQuery(queryInput.trim())
  }

  const handleCreate = async (values: { title: string; content: string }) => {
    setIsSubmitting(true)
    setActionError(null)
    try {
      await notesService.create(values)
      setIsCreateOpen(false)
      setQueryInput('')
      setQuery('')
      setPage(1)
      await refetch()
    } catch {
      setActionError('Unable to create note. Please try again.')
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleUpdate = async (values: { title: string; content: string }) => {
    if (!editingNote) return

    setIsSubmitting(true)
    setActionError(null)
    try {
      await notesService.update(editingNote.id, values)
      setEditingNote(null)
      await refetch()
    } catch {
      setActionError('Unable to update note. Please try again.')
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleArchive = async (id: string) => {
    const confirmed = window.confirm('Archive this note?')
    if (!confirmed) return

    setActionError(null)
    try {
      await notesService.archive(id)
      if (notes.length === 1 && page > 1) {
        setPage((currentPage) => currentPage - 1)
      } else {
        await refetch()
      }
    } catch {
      setActionError('Unable to archive note. Please try again.')
    }
  }

  return (
    <div className="space-y-6">
      <section className="rounded-2xl border border-gray-200 bg-white p-6 shadow-sm">
        <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
          <div>
            <p className="text-sm font-semibold uppercase tracking-wide text-indigo-600">Notes</p>
            <h1 className="mt-2 text-3xl font-bold tracking-tight text-gray-950">Manage your notes</h1>
            <p className="mt-2 text-sm text-gray-600">Search, create, edit, archive, and paginate through your note library.</p>
          </div>

          <button
            type="button"
            onClick={() => {
              setActionError(null)
              setIsCreateOpen(true)
            }}
            className="inline-flex items-center justify-center gap-2 rounded-xl bg-indigo-600 px-4 py-3 text-sm font-semibold text-white transition hover:bg-indigo-700"
          >
            <Plus className="h-4 w-4" />
            New note
          </button>
        </div>

        <form onSubmit={handleSearchSubmit} className="mt-6 flex flex-col gap-3 sm:flex-row">
          <div className="relative flex-1">
            <Search className="pointer-events-none absolute left-4 top-1/2 h-4 w-4 -translate-y-1/2 text-gray-400" />
            <input
              type="search"
              value={queryInput}
              onChange={(event) => setQueryInput(event.target.value)}
              placeholder="Search by title or content"
              className="w-full rounded-xl border border-gray-300 bg-white py-3 pl-11 pr-4 text-sm text-gray-900 outline-none transition focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500/20"
            />
          </div>
          <button
            type="submit"
            className="rounded-xl border border-gray-300 bg-white px-4 py-3 text-sm font-medium text-gray-700 transition hover:bg-gray-50"
          >
            Search
          </button>
        </form>
      </section>

      {actionError ? (
        <div className="flex items-start gap-3 rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
          <AlertCircle className="mt-0.5 h-4 w-4 shrink-0" />
          <span>{actionError}</span>
        </div>
      ) : null}

      {error ? (
        <div className="rounded-2xl border border-rose-200 bg-rose-50 px-6 py-5 text-rose-700">
          <p className="font-semibold">Unable to load notes.</p>
          <p className="mt-1 text-sm">{error}</p>
          <button
            type="button"
            onClick={() => void refetch()}
            className="mt-4 rounded-xl bg-rose-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-rose-700"
          >
            Retry
          </button>
        </div>
      ) : null}

      {loading ? (
        <div className="flex min-h-[280px] items-center justify-center rounded-2xl border border-dashed border-gray-300 bg-white">
          <div className="flex items-center gap-3 text-gray-500">
            <Loader2 className="h-5 w-5 animate-spin" />
            <span className="text-sm font-medium">Loading notes...</span>
          </div>
        </div>
      ) : !error && notes.length === 0 ? (
        <div className="rounded-2xl border border-dashed border-gray-300 bg-white px-6 py-16 text-center shadow-sm">
          <div className="mx-auto flex h-14 w-14 items-center justify-center rounded-full bg-indigo-50 text-indigo-600">
            <FileText className="h-7 w-7" />
          </div>
          <h2 className="mt-4 text-xl font-semibold text-gray-900">No notes found</h2>
          <p className="mt-2 text-sm text-gray-600">
            {query ? 'Try a different keyword or create a new note.' : 'Create your first note to get started.'}
          </p>
        </div>
      ) : !error ? (
        <>
          <section className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
            {notes.map((note) => (
              <NoteCard key={note.id} note={note} onEdit={setEditingNote} onArchive={(id) => void handleArchive(id)} />
            ))}
          </section>

          <section className="flex flex-col gap-3 rounded-2xl border border-gray-200 bg-white px-5 py-4 shadow-sm sm:flex-row sm:items-center sm:justify-between">
            <p className="text-sm text-gray-600">
              Showing <span className="font-semibold text-gray-900">{notes.length}</span> of{' '}
              <span className="font-semibold text-gray-900">{totalCount}</span> notes
            </p>
            <div className="flex items-center gap-2">
              <button
                type="button"
                onClick={() => setPage((currentPage) => Math.max(1, currentPage - 1))}
                disabled={page === 1}
                className="rounded-xl border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 transition hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-50"
              >
                Previous
              </button>
              <span className="px-3 text-sm font-medium text-gray-700">
                Page {page} / {totalPages}
              </span>
              <button
                type="button"
                onClick={() => setPage((currentPage) => Math.min(totalPages, currentPage + 1))}
                disabled={page >= totalPages}
                className="rounded-xl border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 transition hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-50"
              >
                Next
              </button>
            </div>
          </section>
        </>
      ) : null}

      <Modal isOpen={isCreateOpen} onClose={() => setIsCreateOpen(false)} title="Create note">
        <NoteForm onSubmit={handleCreate} onCancel={() => setIsCreateOpen(false)} isSubmitting={isSubmitting} />
      </Modal>

      <Modal isOpen={!!editingNote} onClose={() => setEditingNote(null)} title="Edit note">
        <NoteForm
          initialValues={editingNote ? { title: editingNote.title, content: editingNote.content } : undefined}
          onSubmit={handleUpdate}
          onCancel={() => setEditingNote(null)}
          isSubmitting={isSubmitting}
        />
      </Modal>
    </div>
  )
}
