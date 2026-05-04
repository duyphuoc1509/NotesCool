import { Archive, ChevronRight, FileText, Star } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import type { Note } from '../../types/note'
import { cn } from '../../utils/cn'

interface NoteCardProps {
  note: Note
  isActive?: boolean
  onSelect: (note: Note) => void
  onFavoriteToggle: (note: Note) => void
  onArchive: (note: Note) => void
}

function formatUpdatedAt(dateValue: string, t: (key: string, options?: Record<string, unknown>) => string) {
  const updatedAt = new Date(dateValue)
  const now = new Date()
  const diffMs = now.getTime() - updatedAt.getTime()
  const diffMinutes = Math.floor(diffMs / 60000)

  if (diffMinutes < 1) return t('notes.updatedJustNow')
  if (diffMinutes < 60) return t('notes.updatedMinAgo', { count: diffMinutes })

  const diffHours = Math.floor(diffMinutes / 60)
  if (diffHours < 24) return t('notes.updatedHourAgo', { count: diffHours })

  const diffDays = Math.floor(diffHours / 24)
  if (diffDays < 7) return t('notes.updatedDayAgo', { count: diffDays })

  return t('dashboard.updated', { time: updatedAt.toLocaleDateString() })
}

export function NoteCard({ note, isActive = false, onSelect, onFavoriteToggle, onArchive }: NoteCardProps) {
  const { t } = useTranslation()
  const tags = note.tags?.filter(Boolean) ?? []
  const preview = note.content.trim() || t('notes.noAdditionalContent')

  return (
    <article
      className={cn(
        'group rounded-2xl border bg-white p-4 shadow-sm transition-all',
        'hover:border-indigo-200 hover:shadow-md',
        'focus-within:border-indigo-300 focus-within:shadow-md',
        isActive ? 'border-indigo-500 ring-2 ring-indigo-500/10' : 'border-gray-200'
      )}
    >
      <div className="flex items-start gap-3">
        <button
          type="button"
          onClick={() => onSelect(note)}
          className="flex min-w-0 flex-1 items-start gap-3 text-left"
          aria-pressed={isActive}
        >
          <div className="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-indigo-50 text-indigo-600">
            <FileText className="h-5 w-5" />
          </div>

          <div className="min-w-0 flex-1">
            <div className="flex items-start justify-between gap-2">
              <h3 className="line-clamp-1 text-base font-semibold text-slate-900">{note.title || t('dashboard.untitledNote')}</h3>
              <button
                type="button"
                onClick={(event) => {
                  event.stopPropagation()
                  onFavoriteToggle(note)
                }}
                className={cn(
                  'mt-0.5 inline-flex h-7 w-7 shrink-0 items-center justify-center rounded-lg transition',
                  note.isFavorite ? 'text-amber-400 hover:bg-amber-50' : 'text-slate-300 hover:bg-slate-100 hover:text-slate-500'
                )}
                aria-label={note.isFavorite ? t('notes.removeFromFavorites') : t('notes.addToFavorites')}
                title={note.isFavorite ? t('notes.removeFromFavorites') : t('notes.addToFavorites')}
              >
                <Star className={cn('h-4 w-4', note.isFavorite && 'fill-amber-400')} />
              </button>
            </div>

            <p className="mt-1 line-clamp-2 text-sm leading-6 text-slate-600">{preview}</p>
          </div>
        </button>

        <button
          type="button"
          onClick={() => onArchive(note)}
          className="inline-flex h-11 w-11 shrink-0 items-center justify-center rounded-xl border border-transparent text-slate-400 transition hover:border-rose-100 hover:bg-rose-50 hover:text-rose-600"
          aria-label={t('notes.archiveNote')}
          title={t('notes.archiveNote')}
        >
          <Archive className="h-4 w-4" />
        </button>
      </div>

      <div className="mt-4 flex flex-wrap items-center gap-2">
        {tags.slice(0, 2).map((tag) => (
          <span key={tag} className="rounded-full bg-slate-100 px-2.5 py-1 text-xs font-medium text-slate-600">
            #{tag}
          </span>
        ))}
        {tags.length > 2 ? (
          <span className="rounded-full bg-slate-100 px-2.5 py-1 text-xs font-medium text-slate-500">+{tags.length - 2}</span>
        ) : null}
      </div>

      <div className="mt-4 flex items-center justify-between gap-3 text-xs text-slate-500">
        <span>{formatUpdatedAt(note.updatedAt ?? note.createdAt, t)}</span>
        <button
          type="button"
          onClick={() => onSelect(note)}
          className="inline-flex min-h-11 items-center gap-1 rounded-xl px-2 text-sm font-medium text-indigo-600 transition hover:text-indigo-700"
        >
          {t('notes.open')}
          <ChevronRight className="h-4 w-4" />
        </button>
      </div>
    </article>
  )
}
