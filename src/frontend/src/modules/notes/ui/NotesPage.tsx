import { useCallback, useMemo, useRef, useState } from 'react'
import { AlertCircle, BookOpen, FileText, Loader2, Plus, Search, Star, Archive } from 'lucide-react'
import { useNotes } from '../hooks/useNotes'
import { notesService } from '../services/notesService'
import type { Note } from '../types'
import { NoteCard } from './NoteCard'
import { NoteForm } from './NoteForm'
import { cn } from '../../../utils/cn'

const PAGE_SIZE = 20

type FilterTab = 'all' | 'favorites' | 'archived'

function NoteCardSkeleton() {
  return (
    <div className="animate-pulse rounded-2xl border border-slate-200 bg-white p-4">
      <div className="flex items-start gap-3">
        <div className="h-11 w-11 rounded-xl bg-slate-100" />
        <div className="flex-1 space-y-2">
          <div className="h-4 w-2/3 rounded bg-slate-100" />
          <div className="h-3 rounded bg-slate-100" />
          <div className="h-3 w-4/5 rounded bg-slate-100" />
        </div>
      </div>
      <div className="mt-4 flex items-center justify-between">
        <div className="h-3 w-24 rounded bg-slate-100" />
        <div className="h-3 w-12 rounded bg-slate-100" />
      </div>
    </div>
  )
}

function EmptyState({
  query,
  filterTab,
  onCreateNote,
}: {
  query: string
  filterTab: FilterTab
  onCreateNote: () => void
}) {
  if (query) {
    return (
      <div className="flex flex-col items-center justify-center px-6 py-16 text-center">
        <Search className="mb-4 h-12 w-12 text-slate-300" />
        <h3 className="text-base font-semibold text-slate-800">No results found</h3>
        <p className="mt-1 text-sm text-slate-500">Try a different keyword or clear the search.</p>
      </div>
    )
  }

  if (filterTab === 'favorites') {
    return (
      <div className="flex flex-col items-center justify-center px-6 py-16 text-center">
        <Star className="mb-4 h-12 w-12 text-slate-300" />
        <h3 className="text-base font-semibold text-slate-800">No favorite notes yet</h3>
        <p className="mt-1 text-sm text-slate-500">Star a note to find it here quickly.</p>
      </div>
    )
  }

  if (filterTab === 'archived') {
    return (
      <div className="flex flex-col items-center justify-center px-6 py-16 text-center">
        <Archive className="mb-4 h-12 w-12 text-slate-300" />
        <h3 className="text-base font-semibold text-slate-800">Nothing archived yet</h3>
        <p className="mt-1 text-sm text-slate-500">Archived notes will appear here.</p>
      </div>
    )
  }

  return (
    <div className="flex flex-col items-center justify-center px-6 py-16 text-center">
      <div className="mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-indigo-50">
        <FileText className="h-8 w-8 text-indigo-400" />
      </div>
      <h3 className="text-lg font-semibold text-slate-900">You don&apos;t have any notes yet.</h3>
      <p className="mt-2 max-w-xs text-sm text-slate-500">
        Create your first note to capture ideas, tasks, and reminders.
      </p>
      <button
        type="button"
        onClick={onCreateNote}
        className="mt-6 inline-flex items-center gap-2 rounded-xl bg-indigo-600 px-5 py-2.5 text-sm font-semibold text-white transition hover:bg-indigo-700"
      >
        <Plus className="h-4 w-4" />
        Create note
      </button>
    </div>
  )
}

