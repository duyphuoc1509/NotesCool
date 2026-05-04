import { useMemo, useState } from 'react'
import { Link, useLocation } from 'react-router-dom'
import axios from 'axios'
import { useAuth } from '../hooks/useAuth'
import { useTranslation } from 'react-i18next'
import type { ApiErrorResponse } from '../services/auth'

function extractErrorMessage(error: unknown): string {
  if (axios.isAxiosError<ApiErrorResponse>(error)) {
    const apiError = error.response?.data
    if (apiError?.errors) {
      return Object.values(apiError.errors).flat().join(' ')
    }
    return apiError?.message ?? apiError?.title ?? 'Registration failed.'
  }

  return error instanceof Error ? error.message : 'Registration failed.'
}

export function RegisterPage() {
  const { t } = useTranslation()
  const { register, isLoading } = useAuth()
  const location = useLocation()

  const redirectTo = useMemo(() => {
    const state = location.state as { from?: { pathname?: string } } | null
    return state?.from?.pathname ?? '/'
  }, [location.state])

  const [fullName, setFullName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [error, setError] = useState('')
  const [validationErrors, setValidationErrors] = useState<Record<string, string>>({})

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    const nextErrors: Record<string, string> = {}

    if (!fullName.trim()) nextErrors.fullName = 'Full name is required.'
    if (!email.trim()) nextErrors.email = 'Email is required.'
    if (!password.trim()) nextErrors.password = 'Password is required.'
    if (password.length > 0 && password.length < 8) {
      nextErrors.password = 'Password must be at least 8 characters.'
    }
    if (confirmPassword !== password) {
      nextErrors.confirmPassword = 'Passwords do not match.'
    }

    setValidationErrors(nextErrors)
    setError('')

    if (Object.keys(nextErrors).length > 0) {
      return
    }

    try {
      await register({ fullName, email, password }, redirectTo)
    } catch (err) {
      setError(extractErrorMessage(err))
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-50 px-4">
      <div className="w-full max-w-md rounded-2xl border border-gray-200 bg-white p-8 shadow-sm">
        <p className="text-sm font-semibold uppercase tracking-wide text-indigo-600">NotesCool CMS</p>
        <h1 className="mt-3 text-3xl font-bold tracking-tight text-gray-950">{t('auth.register')}</h1>
        <p className="mt-2 text-sm text-gray-600">{t('dashboard.welcome')}</p>

        <form className="mt-8 space-y-5" onSubmit={handleSubmit} noValidate>
          <div>
            <label htmlFor="fullName" className="block text-sm font-medium text-gray-700">
              Full name
            </label>
            <input
              id="fullName"
              type="text"
              value={fullName}
              onChange={(event) => setFullName(event.target.value)}
              className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-gray-900 outline-none transition focus:border-indigo-500 focus:ring-2 focus:ring-indigo-200"
              autoComplete="name"
            />
            {validationErrors.fullName ? (
              <p className="mt-1 text-sm text-red-600">{validationErrors.fullName}</p>
            ) : null}
          </div>

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
              autoComplete="new-password"
            />
            {validationErrors.password ? (
              <p className="mt-1 text-sm text-red-600">{validationErrors.password}</p>
            ) : null}
          </div>

          <div>
            <label htmlFor="confirmPassword" className="block text-sm font-medium text-gray-700">
              Confirm password
            </label>
            <input
              id="confirmPassword"
              type="password"
              value={confirmPassword}
              onChange={(event) => setConfirmPassword(event.target.value)}
              className="mt-1 block w-full rounded-lg border border-gray-300 px-3 py-2 text-gray-900 outline-none transition focus:border-indigo-500 focus:ring-2 focus:ring-indigo-200"
              autoComplete="new-password"
            />
            {validationErrors.confirmPassword ? (
              <p className="mt-1 text-sm text-red-600">{validationErrors.confirmPassword}</p>
            ) : null}
          </div>

          {error ? <div className="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-700">{error}</div> : null}

          <button
            type="submit"
            disabled={isLoading}
            className="w-full rounded-lg bg-indigo-600 px-4 py-2.5 text-sm font-semibold text-white transition hover:bg-indigo-500 disabled:cursor-not-allowed disabled:opacity-70"
          >
            {isLoading ? 'Creating account...' : 'Create account'}
          </button>
        </form>

        <p className="mt-6 text-center text-sm text-gray-600">
          Already have an account?{' '}
          <Link
            to="/login"
            state={{ from: { pathname: redirectTo } }}
            className="font-medium text-indigo-600 hover:text-indigo-500"
          >
            Sign in
          </Link>
        </p>
      </div>
    </div>
  )
}
