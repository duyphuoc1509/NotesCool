import { X, FileText, CheckSquare } from 'lucide-react'
import { cn } from '../utils/cn'
import { useNavigate } from 'react-router-dom'

interface QuickCreateSheetProps {
  isOpen: boolean
  onClose: () => void
}

export function QuickCreateSheet({ isOpen, onClose }: QuickCreateSheetProps) {
  const navigate = useNavigate()

  if (!isOpen) return null

  const handleAction = (path: string) => {
    navigate(path)
    onClose()
  }

  return (
    <div className="fixed inset-0 z-50 lg:hidden">
      <div 
        className="absolute inset-0 bg-slate-900/40 backdrop-blur-sm transition-opacity" 
        onClick={onClose} 
      />
      
      <div className={cn(
        "absolute inset-x-0 bottom-0 transform transition-transform duration-300 ease-out",
        "rounded-t-3xl bg-white p-6 shadow-2xl ring-1 ring-slate-900/5",
        isOpen ? "translate-y-0" : "translate-y-full"
      )}>
        <div className="mx-auto mb-6 h-1.5 w-12 rounded-full bg-slate-200" onClick={onClose} />
        
        <div className="flex items-center justify-between mb-6">
          <h2 className="text-xl font-bold text-slate-900">Create New</h2>
          <button 
            onClick={onClose}
            className="rounded-full p-2 text-slate-400 hover:bg-slate-50 hover:text-slate-600"
          >
            <X className="h-5 w-5" />
          </button>
        </div>

        <div className="grid grid-cols-2 gap-4 pb-8">
          <button
            onClick={() => handleAction('/notes')}
            className="flex flex-col items-center justify-center gap-3 rounded-2xl border-2 border-slate-100 bg-white p-6 text-indigo-600 transition hover:border-indigo-100 hover:bg-indigo-50/50 active:scale-95"
          >
            <div className="flex h-12 w-12 items-center justify-center rounded-2xl bg-indigo-50 text-indigo-600">
              <FileText className="h-6 w-6" />
            </div>
            <span className="font-semibold text-slate-900">New Note</span>
          </button>

          <button
            onClick={() => handleAction('/tasks')}
            className="flex flex-col items-center justify-center gap-3 rounded-2xl border-2 border-slate-100 bg-white p-6 text-rose-600 transition hover:border-rose-100 hover:bg-rose-50/50 active:scale-95"
          >
            <div className="flex h-12 w-12 items-center justify-center rounded-2xl bg-rose-50 text-rose-600">
              <CheckSquare className="h-6 w-6" />
            </div>
            <span className="font-semibold text-slate-900">New Task</span>
          </button>
        </div>
      </div>
    </div>
  )
}
