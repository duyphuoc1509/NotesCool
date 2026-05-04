import { Route, Routes, useLocation, useNavigate } from 'react-router-dom'
import { AuthProvider } from './contexts/AuthContext'
import { ProtectedRoute } from './components/ProtectedRoute'
import { Sidebar } from './components/Sidebar'
import { Navbar } from './components/Navbar'
import { BottomNav } from './components/BottomNav'
import { QuickCreateSheet } from './components/QuickCreateSheet'
import { LoginPage } from './pages/LoginPage'
import { RegisterPage } from './pages/RegisterPage'
import { SsoCallbackPage } from './pages/SsoCallbackPage'
import { NotesPage } from './pages/NotesPage'
import { TasksPage } from './pages/TasksPage'
import { AdminUsersPage } from './pages/AdminUsersPage'
import { useState, useEffect, type ReactNode } from 'react'
import { useAuth } from './hooks/useAuth'
import { FileText, CheckSquare, Clock } from 'lucide-react'
import { useNotes } from './hooks/useNotes'
import { useTasks } from './hooks/useTasks'
import { useTranslation } from 'react-i18next'

function Layout({ children }: { children: ReactNode }) {
  const [isSidebarOpen, setIsSidebarOpen] = useState(false)
  const [isQuickCreateOpen, setIsQuickCreateOpen] = useState(false)
  const location = useLocation()

  // Close sidebar and quick create on route change
  useEffect(() => {
    setIsSidebarOpen(false)
    setIsQuickCreateOpen(false)
  }, [location.pathname])

  return (
    <div className="flex h-screen bg-gray-50 overflow-hidden">
      <Sidebar isOpen={isSidebarOpen} onClose={() => setIsSidebarOpen(false)} />
      <div className="flex flex-1 flex-col overflow-hidden min-w-0">
        <Navbar onMenuClick={() => setIsSidebarOpen(true)} />
        <main className="flex-1 overflow-y-auto p-4 md:p-8 pb-24 md:pb-8">{children}</main>
        <BottomNav onQuickCreate={() => setIsQuickCreateOpen(true)} />
        <QuickCreateSheet isOpen={isQuickCreateOpen} onClose={() => setIsQuickCreateOpen(false)} />
      </div>
    </div>
  )
}

