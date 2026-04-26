import { Navigate, Route, Routes } from 'react-router-dom'
import { Sidebar } from './components/Sidebar'
import { Navbar } from './components/Navbar'
import { LoginPage } from './pages/LoginPage'
import { ProtectedRoute } from './components/ProtectedRoute'

function DashboardPage() {
  return (
    <div className="flex h-screen bg-gray-50">
      <Sidebar />
      <div className="flex flex-1 flex-col overflow-hidden">
        <Navbar />
        <main className="flex-1 overflow-y-auto p-8">
          <div className="rounded-2xl border border-gray-200 bg-white p-8 shadow-sm">
            <p className="text-sm font-semibold uppercase tracking-wide text-indigo-600">NotesCool CMS</p>
            <h1 className="mt-3 text-3xl font-bold tracking-tight text-gray-950 sm:text-4xl">Session persistence is enabled</h1>
            <p className="mt-4 max-w-2xl text-base text-gray-600">
              Auth state is restored from storage, unauthorized requests attempt token refresh, and logout clears the session before redirecting back to login.
            </p>

            <div className="mt-8 grid gap-4 sm:grid-cols-3">
              {[
                'Restores stored access token on reload',
                'Refreshes expired sessions when a refresh token exists',
                'Clears session and redirects on logout',
              ].map((item) => (
                <div key={item} className="rounded-xl border border-gray-200 bg-gray-50 px-4 py-4 text-sm text-gray-700">
                  {item}
                </div>
              ))}
            </div>
          </div>
        </main>
      </div>
    </div>
  )
}

function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route
        path="/"
        element={
          <ProtectedRoute>
            <DashboardPage />
          </ProtectedRoute>
        }
      />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}

export default App
