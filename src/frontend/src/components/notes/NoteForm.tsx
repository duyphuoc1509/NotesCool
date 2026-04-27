import { useState } from 'react'
import type { FormEvent } from 'react'

interface NoteFormProps {
  initialValues?: { title: string; content: string }
  onSubmit: (values: { title: string; content: string }) => Promise<void>
  onCancel: () => void
  isSubmitting?: boolean
}

export function NoteForm({ initialValues, onSubmit, onCancel, isSubmitting }: NoteFormProps) {
  const [title, setTitle] = useState(initialValues?.title ?? '')
  const [content, setContent] = useState(initialValues?.content ?? '')

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    await onSubmit({ title: title.trim(), content: content.trim() })
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div>
        <label htmlFor="title" className="block text-sm font-medium text-gray-700">
          Title
        </label>
        <input
          type="text"
          id="title"
          value={title}
          onChange={(event) => setTitle(event.target.value)}
          required
          maxLength={200}
          className="mt-1 block w-full rounded-xl border border-gray-300 px-4 py-3 shadow-sm focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500/20 focus:outline-none sm:text-sm"
          placeholder="Note title"
        />
      </div>
      <div>
        <label htmlFor="content" className="block text-sm font-medium text-gray-700">
          Content
        </label>
        <textarea
          id="content"
          rows={10}
          value={content}
          onChange={(event) => setContent(event.target.value)}
          required
          className="mt-1 block w-full rounded-xl border border-gray-300 px-4 py-3 shadow-sm focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500/20 focus:outline-none sm:text-sm"
          placeholder="Note content..."
        />
      </div>
      <div className="flex justify-end gap-3">
        <button
          type="button"
          onClick={onCancel}
          className="rounded-xl border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm transition hover:bg-gray-50"
        >
          Cancel
        </button>
        <button
          type="submit"
          disabled={isSubmitting || !title.trim() || !content.trim()}
          className="inline-flex justify-center rounded-xl border border-transparent bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-sm transition hover:bg-indigo-700 disabled:cursor-not-allowed disabled:opacity-50"
        >
          {isSubmitting ? 'Saving...' : 'Save'}
        </button>
      </div>
    </form>
  )
}