function DashboardPage() {
  const { user, isAdmin } = useAuth()
  const navigate = useNavigate()
  const { t } = useTranslation()
  
  // Quick fetch for dashboard preview
  const { notes } = useNotes({ page: 1, pageSize: 3, query: '' })
  const { tasks } = useTasks({ page: 1, pageSize: 5 })
  
  const todayTasks = tasks.filter(task => task.status !== 'Done' && task.status !== 'Archived').slice(0, 3)

  return (
    <div className="mx-auto max-w-4xl space-y-8">
      {/* Welcome & Quick Actions */}
      <section className="rounded-2xl border border-gray-200 bg-white p-6 shadow-sm">
        <h1 className="text-2xl font-bold tracking-tight text-gray-950 sm:text-3xl">
          {t('dashboard.greeting', { name: user?.displayName ?? user?.fullName?.split(' ')[0] ?? t('user.adminFallback') })}
        </h1>
        <p className="mt-1 text-sm text-gray-500">{t('dashboard.welcome')}</p>
        
        <div className="mt-6 grid grid-cols-2 gap-4">
          <button
            onClick={() => navigate('/notes')}
            className="flex items-center gap-3 rounded-xl border border-indigo-100 bg-indigo-50 p-4 transition-colors hover:bg-indigo-100/70"
          >
            <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-white text-indigo-600 shadow-sm">
              <FileText className="h-5 w-5" />
            </div>
            <div className="text-left">
              <div className="font-semibold text-indigo-900 text-sm md:text-base">{t('dashboard.newNote')}</div>
            </div>
          </button>
          
          <button
            onClick={() => navigate('/tasks')}
            className="flex items-center gap-3 rounded-xl border border-rose-100 bg-rose-50 p-4 transition-colors hover:bg-rose-100/70"
          >
            <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-white text-rose-600 shadow-sm">
              <CheckSquare className="h-5 w-5" />
            </div>
            <div className="text-left">
              <div className="font-semibold text-rose-900 text-sm md:text-base">{t('dashboard.newTask')}</div>
            </div>
          </button>
        </div>
      </section>

      {isAdmin ? (
        <section className="rounded-2xl border border-emerald-100 bg-emerald-50 p-5 shadow-sm">
          <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <p className="text-xs font-semibold uppercase tracking-wide text-emerald-700">{t('dashboard.adminLabel')}</p>
              <h2 className="mt-1 text-lg font-bold text-emerald-950">{t('dashboard.userManagement')}</h2>
              <p className="mt-1 text-sm text-emerald-800">{t('dashboard.userManagementDescription')}</p>
            </div>
            <button
              type="button"
              onClick={() => navigate('/users')}
              className="inline-flex items-center justify-center rounded-xl bg-emerald-600 px-4 py-2 text-sm font-semibold text-white shadow-sm transition hover:bg-emerald-500"
            >
              {t('dashboard.manageUsers')}
            </button>
          </div>
        </section>
      ) : null}

      <div className="grid gap-8 md:grid-cols-2">
        {/* Today Tasks */}
        <section className="flex flex-col gap-4">
          <div className="flex items-center justify-between">
            <h2 className="text-lg font-bold text-gray-900">{t('dashboard.todaysTasks')}</h2>
            <button onClick={() => navigate('/tasks')} className="text-sm font-medium text-indigo-600 hover:text-indigo-700">{t('dashboard.seeAll')}</button>
          </div>
          
          <div className="flex-1 rounded-2xl border border-gray-200 bg-white p-2 shadow-sm">
            {todayTasks.length === 0 ? (
              <div className="flex h-full flex-col items-center justify-center py-8 text-center">
                <CheckSquare className="mb-2 h-8 w-8 text-gray-300" />
                <p className="text-sm font-medium text-gray-900">{t('dashboard.allCaughtUp')}</p>
                <p className="text-xs text-gray-500">{t('dashboard.noPendingTasks')}</p>
              </div>
            ) : (
              <ul className="divide-y divide-gray-100">
                {todayTasks.map(task => (
                  <li key={task.id} className="flex items-start gap-3 p-3">
                    <div className="mt-0.5 flex h-5 w-5 shrink-0 items-center justify-center rounded border border-gray-300" />
                    <div className="min-w-0 flex-1">
                      <p className="truncate text-sm font-medium text-gray-900">{task.title}</p>
                      {task.dueDate && (
                        <p className="mt-1 text-xs text-gray-500">{t('dashboard.due', { date: new Date(task.dueDate).toLocaleDateString() })}</p>
                      )}
                    </div>
                  </li>
                ))}
              </ul>
            )}
          </div>
        </section>

        {/* Recent Notes */}
        <section className="flex flex-col gap-4">
          <div className="flex items-center justify-between">
            <h2 className="text-lg font-bold text-gray-900">{t('dashboard.recentNotes')}</h2>
            <button onClick={() => navigate('/notes')} className="text-sm font-medium text-indigo-600 hover:text-indigo-700">{t('dashboard.seeAll')}</button>
          </div>
          
          <div className="flex-1 space-y-3">
            {notes.length === 0 ? (
              <div className="rounded-2xl border border-gray-200 bg-white py-8 text-center shadow-sm">
                <FileText className="mx-auto mb-2 h-8 w-8 text-gray-300" />
                <p className="text-sm font-medium text-gray-900">{t('dashboard.noNotesYet')}</p>
              </div>
            ) : (
              notes.map(note => (
                <div key={note.id} className="flex flex-col gap-2 rounded-2xl border border-gray-200 bg-white p-4 shadow-sm transition hover:border-indigo-200 cursor-pointer" onClick={() => navigate('/notes')}>
                  <h3 className="font-semibold text-gray-900 truncate">{note.title || t('dashboard.untitledNote')}</h3>
                  <div className="flex items-center text-xs text-gray-500">
                    <Clock className="mr-1 h-3 w-3" />
                    <span>{t('dashboard.updatedRecently')}</span>
                  </div>
                </div>
              ))
            )}
          </div>
        </section>
      </div>
    </div>
  )
}


function AppRoutes() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
      <Route path="/auth/sso/callback" element={<SsoCallbackPage />} />
      <Route path="/auth/sso/:provider/callback" element={<SsoCallbackPage />} />
      <Route path="/auth/callback/:provider" element={<SsoCallbackPage />} />
      <Route
        path="/notes"
        element={
          <ProtectedRoute>
            <Layout>
              <NotesPage />
            </Layout>
          </ProtectedRoute>
        }
      />
      <Route
        path="/tasks"
        element={
          <ProtectedRoute>
            <Layout>
              <TasksPage />
            </Layout>
          </ProtectedRoute>
        }
      />
      <Route
        path="/users"
        element={
          <ProtectedRoute requireAdmin>
            <Layout>
              <AdminUsersPage />
            </Layout>
          </ProtectedRoute>
        }
      />
      <Route
        path="/*"
        element={
          <ProtectedRoute>
            <Layout>
              <DashboardPage />
            </Layout>
          </ProtectedRoute>
        }
      />
    </Routes>
  )
}

function App() {
  return (
    <AuthProvider>
      <AppRoutes />
    </AuthProvider>
  )
}

export default App
