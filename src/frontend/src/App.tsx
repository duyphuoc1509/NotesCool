import { useCallback, useEffect, useState } from 'react'
import { Sidebar } from './components/Sidebar'
import { Navbar } from './components/Navbar'
import { ToastViewport, type ToastItem } from './components/Toast'
import { useApiError } from './hooks/useApiError'
import type { StandardError } from './services/apiError'

function App() {
  const [toasts, setToasts] = useState<ToastItem[]>([])

  const dismissToast = useCallback((id: string) => {
    setToasts((current) => current.filter((toast) => toast.id !== id))
  }, [])

  const showToast = useCallback((message: string, tone: ToastItem['tone'] = 'error') => {
    const id = `${Date.now()}-${Math.random().toString(16).slice(2)}`
    setToasts((current) => [...current, { id, message, tone }])
    window.setTimeout(() => {
      setToasts((current) => current.filter((toast) => toast.id !== id))
    }, 5000)
  }, [])

  useApiError((error: StandardError) => {
    showToast(error.message, 'error')
  })

  useEffect(() => {
    // Demo info toast to verify the shared UI state is wired.
    const timer = window.setTimeout(() => {
      showToast('Centralized API error handling is ready.', 'info')
    }, 0)
    return () => window.clearTimeout(timer)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  return (
    <>
      <ToastViewport toasts={toasts} onDismiss={dismissToast} />
      <div className="flex h-screen bg-gray-50">
        <Sidebar />
        <div className="flex flex-1 flex-col overflow-hidden">
          <Navbar />
          <main className="flex-1 overflow-y-auto p-8">
            <div className="rounded-2xl border border-gray-200 bg-white p-8 shadow-sm">
              <p className="text-sm font-semibold uppercase tracking-wide text-indigo-600">NotesCool CMS</p>
              <h1 className="mt-3 text-3xl font-bold tracking-tight text-gray-950 sm:text-4xl">
                Centralized API error handling is ready
              </h1>
              <p className="mt-4 max-w-2xl text-base text-gray-600">
                HTTP errors, network failures, refresh-token logout behavior, and shared toast messaging
                are now managed from a single frontend API layer.
              </p>

              <div className="mt-8 grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
                {[
                  '400 validation → standard user feedback',
                  '401 unauthorized → token refresh or logout',
                  '403 forbidden → permission warning',
                  '404 missing resource → friendly empty/error copy',
                  '409 conflict → suggest refresh and retry',
                  '500/server & network → bounded retry for safe requests',
                ].map((item) => (
                  <div
                    key={item}
                    className="rounded-xl border border-gray-200 bg-gray-50 px-4 py-3 text-sm text-gray-700"
                  >
                    {item}
                  </div>
                ))}
              </div>
            </div>
          </main>
        </div>
      </div>
    </>
  )
}

export default App
