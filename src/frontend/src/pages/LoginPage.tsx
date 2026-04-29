import { useMemo, useState } from 'react'
import { Link, useLocation } from 'react-router-dom'
import axios from 'axios'
import { useAuth } from '../contexts/useAuth'
import { useSsoProviders } from '../hooks/useSsoProviders'
import type { ApiErrorResponse } from '../services/auth'
import type { SsoProvider } from '../services/ssoProviders'

function SsoProviderButton({ provider }: { provider: SsoProvider }) {
  const handleClick = () => {
    if (provider.authorizationUrl) {
      window.location.assign(provider.authorizationUrl)
    }
  }

  return (
    <button
      type="button"
      onClick={handleClick}
      disabled={!provider.authorizationUrl}
      className="flex w-full items-center justify-center rounded-lg border border-gray-300 bg-white px-4 py-2.5 text-sm font-semibold text-gray-700 transition hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-70"
      title={!provider.authorizationUrl ? `${provider.displayName} sign-in is not configured yet.` : undefined}
    >
      Continue with {provider.displayName}
    </button>
  )
}

function extractErrorMessage(error: unknown): string {
  if (axios.isAxiosError<ApiErrorResponse>(error)) {
    const apiError = error.response?.data
    if (apiError?.errors) {
      return Object.values(apiError.errors).flat().join(' ')
    }
    return apiError?.message ?? apiError?.title ?? 'Login failed.'
  }

  return error instanceof Error ? error.message : 'Login failed.'
}

export function LoginPage() {
  const { login, isLoading } = useAuth()
  const { providers, isLoading: isLoadingProviders } = useSsoProviders()
  const location = useLocation()

  const redirectTo = useMemo(() => {
    const state = location.state as { from?: { pathname?: string } } | null
    return state?.from?.pathname ?? '/'
  }, [location.state])

  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [validationErrors, setValidationErrors] = useState<Record<string, string>>({})

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    const nextErrors: Record<string, string> = {}

    if (!email.trim()) nextErrors.email = 'Email is required.'
    if (!password.trim()) nextErrors.password = 'Password is required.'

    setValidationErrors(nextErrors)
    setError('')

    if (Object.keys(nextErrors).length > 0) {
      return
    }

    try {
      await login({ email, password }, redirectTo)
    } catch (err) {
      setError(extractErrorMessage(err))
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-50 px-4">
      <div className="w-full max-w-md rounded-2xl border border-gray-200 bg-white p-8 shadow-sm">
        <p className="text-sm font-semibold uppercase tracking-wide text-indigo-600">NotesCool CMS</p>
        <h1 className="mt-3 text-3xl font-bold tracking-tight text-gray-950">Sign in</h1>
        <p className="mt-2 text-sm text-gray-600">Access your workspace securely.</p>

        <div className="mt-8 space-y-3" aria-live="polite">
          {isLoadingProviders ? (
            <div className="rounded-lg border border-gray-200 bg-gray-50 px-4 py-3 text-center text-sm text-gray-600">
              Loading sign-in providers...
            </div>
          ) : providers.length > 0 ? (
            <>
              {providers.map((provider) => (
                <SsoProviderButton key={provider.id} provider={provider} />
              ))}

              <div className="flex items-center gap-3" role="separator" aria-label="or sign in with email">
                <span className="h-px flex-1 bg-gray-200" />
                <span className="text-xs font-medium uppercase tracking-wide text-gray-500">or</span>
                <span className="h-px flex-1 bg-gray-200" />
              </div>
            </>
          ) : null}
        </div>

        <form className="mt-6 space-y-5" onSubmit={handleSubmit} noValidate>
          <div>
            <label htmlFor="email" className="block text-sm font-medium text-gray-700">
              Email
            </label>
            <input
              id="email"
              type="email"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-gray-900 outline-none transition focus:border-indigo-500 focus:ring-2 focus:ring-indigo-200"
              autoComplete="email"
            />
            {validationErrors.email ? (
              <p className="mt-1 text-sm text-red-600">{validationErrors.email}</p>
            ) : null}
          </div>

          <div>
            <label htmlFor="password" className="block text-sm font-medium text-gray-700">
              Password
            </label>
            <input
              id="password"
              type="password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-gray-900 outline-none transition focus:border-indigo-500 focus:ring-2 focus:ring-indigo-200"
              autoComplete="current-password"
            />
            {validationErrors.password ? (
              <p className="mt-1 text-sm text-red-600">{validationErrors.password}</p>
            ) : null}
          </div>

          {error ? <div className="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div> : null}

          <button
            type="submit"
            disabled={isLoading}
            className="w-full rounded-lg bg-indigo-600 px-4 py-2.5 text-sm font-semibold text-white transition hover:bg-indigo-500 disabled:cursor-not-allowed disabled:opacity-70"
          >
            {isLoading ? 'Signing in...' : 'Sign in'}
          </button>
        </form>

        <p className="mt-6 text-center text-sm text-gray-600">
          Don&apos;t have an account?{' '}
          <Link
            to="/register"
            state={{ from: { pathname: redirectTo } }}
            className="font-medium text-indigo-600 hover:text-indigo-500"
          >
            Create one
          </Link>
        </p>
      </div>
    </div>
  )
}
