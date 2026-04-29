import { useMemo, useState } from 'react'
import { Link, useLocation } from 'react-router-dom'
import axios from 'axios'
import { useAuth } from '../contexts/useAuth'
import type { ApiErrorResponse } from '../services/auth'
import { API_BASE_URL } from '../constants/env'

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
  const location = useLocation()

  const redirectTo = useMemo(() => {
    const state = location.state as { from?: { pathname?: string } } | null
    return state?.from?.pathname ?? '/'
  }, [location.state])

  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [validationErrors, setValidationErrors] = useState<Record<string, string>>({})
  const [isRedirecting, setIsRedirecting] = useState(false)

  const isFormDisabled = isLoading || isRedirecting

  const handleSsoLogin = (provider: 'google' | 'microsoft') => {
    setError('')
    setIsRedirecting(true)
    window.location.assign(`${API_BASE_URL}/api/auth/sso/login/${provider}`)
  }

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()

    if (isFormDisabled) {
      return
    }

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

        <form className="mt-8 space-y-5" onSubmit={handleSubmit} noValidate>
          <div>
            <label htmlFor="email" className="block text-sm font-medium text-gray-700">
              Email
            </label>
            <input
              id="email"
              type="email"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              disabled={isFormDisabled}
              className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-gray-900 outline-none transition focus:border-indigo-500 focus:ring-2 focus:ring-indigo-200 disabled:opacity-70 disabled:cursor-not-allowed"
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
              disabled={isFormDisabled}
              className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-gray-900 outline-none transition focus:border-indigo-500 focus:ring-2 focus:ring-indigo-200 disabled:opacity-70 disabled:cursor-not-allowed"
              autoComplete="current-password"
            />
            {validationErrors.password ? (
              <p className="mt-1 text-sm text-red-600">{validationErrors.password}</p>
            ) : null}
          </div>

          {error ? <div className="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div> : null}

          <button
            type="submit"
            disabled={isFormDisabled}
            className="w-full rounded-lg bg-indigo-600 px-4 py-2.5 text-sm font-semibold text-white transition hover:bg-indigo-500 disabled:cursor-not-allowed disabled:opacity-70"
          >
            {isLoading ? 'Signing in...' : 'Sign in'}
          </button>
        </form>

        <div className="relative mt-8">
          <div className="absolute inset-0 flex items-center" aria-hidden="true">
            <div className="w-full border-t border-gray-200" />
          </div>
          <div className="relative flex justify-center">
            <span className="bg-white px-3 text-sm text-gray-500">Or continue with</span>
          </div>
        </div>

        <div className="mt-6 grid grid-cols-2 gap-3">
          <button
            type="button"
            onClick={() => handleSsoLogin('google')}
            disabled={isFormDisabled}
            className="flex w-full items-center justify-center gap-2 rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm transition hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-70"
          >
            Google
          </button>
          <button
            type="button"
            onClick={() => handleSsoLogin('microsoft')}
            disabled={isFormDisabled}
            className="flex w-full items-center justify-center gap-2 rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm transition hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-70"
          >
            Microsoft
          </button>
        </div>

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
