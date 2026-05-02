import { useEffect, useState, useCallback, useRef } from 'react'
import { Archive, ChevronLeft, MoreVertical, Star, Trash2, Tag, Loader2, Check } from 'lucide-react'
import type { Note } from '../types'
import { cn } from '../../../utils/cn'

interface NoteFormProps {
  note: Note | null
  onSave: (id: string | undefined, values: { title: string; content: string; tags?: string[]; isFavorite?: boolean }) => Promise<void>
  onArchive: (id: string) => Promise<void>
  onDelete: (id: string) => Promise<void>
  onClose: () => void
  isNew?: boolean
}

export function NoteForm({ note, onSave, onArchive, onDelete, onClose, isNew = false }: NoteFormProps) {
  const [title, setTitle] = useState(note?.title ?? '')
  const [content, setContent] = useState(note?.content ?? '')
  const [isFavorite, setIsFavorite] = useState(note?.isFavorite ?? false)
  const [tags, setTags] = useState<string[]>(note?.tags ?? [])
  const [tagInput, setTagInput] = useState('')

  const [saveStatus, setSaveStatus] = useState<'idle' | 'saving' | 'saved' | 'error'>('idle')
  const [isMenuOpen, setIsMenuOpen] = useState(false)

  const saveTimeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null)
  const isFirstRender = useRef(true)

  const handleManualSave = useCallback(async () => {
    if (!title.trim() && !content.trim()) return

    setSaveStatus('saving')
    try {
      await onSave(note?.id, {
        title: title.trim(),
        content: content.trim(),
        isFavorite,
        tags,
      })
      setSaveStatus('saved')
      setTimeout(() => setSaveStatus('idle'), 2000)
    } catch {
      setSaveStatus('error')
    }
  }, [note, title, content, isFavorite, tags, onSave])

  useEffect(() => {
    if (isFirstRender.current) {
      isFirstRender.current = false
      return
    }

    if (saveTimeoutRef.current) clearTimeout(saveTimeoutRef.current)

    const isSame =
      title === (note?.title ?? '') &&
      content === (note?.content ?? '') &&
      isFavorite === (note?.isFavorite ?? false) &&
      JSON.stringify(tags) === JSON.stringify(note?.tags ?? [])

    if (isSame) return

    saveTimeoutRef.current = setTimeout(() => {
      void handleManualSave()
    }, 2000)

    return () => {
      if (saveTimeoutRef.current) clearTimeout(saveTimeoutRef.current)
    }
  }, [title, content, isFavorite, tags, handleManualSave, note])

  const handleAddTag = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && tagInput.trim()) {
      e.preventDefault()
      if (!tags.includes(tagInput.trim())) {
        setTags([...tags, tagInput.trim()])
      }
      setTagInput('')
    }
  }

  const removeTag = (tagToRemove: string) => {
    setTags(tags.filter((t) => t !== tagToRemove))
  }

  if (!note && !isNew) {
    return (
      <div className="flex h-full flex-col items-center justify-center space-y-4 px-6 text-center">
        <div className="flex h-16 w-16 items-center justify-center rounded-full bg-slate-100 text-slate-400">
          <Star className="h-8 w-8" />
        </div>
        <div>
          <h3 className="text-lg font-semibold text-slate-900">Select a note to view</h3>
          <p className="mt-1 text-sm text-slate-500">Choose a note from the list on the left to view its content, or create a new one.</p>
        </div>
      </div>
    )
  }

  return (
    <div className="flex h-full flex-col bg-white overflow-hidden">
      <header className="flex h-16 shrink-0 items-center justify-between border-b border-slate-200 px-4">
        <div className="flex items-center gap-2">
          <button
            onClick={onClose}
            className="flex h-10 w-10 items-center justify-center rounded-xl text-slate-500 transition hover:bg-slate-100 lg:hidden"
            aria-label="Back to list"
          >
            <ChevronLeft className="h-6 w-6" />
          </button>

          <div className="flex items-center gap-2 text-xs font-medium text-slate-400">
            {saveStatus === 'saving' && (
              <span className="flex items-center gap-1.5">
                <Loader2 className="h-3 w-3 animate-spin" />
                Saving...
              </span>
            )}
            {saveStatus === 'saved' && (
              <span className="flex items-center gap-1.5 text-emerald-600">
                <Check className="h-3 w-3" />
                Saved
              </span>
            )}
            {saveStatus === 'error' && <span className="text-rose-500">Error saving</span>}
          </div>
        </div>

        <div className="flex items-center gap-2">
          <button
            onClick={() => setIsFavorite(!isFavorite)}
            className={cn(
              'flex h-10 w-10 items-center justify-center rounded-xl transition',
              isFavorite ? 'text-amber-400 hover:bg-amber-50' : 'text-slate-400 hover:bg-slate-100'
            )}
            title={isFavorite ? 'Remove from favorites' : 'Add to favorites'}
          >
            <Star className={cn('h-5 w-5', isFavorite && 'fill-amber-400')} />
          </button>

          <div className="relative">
            <button
              onClick={() => setIsMenuOpen(!isMenuOpen)}
              className="flex h-10 w-10 items-center justify-center rounded-xl text-slate-500 transition hover:bg-slate-100"
            >
              <MoreVertical className="h-5 w-5" />
            </button>

            {isMenuOpen && (
              <>
                <div className="fixed inset-0 z-10" onClick={() => setIsMenuOpen(false)} />
                <div className="absolute right-0 top-full z-20 mt-2 w-48 rounded-2xl border border-slate-200 bg-white p-1 shadow-xl">
                  {note && (
                    <>
                      <button
                        onClick={() => {
                          void onArchive(note.id)
                          setIsMenuOpen(false)
                        }}
                        className="flex w-full items-center gap-3 rounded-xl px-3 py-2 text-left text-sm text-slate-700 transition hover:bg-slate-50"
                      >
                        <Archive className="h-4 w-4" />
                        Archive Note
                      </button>
                      <button
                        onClick={() => {
                          if (confirm('Are you sure you want to delete this note permanently?')) {
                            void onDelete(note.id)
                          }
                          setIsMenuOpen(false)
                        }}
                        className="flex w-full items-center gap-3 rounded-xl px-3 py-2 text-left text-sm text-rose-600 transition hover:bg-rose-50"
                      >
                        <Trash2 className="h-4 w-4" />
                        Delete Permanently
                      </button>
                    </>
                  )}
                </div>
              </>
            )}
          </div>
        </div>
      </header>

      <div className="flex-1 overflow-y-auto p-6 md:p-10">
        <div className="mx-auto max-w-4xl space-y-6">
          <input
            type="text"
            placeholder="Note title"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            className="w-full border-none bg-transparent p-0 text-3xl font-bold text-slate-900 placeholder:text-slate-300 focus:outline-none focus:ring-0"
          />

          <div className="flex flex-wrap items-center gap-2">
            <Tag className="h-4 w-4 text-slate-400" />
            {tags.map((tag) => (
              <span
                key={tag}
                className="inline-flex items-center gap-1 rounded-full bg-slate-100 px-2.5 py-1 text-xs font-medium text-slate-600"
              >
                #{tag}
                <button onClick={() => removeTag(tag)} className="rounded-full p-0.5 hover:bg-slate-200">
                  <ChevronLeft className="h-3 w-3 rotate-45" />
                </button>
              </span>
            ))}
            <input
              type="text"
              placeholder="Add tag..."
              value={tagInput}
              onChange={(e) => setTagInput(e.target.value)}
              onKeyDown={handleAddTag}
              className="border-none bg-transparent p-0 text-xs text-slate-500 placeholder:text-slate-400 focus:outline-none focus:ring-0"
            />
          </div>

          <textarea
            placeholder="Start writing..."
            value={content}
            onChange={(e) => setContent(e.target.value)}
            className="min-h-[400px] w-full resize-none border-none bg-transparent p-0 text-lg leading-relaxed text-slate-700 placeholder:text-slate-300 focus:outline-none focus:ring-0"
          />
        </div>
      </div>
    </div>
  )
}
