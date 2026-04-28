import type { Note } from '../../types/note'
import { FileText, Archive, Edit2 } from 'lucide-react'

interface NoteCardProps {
  note: Note
  onEdit: (note: Note) => void
  onArchive: (id: string) => void
}

export function NoteCard({ note, onEdit, onArchive }: NoteCardProps) {
  return (
    <div className="group flex flex-col rounded-xl border border-gray-200 bg-white p-5 shadow-sm transition hover:shadow-md">
      <div className="flex items-start justify-between">
        <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-indigo-50 text-indigo-600">
          <FileText className="h-6 w-6" />
        </div>
        <div className="flex space-x-1 opacity-0 transition group-hover:opacity-100">
          <button
            onClick={() => onEdit(note)}
            className="rounded-md p-2 text-gray-400 hover:bg-gray-100 hover:text-gray-600"
            title="Edit note"
          >
            <Edit2 className="h-4 w-4" />
          </button>
          <button
            onClick={() => onArchive(note.id)}
            className="rounded-md p-2 text-gray-400 hover:bg-rose-50 hover:text-rose-600"
            title="Archive note"
          >
            <Archive className="h-4 w-4" />
          </button>
        </div>
      </div>
      <h3 className="mt-4 text-lg font-semibold text-gray-900 line-clamp-1">{note.title}</h3>
      <p className="mt-2 flex-1 text-sm text-gray-600 line-clamp-3">{note.content}</p>
      <div className="mt-4 flex items-center text-xs text-gray-400">
        <span>Updated {new Date(note.updatedAt ?? note.createdAt).toLocaleDateString()}</span>
      </div>
    </div>
  )
}
