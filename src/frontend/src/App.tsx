import { useEffect, useMemo, useState } from 'react'
import {
  clearAuthSession,
  getAuthSession,
  getSsoProviders,
  login,
  logout,
  refreshSession,
  register,
  saveAuthSession,
  startSsoLink,
  startSsoLogin,
  unlinkSsoProvider,
  type AuthResponse,
  type SsoProvider,
} from './services/auth'

type AuthView = 'login' | 'register' | 'account'

type FormStatus = {
  type: 'idle' | 'loading' | 'success' | 'error'
  message: string
}

const initialStatus: FormStatus = { type: 'idle', message: '' }

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
    <div className="min-h-screen bg-slate-950 text-slate-900">
      <main className="mx-auto flex min-h-screen w-full max-w-6xl flex-col gap-8 px-4 py-8 sm:px-6 lg:grid lg:grid-cols-[0.9fr_1.1fr] lg:items-center lg:px-8">
        <section className="rounded-[2rem] bg-gradient-to-br from-indigo-500 via-sky-500 to-emerald-400 p-8 text-white shadow-2xl shadow-sky-950/40">
          <p className="text-sm font-semibold uppercase tracking-[0.3em] text-white/80">NotesCool Identity</p>
          <h1 className="mt-6 text-4xl font-black tracking-tight sm:text-5xl">
            Secure auth flows for notes and tasks.
          </h1>
          <p className="mt-5 max-w-xl text-base leading-7 text-white/85">
            Sign in with email/password or SSO, keep sessions refreshed, and manage linked providers from one responsive account screen.
          </p>

          <div className="mt-8 grid gap-3 text-sm sm:grid-cols-3">
            {['Password login', 'SSO start', 'Provider linking'].map((item) => (
              <div key={item} className="rounded-2xl bg-white/15 p-4 backdrop-blur">
                <span className="font-semibold">{item}</span>
              </div>
            ))}
          </div>
        </section>

        <section className="rounded-[2rem] bg-white p-6 shadow-2xl shadow-slate-950/30 sm:p-8" aria-label="Authentication panel">
          <nav className="mb-6 flex flex-wrap gap-2" aria-label="Authentication views">
            <TabButton active={view === 'login'} onClick={() => setView('login')}>Login</TabButton>
            <TabButton active={view === 'register'} onClick={() => setView('register')}>Register</TabButton>
            <TabButton active={view === 'account'} onClick={() => setView('account')} disabled={!isAuthenticated}>
              Account SSO
            </TabButton>
          </nav>

          {view === 'login' && <LoginForm onSuccess={handleAuthenticated} />}
          {view === 'register' && <RegisterForm onSuccess={handleAuthenticated} />}
          {view === 'account' && session && <AccountPanel session={session} onLogout={handleLogout} />}
          {view === 'account' && !session && <EmptyAccountState onLogin={() => setView('login')} />}
        </section>
      </main>
    </div>
  )
}

function TabButton({ active, disabled, onClick, children }: React.PropsWithChildren<{ active: boolean; disabled?: boolean; onClick: () => void }>) {
  return (
    <button
      type="button"
      onClick={onClick}
      disabled={disabled}
      className={`rounded-full px-4 py-2 text-sm font-semibold transition ${
        active ? 'bg-slate-950 text-white' : 'bg-slate-100 text-slate-700 hover:bg-slate-200 disabled:cursor-not-allowed disabled:opacity-50'
      }`}
    >
      {children}
    </button>
  )
}

function LoginForm({ onSuccess }: { onSuccess: (session: AuthResponse) => void }) {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [status, setStatus] = useState<FormStatus>(initialStatus)
  const canSubmit = useMemo(() => email.includes('@') && password.length >= 8, [email, password])

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!canSubmit) {
      setStatus({ type: 'error', message: 'Enter a valid email and at least 8 password characters.' })
      return
    }

    setStatus({ type: 'loading', message: 'Signing you in...' })
    try {
      onSuccess(await login({ email, password }))
      setStatus({ type: 'success', message: 'Login successful.' })
    } catch {
      setStatus({ type: 'error', message: 'Login failed. Check your credentials and try again.' })
    }
  }

  return (
    <AuthForm title="Welcome back" description="Use your NotesCool password or continue with an external provider." status={status} onSubmit={handleSubmit}>
      <Input label="Email" type="email" value={email} onChange={setEmail} autoComplete="email" />
      <Input label="Password" type="password" value={password} onChange={setPassword} autoComplete="current-password" />
      <button className="w-full rounded-xl bg-indigo-600 px-4 py-3 font-semibold text-white hover:bg-indigo-700 disabled:opacity-50" disabled={status.type === 'loading'}>
        Sign in
      </button>
      <SsoButtons />
    </AuthForm>
  )
}

