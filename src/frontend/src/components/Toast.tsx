import { AlertCircle, CheckCircle2, Info, X } from 'lucide-react'

export type ToastTone = 'error' | 'success' | 'info'

export type ToastItem = {
  id: string
  message: string
  tone?: ToastTone
}

const styles: Record<ToastTone, string> = {
  error: 'border-red-200 bg-red-50 text-red-900',
  success: 'border-emerald-200 bg-emerald-50 text-emerald-900',
  info: 'border-sky-200 bg-sky-50 text-sky-900',
}

const icons = {
  error: AlertCircle,
  success: CheckCircle2,
  info: Info,
} satisfies Record<ToastTone, typeof AlertCircle>

export function Toast({ toast, onDismiss }: { toast: ToastItem; onDismiss: (id: string) => void }) {
  const tone = toast.tone ?? 'info'
  const Icon = icons[tone]

  return (
    <div className={`flex items-start gap-3 rounded-xl border p-4 shadow-lg ${styles[tone]}`} role="alert">
      <Icon className="mt-0.5 h-5 w-5 shrink-0" />
      <p className="flex-1 text-sm font-medium">{toast.message}</p>
      <button
        type="button"
        onClick={() => onDismiss(toast.id)}
        className="rounded-md p-1 transition hover:bg-black/5"
        aria-label="Dismiss notification"
      >
        <X className="h-4 w-4" />
      </button>
    </div>
  )
}

export function ToastViewport({
  toasts,
  onDismiss,
}: {
  toasts: ToastItem[]
  onDismiss: (id: string) => void
}) {
  return (
    <div className="pointer-events-none fixed right-4 top-4 z-50 flex w-full max-w-sm flex-col gap-3">
      {toasts.map((toast) => (
        <div key={toast.id} className="pointer-events-auto">
          <Toast toast={toast} onDismiss={onDismiss} />
        </div>
      ))}
    </div>
  )
}
