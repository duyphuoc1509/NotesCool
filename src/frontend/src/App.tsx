import { Navbar } from './components/Navbar'
import { Sidebar } from './components/Sidebar'
import { AccountSsoSettings } from './pages/AccountSsoSettings'

function App() {
  const [session, setSession] = useState<AuthResponse | null>(() => getAuthSession())
  const [view, setView] = useState<AuthView>(() => (getAuthSession() ? 'account' : 'login'))

  const isAuthenticated = Boolean(session)

  useEffect(() => {
    if (!session?.refreshToken) return

    refreshSession()
      .then((nextSession) => {
        if (nextSession) {
          saveAuthSession(nextSession)
          setSession(nextSession)
        }
      })
      .catch(() => {
        clearAuthSession()
        setSession(null)
        setView('login')
      })
  }, [session?.refreshToken])

  function handleAuthenticated(nextSession: AuthResponse) {
    saveAuthSession(nextSession)
    setSession(nextSession)
    setView('account')
  }

  async function handleLogout() {
    try {
      await logout()
    } finally {
      clearAuthSession()
      setSession(null)
      setView('login')
    }
  }

  return (
    <div className="flex h-screen bg-gray-50">
      <Sidebar />
      <div className="flex flex-1 flex-col overflow-hidden">
        <Navbar />
        <main className="flex-1 overflow-y-auto p-6 sm:p-8">
          <AccountSsoSettings />
        </main>
      </div>
      <div className="space-y-3">
        {providers.map((provider) => (
          <article key={provider.id} className="flex flex-col gap-3 rounded-2xl border border-slate-200 p-4 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <h3 className="font-semibold text-slate-950">{provider.name}</h3>
              <p className="text-sm text-slate-600">{provider.linked ? provider.email ?? 'Linked provider' : 'Not linked'}</p>
              {!provider.canUnlink && provider.linked && provider.disabledReason && <p className="mt-1 text-xs text-amber-700">{provider.disabledReason}</p>}
            </div>
            {provider.linked ? (
              <button type="button" onClick={() => handleUnlink(provider)} disabled={!provider.canUnlink} className="rounded-xl bg-red-50 px-4 py-2 text-sm font-semibold text-red-700 disabled:cursor-not-allowed disabled:opacity-50">
                Unlink
              </button>
            ) : (
              <button type="button" onClick={() => startSsoLink(provider.id)} className="rounded-xl bg-slate-950 px-4 py-2 text-sm font-semibold text-white">
                Link
              </button>
            )}
          </article>
        ))}
      </div>
      {status.message && <p role="status" className="rounded-xl bg-slate-100 px-4 py-3 text-sm text-slate-700">{status.message}</p>}
      <button type="button" onClick={onLogout} className="w-full rounded-xl border border-slate-300 px-4 py-3 font-semibold text-slate-700 hover:bg-slate-50">
        Logout and clear session
      </button>
    </div>
  )
}

function EmptyAccountState({ onLogin }: { onLogin: () => void }) {
  return (
    <div className="rounded-2xl bg-slate-50 p-6 text-center">
      <h2 className="text-xl font-bold">Login required</h2>
      <p className="mt-2 text-sm text-slate-600">Sign in before managing SSO providers.</p>
      <button type="button" onClick={onLogin} className="mt-4 rounded-xl bg-indigo-600 px-4 py-2 font-semibold text-white">Go to login</button>
    </div>
  )
}

export default App