function RegisterForm({ onSuccess }: { onSuccess: (session: AuthResponse) => void }) {
  const [name, setName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [status, setStatus] = useState<FormStatus>(initialStatus)
  const canSubmit = name.trim().length >= 2 && email.includes('@') && password.length >= 8

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!canSubmit) {
      setStatus({ type: 'error', message: 'Provide a name, valid email, and a password with 8+ characters.' })
      return
    }

    setStatus({ type: 'loading', message: 'Creating your account...' })
    try {
      onSuccess(await register({ name, email, password }))
      setStatus({ type: 'success', message: 'Registration successful.' })
    } catch {
      setStatus({ type: 'error', message: 'Registration failed. Please review your details.' })
    }
  }

  return (
    <AuthForm title="Create account" description="Validation prevents incomplete signup and shows clear errors." status={status} onSubmit={handleSubmit}>
      <Input label="Name" value={name} onChange={setName} autoComplete="name" />
      <Input label="Email" type="email" value={email} onChange={setEmail} autoComplete="email" />
      <Input label="Password" type="password" value={password} onChange={setPassword} autoComplete="new-password" />
      <button className="w-full rounded-xl bg-emerald-600 px-4 py-3 font-semibold text-white hover:bg-emerald-700 disabled:opacity-50" disabled={status.type === 'loading'}>
        Register
      </button>
    </AuthForm>
  )
}

function AuthForm({ title, description, status, onSubmit, children }: React.PropsWithChildren<{ title: string; description: string; status: FormStatus; onSubmit: (event: React.FormEvent<HTMLFormElement>) => void }>) {
  return (
    <form className="space-y-4" onSubmit={onSubmit} noValidate>
      <div>
        <h2 className="text-2xl font-bold text-slate-950">{title}</h2>
        <p className="mt-2 text-sm text-slate-600">{description}</p>
      </div>
      {children}
      {status.message && (
        <p role="status" className={`rounded-xl px-4 py-3 text-sm ${status.type === 'error' ? 'bg-red-50 text-red-700' : 'bg-sky-50 text-sky-700'}`}>
          {status.message}
        </p>
      )}
    </form>
  )
}

function Input({ label, value, onChange, type = 'text', autoComplete }: { label: string; value: string; onChange: (value: string) => void; type?: string; autoComplete?: string }) {
  const id = label.toLowerCase().replaceAll(' ', '-')
  return (
    <label htmlFor={id} className="block text-sm font-medium text-slate-700">
      {label}
      <input
        id={id}
        type={type}
        value={value}
        onChange={(event) => onChange(event.target.value)}
        autoComplete={autoComplete}
        className="mt-2 w-full rounded-xl border border-slate-300 px-4 py-3 outline-none focus:border-indigo-500 focus:ring-4 focus:ring-indigo-100"
      />
    </label>
  )
}

function SsoButtons() {
  return (
    <div className="grid gap-3 sm:grid-cols-2">
      {['google', 'github'].map((provider) => (
        <button key={provider} type="button" onClick={() => startSsoLogin(provider)} className="rounded-xl border border-slate-300 px-4 py-3 font-semibold capitalize hover:bg-slate-50">
          Continue with {provider}
        </button>
      ))}
    </div>
  )
}

function AccountPanel({ session, onLogout }: { session: AuthResponse; onLogout: () => void }) {
  const [providers, setProviders] = useState<SsoProvider[]>([])
  const [status, setStatus] = useState<FormStatus>({ type: 'loading', message: 'Loading providers...' })

  useEffect(() => {
    getSsoProviders()
      .then((items) => {
        setProviders(items)
        setStatus(initialStatus)
      })
      .catch(() => setStatus({ type: 'error', message: 'Could not load linked SSO providers.' }))
  }, [])

  async function handleUnlink(provider: SsoProvider) {
    if (!provider.canUnlink) return
    setStatus({ type: 'loading', message: `Unlinking ${provider.name}...` })
    try {
      await unlinkSsoProvider(provider.id)
      setProviders((items) => items.map((item) => (item.id === provider.id ? { ...item, linked: false, canUnlink: false } : item)))
      setStatus({ type: 'success', message: `${provider.name} unlinked.` })
    } catch {
      setStatus({ type: 'error', message: `Unable to unlink ${provider.name}.` })
    }
  }

  return (
    <div className="space-y-5">
      <div>
        <h2 className="text-2xl font-bold text-slate-950">Account security</h2>
        <p className="mt-2 text-sm text-slate-600">Signed in as {session.user.email}. Manage SSO providers or logout cleanly.</p>
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
