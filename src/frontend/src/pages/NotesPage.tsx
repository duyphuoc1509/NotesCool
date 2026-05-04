import { useCallback, useMemo, useRef, useState } from 'react'
import { AlertCircle, BookOpen, FileText, Loader2, Plus, Search, Star, Archive } from 'lucide-react'
import { useNotes } from '../hooks/useNotes'
import { notesService } from '../services/notesService'
import type { Note } from '../types/note'
import { NoteCard } from '../components/notes/NoteCard'
import { NoteForm } from '../components/notes/NoteForm'
import { cn } from '../utils/cn'
import { useTranslation } from 'react-i18next'

const PAGE_SIZE = 20

type FilterTab = 'all' | 'favorites' | 'archived'

// ------------------------------------
// Skeleton loader
// ------------------------------------
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

// ------------------------------------
// Empty State
// ------------------------------------
function EmptyState({
  query,
  filterTab,
  onCreateNote,
}: {
  query: string
  filterTab: FilterTab
  onCreateNote: () => void
}) {
  const { t } = useTranslation()

  if (query) {
    return (
      <div className="flex flex-col items-center justify-center px-6 py-16 text-center">
        <Search className="mb-4 h-12 w-12 text-slate-300" />
        <h3 className="text-base font-semibold text-slate-800">{t('notes.noResults')}</h3>
        <p className="mt-1 text-sm text-slate-500">{t('notes.noResultsDesc')}</p>
      </div>
    )
  }

  if (filterTab === 'favorites') {
    return (
      <div className="flex flex-col items-center justify-center px-6 py-16 text-center">
        <Star className="mb-4 h-12 w-12 text-slate-300" />
        <h3 className="text-base font-semibold text-slate-800">{t('notes.noFavorites')}</h3>
        <p className="mt-1 text-sm text-slate-500">{t('notes.noFavoritesDesc')}</p>
      </div>
    )
  }

  if (filterTab === 'archived') {
    return (
      <div className="flex flex-col items-center justify-center px-6 py-16 text-center">
        <Archive className="mb-4 h-12 w-12 text-slate-300" />
        <h3 className="text-base font-semibold text-slate-800">{t('notes.noArchived')}</h3>
        <p className="mt-1 text-sm text-slate-500">{t('notes.noArchivedDesc')}</p>
      </div>
    )
  }

  return (
    <div className="flex flex-col items-center justify-center px-6 py-16 text-center">
      <div className="mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-indigo-50">
        <FileText className="h-8 w-8 text-indigo-400" />
      </div>
      <h3 className="text-lg font-semibold text-slate-900">{t('notes.emptyStateTitle')}</h3>
      <p className="mt-2 max-w-xs text-sm text-slate-500">
        {t('notes.emptyStateDesc')}
      </p>
      <button
        type="button"
        onClick={onCreateNote}
        className="mt-6 inline-flex items-center gap-2 rounded-xl bg-indigo-600 px-5 py-2.5 text-sm font-semibold text-white transition hover:bg-indigo-700"
      >
        <Plus className="h-4 w-4" />
        {t('notes.createNote')}
      </button>
    </div>
  )
}

