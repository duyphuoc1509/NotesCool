import { useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import { Loader2, LockKeyhole, Mail } from 'lucide-react'
import { loginWithPassword, startSsoLogin } from '../services/auth'

const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
const ssoProviders = [
  { id: 'google', label: 'Continue with Google' },
  { id: 'microsoft', label: 'Continue with Microsoft' },
]

type FieldErrors = Partial<Record<'email' | 'password', string>>

function getRedirectTarget() {
  const params = new URLSearchParams(window.location.search)
  return params.get('redirect') || params.get('returnUrl') || '/'
}

export function LoginPage() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [errors, setErrors] = useState<FieldErrors>({})
  const [submitError, setSubmitError] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const redirectTo = useMemo(() => getRedirectTarget(), [])

  function validate() {
    const nextErrors: FieldErrors = {}

    if (!email.trim()) nextErrors.email = 'Email is required.'
    else if (!emailPattern.test(email)) nextErrors.email = 'Enter a valid email address.'

    if (!password) nextErrors.password = 'Password is required.'
    else if (password.length < 8) nextErrors.password = 'Password must be at least 8 characters.'

    setErrors(nextErrors)
    return Object.keys(nextErrors).length === 0
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setSubmitError('')

    if (!validate()) return

    setIsSubmitting(true)
    try {
      await loginWithPassword({ email: email.trim(), password })
      window.location.assign(redirectTo)
    } catch {
      setSubmitError('Invalid email or password. Please check your credentials and try again.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main className="min-h-screen bg-slate-950 px-4 py-10 text-slate-900 sm:px-6 lg:px-8">
      <div className="mx-auto flex min-h-[calc(100vh-5rem)] w-full max-w-6xl items-center justify-center lg:grid lg:grid-cols-[1fr_28rem] lg:gap-12">
        <section className="hidden text-white lg:block">
          <p className="text-sm font-semibold uppercase tracking-[0.35em] text-indigo-300">NotesCool CMS</p>
          <h1 className="mt-6 max-w-xl text-5xl font-bold tracking-tight">Organize every note, task, and idea in one calm workspace.</h1>
          <p className="mt-6 max-w-lg text-lg text-slate-300">Sign in to continue to your dashboard. Your session is stored securely for authenticated API requests.</p>
        </section>

        <section className="w-full rounded-3xl bg-white p-6 shadow-2xl shadow-indigo-950/40 sm:p-8" aria-labelledby="login-heading">
          <div>
            <p className="text-sm font-semibold uppercase tracking-wide text-indigo-600">Welcome back</p>
            <h2 id="login-heading" className="mt-2 text-3xl font-bold tracking-tight text-slate-950">Sign in to NotesCool</h2>
            <p className="mt-2 text-sm text-slate-500">Use your email and password or continue with SSO.</p>
          </div>

          <div className="mt-8 grid gap-3">
            {ssoProviders.map((provider) => (
              <button
                key={provider.id}
                type="button"
                onClick={() => startSsoLogin(provider.id, redirectTo)}
                className="rounded-xl border border-slate-200 px-4 py-3 text-sm font-semibold text-slate-700 transition hover:border-indigo-300 hover:bg-indigo-50 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
              >
                {provider.label}
              </button>
            ))}
          </div>

          <div className="my-8 flex items-center gap-3 text-xs font-medium uppercase tracking-wide text-slate-400">
            <span className="h-px flex-1 bg-slate-200" />
            or sign in with email
            <span className="h-px flex-1 bg-slate-200" />
          </div>

          <form className="space-y-5" onSubmit={handleSubmit} noValidate>
            <div>
              <label htmlFor="email" className="text-sm font-medium text-slate-700">Email</label>
              <div className="mt-2 flex items-center rounded-xl border border-slate-200 px-3 focus-within:border-indigo-500 focus-within:ring-2 focus-within:ring-indigo-100">
                <Mail className="h-5 w-5 text-slate-400" aria-hidden="true" />
                <input id="email" type="email" value={email} onChange={(event) => setEmail(event.target.value)} className="w-full border-0 bg-transparent px-3 py-3 text-sm outline-none" autoComplete="email" aria-invalid={Boolean(errors.email)} aria-describedby={errors.email ? 'email-error' : undefined} />
              </div>
              {errors.email && <p id="email-error" className="mt-2 text-sm text-red-600">{errors.email}</p>}
            </div>

            <div>
              <label htmlFor="password" className="text-sm font-medium text-slate-700">Password</label>
              <div className="mt-2 flex items-center rounded-xl border border-slate-200 px-3 focus-within:border-indigo-500 focus-within:ring-2 focus-within:ring-indigo-100">
                <LockKeyhole className="h-5 w-5 text-slate-400" aria-hidden="true" />
                <input id="password" type="password" value={password} onChange={(event) => setPassword(event.target.value)} className="w-full border-0 bg-transparent px-3 py-3 text-sm outline-none" autoComplete="current-password" aria-invalid={Boolean(errors.password)} aria-describedby={errors.password ? 'password-error' : undefined} />
              </div>
              {errors.password && <p id="password-error" className="mt-2 text-sm text-red-600">{errors.password}</p>}
            </div>

            {submitError && <div role="alert" className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">{submitError}</div>}

            <button type="submit" disabled={isSubmitting} className="flex w-full items-center justify-center gap-2 rounded-xl bg-indigo-600 px-4 py-3 text-sm font-semibold text-white shadow-lg shadow-indigo-600/20 transition hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-70">
              {isSubmitting && <Loader2 className="h-4 w-4 animate-spin" aria-hidden="true" />}
              {isSubmitting ? 'Signing in...' : 'Sign in'}
            </button>
          </form>
        </section>
      </div>
    </main>
  )
}