export function NotesPage() {
  const [queryInput, setQueryInput] = useState('')
  const [query, setQuery] = useState('')
  const [filterTab, setFilterTab] = useState<FilterTab>('all')
  const [selectedNote, setSelectedNote] = useState<Note | null>(null)
  const [isCreating, setIsCreating] = useState(false)
  const [mobileScreen, setMobileScreen] = useState<'list' | 'editor'>('list')
  const [actionError, setActionError] = useState<string | null>(null)

  const { notes, totalCount, loading, error, refetch } = useNotes({ page: 1, pageSize: PAGE_SIZE, query })
  const searchInputRef = useRef<HTMLInputElement>(null)

  const displayedNotes = useMemo(() => {
    if (filterTab === 'favorites') {
      return notes.filter((n) => n.isFavorite)
    }
    if (filterTab === 'archived') {
      return notes.filter((n) => n.isArchived)
    }
    return notes.filter((n) => !n.isArchived)
  }, [notes, filterTab])

  const searchNote = useCallback(
    (e: React.FormEvent) => {
      e.preventDefault()
      setQuery(queryInput.trim())
    },
    [queryInput]
  )

  const openNote = useCallback((note: Note) => {
    setSelectedNote(note)
    setIsCreating(false)
    setMobileScreen('editor')
  }, [])

  const openNewNote = useCallback(() => {
    setSelectedNote(null)
    setIsCreating(true)
    setMobileScreen('editor')
  }, [])

  const closeEditor = useCallback(() => {
    setSelectedNote(null)
    setIsCreating(false)
    setMobileScreen('list')
  }, [])

  const handleSave = useCallback(
    async (
      id: string | undefined,
      values: { title: string; content: string; tags?: string[]; isFavorite?: boolean }
    ) => {
      setActionError(null)
      try {
        if (id) {
          const updated = await notesService.update(id, values)
          setSelectedNote(updated)
        } else {
          if (!values.title.trim() && !values.content.trim()) return
          const created = await notesService.create(values)
          setSelectedNote(created)
          setIsCreating(false)
        }
        refetch()
      } catch {
        setActionError('Unable to save note. Please try again.')
      }
    },
    [refetch]
  )

  const handleArchive = useCallback(
    async (id: string) => {
      setActionError(null)
      try {
        await notesService.archive(id)
        if (selectedNote?.id === id) {
          closeEditor()
        }
        refetch()
      } catch {
        setActionError('Unable to archive note. Please try again.')
      }
    },
    [selectedNote, closeEditor, refetch]
  )

  const handleDelete = useCallback(async (id: string) => {
    await handleArchive(id)
  }, [handleArchive])

  const filterTabs: { key: FilterTab; label: string; icon: typeof BookOpen }[] = [
    { key: 'all', label: 'All Notes', icon: BookOpen },
    { key: 'favorites', label: 'Favorites', icon: Star },
    { key: 'archived', label: 'Archived', icon: Archive },
  ]

  return (
    <div className="flex h-full min-h-0 flex-col">
      {actionError && (
        <div className="mx-4 mb-2 flex items-start gap-3 rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
          <AlertCircle className="mt-0.5 h-4 w-4 shrink-0" />
          <span className="flex-1">{actionError}</span>
          <button className="text-rose-400 hover:text-rose-600" onClick={() => setActionError(null)}>
            ✕
          </button>
        </div>
      )}

      <div className="relative flex min-h-0 flex-1">
        <div
          className={cn(
            'flex w-full flex-col border-r border-slate-200 bg-[#F8FAFC] transition-all',
            'lg:w-80 xl:w-96',
            mobileScreen === 'editor' ? 'hidden lg:flex' : 'flex'
          )}
        >
          <div className="border-b border-slate-200 bg-[#F8FAFC] px-3 pb-0 pt-3 sm:px-4 sm:pt-4">
            <div className="flex items-center gap-2">
              <form onSubmit={searchNote} className="relative flex-1">
                <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
                <input
                  ref={searchInputRef}
                  type="search"
                  value={queryInput}
                  onChange={(e) => {
                    setQueryInput(e.target.value)
                    if (e.target.value === '') {
                      setQuery('')
                    }
                  }}
                  placeholder="Search notes..."
                  className="h-11 w-full rounded-xl border border-slate-300 bg-white pl-10 pr-4 text-sm text-slate-900 placeholder:text-slate-400 focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-500/20"
                />
              </form>
              <button
                type="button"
                onClick={openNewNote}
                className="hidden h-11 items-center gap-1 rounded-xl bg-indigo-600 px-3 text-sm font-semibold text-white transition hover:bg-indigo-700 lg:flex"
              >
                <Plus className="h-4 w-4" />
                New
              </button>
            </div>

            <div className="mt-3 flex gap-1 border-b border-slate-200 pb-0">
              {filterTabs.map((tab) => (
                <button
                  key={tab.key}
                  type="button"
                  onClick={() => setFilterTab(tab.key)}
                  className={cn(
                    'flex-1 pb-3 pt-1 text-xs font-medium transition-colors',
                    filterTab === tab.key
                      ? 'border-b-2 border-indigo-600 text-indigo-600'
                      : 'text-slate-500 hover:text-slate-700'
                  )}
                >
                  {tab.label}
                </button>
              ))}
            </div>
          </div>

          <div className="px-4 py-2 text-xs text-slate-500">
            {!loading && !error && (
              <span>
                {displayedNotes.length} {displayedNotes.length === 1 ? 'note' : 'notes'}
                {totalCount > PAGE_SIZE && ` (${totalCount} total)`}
              </span>
            )}
          </div>

          <div className="flex-1 space-y-2 overflow-y-auto px-3 pb-24 sm:px-4 lg:pb-4">
            {error ? (
              <div className="mt-4 rounded-2xl border border-rose-200 bg-rose-50 px-4 py-4 text-sm text-rose-700">
                <p className="font-semibold">Unable to load notes.</p>
                <p className="mt-1">{error}</p>
                <button
                  type="button"
                  onClick={() => void refetch()}
                  className="mt-3 rounded-xl bg-rose-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-rose-700"
                >
                  Retry
                </button>
              </div>
            ) : loading ? (
              <>
                <NoteCardSkeleton />
                <NoteCardSkeleton />
                <NoteCardSkeleton />
                <div className="flex items-center justify-center py-4 text-xs text-slate-400">
                  <Loader2 className="mr-2 h-3 w-3 animate-spin" />
                  Loading notes...
                </div>
              </>
            ) : displayedNotes.length === 0 ? (
              <EmptyState query={query} filterTab={filterTab} onCreateNote={openNewNote} />
            ) : (
              displayedNotes.map((note) => (
                <NoteCard
                  key={note.id}
                  note={note}
                  isActive={selectedNote?.id === note.id}
                  onSelect={openNote}
                  onArchive={(n) => void handleArchive(n.id)}
                />
              ))
            )}
          </div>
        </div>

        <div
          className={cn(
            'flex min-h-0 flex-1 flex-col overflow-hidden',
            mobileScreen === 'list' ? 'hidden lg:flex' : 'flex'
          )}
        >
          <NoteForm
            note={isCreating ? null : selectedNote}
            isNew={isCreating}
            onSave={handleSave}
            onArchive={handleArchive}
            onDelete={handleDelete}
            onClose={closeEditor}
          />
        </div>
      </div>

      <button
        type="button"
        onClick={openNewNote}
        className={cn(
          'fixed bottom-24 right-4 z-30 flex h-14 w-14 items-center justify-center rounded-full bg-indigo-600 text-white shadow-lg transition hover:bg-indigo-700 focus:outline-none focus:ring-4 focus:ring-indigo-500/30 sm:right-6 lg:hidden',
          mobileScreen === 'editor' && 'hidden'
        )}
        aria-label="Create new note"
      >
        <Plus className="h-7 w-7" />
      </button>
    </div>
  )
}