// ------------------------------------
// Main Page
// ------------------------------------
export function NotesPage() {
  const [queryInput, setQueryInput] = useState('')
  const [query, setQuery] = useState('')
  const [filterTab, setFilterTab] = useState<FilterTab>('all')
  const [selectedNote, setSelectedNote] = useState<Note | null>(null)
  const [isCreating, setIsCreating] = useState(false)
  // Mobile: show list or editor
  const [mobileScreen, setMobileScreen] = useState<'list' | 'editor'>('list')
  const [actionError, setActionError] = useState<string | null>(null)
  const { t } = useTranslation()

  const { notes, totalCount, loading, error, refetch } = useNotes({ page: 1, pageSize: PAGE_SIZE, query })
  const searchInputRef = useRef<HTMLInputElement>(null)

  // Filter notes client-side for favorites/archived since backend may not have those fields
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
        setActionError(t('notes.saveError'))
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
        setActionError(t('notes.archiveError'))
      }
    },
    [selectedNote, closeEditor, refetch]
  )

  const handleDelete = useCallback(
    async (id: string) => {
      await handleArchive(id)
    },
    [handleArchive]
  )

  const handleFavoriteToggle = useCallback(
    async (note: Note) => {
      setActionError(null)
      try {
        const updated = await notesService.setFavorite(note.id, !note.isFavorite)
        if (selectedNote?.id === note.id) {
          setSelectedNote(updated)
        }
        refetch()
      } catch {
        setActionError(t('notes.favoriteError'))
      }
    },
    [refetch, selectedNote]
  )

  const filterTabs: { key: FilterTab; label: string; icon: typeof BookOpen }[] = [
    { key: 'all', label: t('notes.allNotes'), icon: BookOpen },
    { key: 'favorites', label: t('notes.favorites'), icon: Star },
    { key: 'archived', label: t('notes.archived'), icon: Archive },
  ]

  return (
    <div className="flex h-full min-h-0 flex-col">
      {/* Action Error Banner */}
      {actionError && (
        <div className="mx-4 mb-2 flex items-start gap-3 rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
          <AlertCircle className="mt-0.5 h-4 w-4 shrink-0" />
          <span className="flex-1">{actionError}</span>
          <button className="text-rose-400 hover:text-rose-600" onClick={() => setActionError(null)}>
            ✕
          </button>
        </div>
      )}

      {/* Main Layout */}
      <div className="relative flex min-h-0 flex-1">
        {/* ---- LEFT PANEL: Note List ---- */}
        <div
          className={cn(
            'flex w-full flex-col border-r border-slate-200 bg-[#F8FAFC] transition-all',
            // On desktop and tablet: always visible, take a portion of width
            'lg:w-80 xl:w-96',
            // On mobile: full width only when list is showing
            mobileScreen === 'editor' ? 'hidden lg:flex' : 'flex'
          )}
        >
          {/* Search header */}
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
                  placeholder={t('notes.searchPlaceholder')}
                  className="h-11 w-full rounded-xl border border-slate-300 bg-white pl-10 pr-4 text-sm text-slate-900 placeholder:text-slate-400 focus:border-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-500/20"
                />
              </form>
              {/* Desktop New Note button */}
              <button
                type="button"
                onClick={openNewNote}
                className="hidden h-11 items-center gap-1 rounded-xl bg-indigo-600 px-3 text-sm font-semibold text-white transition hover:bg-indigo-700 lg:flex"
              >
                <Plus className="h-4 w-4" />
                {t('notes.new')}
              </button>
            </div>

            {/* Filter Tabs */}
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

          {/* Note Count */}
          <div className="px-4 py-2 text-xs text-slate-500">
            {!loading && !error && (
              <span>
                {t('notes.noteCount', { count: displayedNotes.length })}
                {totalCount > PAGE_SIZE && ` ${t('notes.totalCount', { total: totalCount })}`}
              </span>
            )}
          </div>

          {/* Note list / Loading / Error / Empty */}
          <div className="flex-1 space-y-2 overflow-y-auto px-3 pb-24 sm:px-4 lg:pb-4">
            {error ? (
              <div className="mt-4 rounded-2xl border border-rose-200 bg-rose-50 px-4 py-4 text-sm text-rose-700">
                <p className="font-semibold">{t('notes.unableToLoad')}</p>
                <p className="mt-1">{error}</p>
                <button
                  type="button"
                  onClick={() => void refetch()}
                  className="mt-3 rounded-xl bg-rose-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-rose-700"
                >
                  {t('notes.retry')}
                </button>
              </div>
            ) : loading ? (
              <>
                <NoteCardSkeleton />
                <NoteCardSkeleton />
                <NoteCardSkeleton />
                <div className="flex items-center justify-center py-4 text-xs text-slate-400">
                  <Loader2 className="mr-2 h-3 w-3 animate-spin" />
                  {t('notes.loading')}
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
                  onFavoriteToggle={(n) => void handleFavoriteToggle(n)}
                  onArchive={(n) => void handleArchive(n.id)}
                />
              ))
            )}
          </div>
        </div>

        {/* ---- RIGHT PANEL: Note Editor ---- */}
        <div
          className={cn(
            'flex min-h-0 flex-1 flex-col overflow-hidden',
            // On mobile: take up full screen only when editing
            mobileScreen === 'list' ? 'hidden lg:flex' : 'flex'
          )}
        >
          <NoteForm
            key={isCreating ? 'new-note' : selectedNote?.id ?? 'empty-note'}
            note={isCreating ? null : selectedNote}
            isNew={isCreating}
            onSave={handleSave}
            onArchive={handleArchive}
            onDelete={handleDelete}
            onClose={closeEditor}
          />
        </div>
      </div>

      {/* Mobile FAB */}
      <button
        type="button"
        onClick={openNewNote}
        className={cn(
          'fixed bottom-24 right-4 z-30 flex h-14 w-14 items-center justify-center rounded-full bg-indigo-600 text-white shadow-lg transition hover:bg-indigo-700 focus:outline-none focus:ring-4 focus:ring-indigo-500/30 sm:right-6 lg:hidden',
          mobileScreen === 'editor' && 'hidden'
        )}
        aria-label={t('notes.createNote')}
      >
        <Plus className="h-7 w-7" />
      </button>
    </div>
  )
}
