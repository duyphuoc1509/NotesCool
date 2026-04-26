import { useState } from 'react'
import type { FormEvent, ReactNode } from 'react'
import { CheckCircle2, LoaderCircle, Mail, Lock, UserRound, AlertCircle } from 'lucide-react'
import { AxiosError } from 'axios'
import { useNavigate } from 'react-router-dom'
import api from '../services/api'

type RegisterFormValues = {
  fullName: string
  email: string
  password: string
  confirmPassword: string
}

type RegisterFormErrors = Partial<Record<keyof RegisterFormValues, string>>

type SubmissionState = 'idle' | 'submitting' | 'success' | 'error'

type RegisterApiPayload = {
  fullName: string
  email: string
  password: string
}

const INITIAL_VALUES: RegisterFormValues = {
  fullName: '',
  email: '',
  password: '',
  confirmPassword: '',
}

const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
const PASSWORD_MIN_LENGTH = 8

function validateRegistration(values: RegisterFormValues): RegisterFormErrors {
  const errors: RegisterFormErrors = {}

  if (!values.fullName.trim()) {
    errors.fullName = 'Full name is required.'
  }

  if (!values.email.trim()) {
    errors.email = 'Email is required.'
  } else if (!EMAIL_REGEX.test(values.email)) {
    errors.email = 'Enter a valid email address.'
  }

  if (!values.password) {
    errors.password = 'Password is required.'
  } else if (values.password.length < PASSWORD_MIN_LENGTH) {
    errors.password = `Password must be at least ${PASSWORD_MIN_LENGTH} characters.`
  }

  if (!values.confirmPassword) {
    errors.confirmPassword = 'Please confirm your password.'
  } else if (values.confirmPassword !== values.password) {
    errors.confirmPassword = 'Passwords do not match.'
  }

  return errors
}

function toRegisterPayload(values: RegisterFormValues): RegisterApiPayload {
  return {
    fullName: values.fullName.trim(),
    email: values.email.trim().toLowerCase(),
    password: values.password,
  }
}

function getErrorMessage(error: unknown) {
  if (error instanceof AxiosError) {
    return error.response?.data?.message ?? 'We could not create your account. Please try again.'
  }

  return 'Something went wrong while creating your account.'
}

function InputField({
  id,
  label,
  type,
  value,
  placeholder,
  error,
  onChange,
  icon,
}: {
  id: keyof RegisterFormValues
  label: string
  type: string
  value: string
  placeholder: string
  error?: string
  onChange: (value: string) => void
  icon: ReactNode
}) {
  return (
    <div>
      <label htmlFor={id} className="mb-2 block text-sm font-medium text-slate-700">
        {label}
      </label>
      <div
        className={`flex items-center gap-3 rounded-2xl border bg-white px-4 py-3 shadow-sm transition ${
          error
            ? 'border-red-300 ring-2 ring-red-100'
            : 'border-slate-200 focus-within:border-indigo-400 focus-within:ring-2 focus-within:ring-indigo-100'
        }`}
      >
        <span className="text-slate-400">{icon}</span>
        <input
          id={id}
          type={type}
          value={value}
          onChange={(event) => onChange(event.target.value)}
          placeholder={placeholder}
          aria-invalid={Boolean(error)}
          aria-describedby={error ? `${id}-error` : undefined}
          className="w-full border-none bg-transparent text-sm text-slate-900 outline-none placeholder:text-slate-400"
        />
      </div>
      {error ? (
        <p id={`${id}-error`} className="mt-2 text-sm text-red-600">
          {error}
        </p>
      ) : null}
    </div>
  )
}

export function RegisterPage() {
  const navigate = useNavigate()
  const [values, setValues] = useState(INITIAL_VALUES)
  const [errors, setErrors] = useState<RegisterFormErrors>({})
  const [submissionState, setSubmissionState] = useState<SubmissionState>('idle')
  const [feedbackMessage, setFeedbackMessage] = useState('')

  const isSubmitting = submissionState === 'submitting'

  const updateField = (field: keyof RegisterFormValues, value: string) => {
    setValues((current) => ({ ...current, [field]: value }))
    setErrors((current) => {
      if (!current[field]) {
        return current
      }

      return { ...current, [field]: undefined }
    })

    if (submissionState !== 'idle') {
      setSubmissionState('idle')
      setFeedbackMessage('')
    }
  }

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()

    const nextErrors = validateRegistration(values)
    setErrors(nextErrors)

    if (Object.keys(nextErrors).length > 0) {
      setSubmissionState('error')
      setFeedbackMessage('Please fix the validation errors before submitting.')
      return
    }

    setSubmissionState('submitting')
    setFeedbackMessage('')

    const payload = toRegisterPayload(values)

    try {
      await api.post('/auth/register', payload)
      setSubmissionState('success')
      setFeedbackMessage('Registration successful. Redirecting to sign in...')
      setValues(INITIAL_VALUES)
      setErrors({})
      navigate('/login', {
        replace: true,
        state: {
          registeredEmail: payload.email,
        },
      })
    } catch (error) {
      setSubmissionState('error')
      setFeedbackMessage(getErrorMessage(error))
    }
  }

  return (
    <div className="min-h-screen bg-slate-100 px-4 py-10 sm:px-6 lg:px-8">
      <div className="mx-auto grid max-w-6xl gap-8 lg:grid-cols-[1.1fr_0.9fr]">
        <section className="rounded-[2rem] bg-gradient-to-br from-indigo-600 via-indigo-500 to-sky-500 p-8 text-white shadow-xl sm:p-10">
          <p className="text-sm font-semibold uppercase tracking-[0.3em] text-indigo-100">
            NotesCool registration
          </p>
          <h1 className="mt-5 text-4xl font-bold tracking-tight sm:text-5xl">
            Create your account in a few quick steps.
          </h1>
          <p className="mt-4 max-w-xl text-base text-indigo-50 sm:text-lg">
            Join NotesCool to organize notes, track tasks, and collaborate with your team from one clean workspace.
          </p>

          <div className="mt-10 space-y-4 rounded-3xl bg-white/10 p-6 backdrop-blur-sm">
            {[
              'Use your real email address for account verification.',
              'Choose a password with at least 8 characters.',
              'After registration, continue to the sign-in step to access your dashboard.',
            ].map((item) => (
              <div key={item} className="flex items-start gap-3 text-sm text-indigo-50">
                <CheckCircle2 className="mt-0.5 h-5 w-5 shrink-0" />
                <span>{item}</span>
              </div>
            ))}
          </div>
        </section>

        <section className="rounded-[2rem] border border-slate-200 bg-white p-8 shadow-lg sm:p-10">
          <div className="mb-8">
            <p className="text-sm font-semibold uppercase tracking-[0.25em] text-indigo-600">
              Start now
            </p>
            <h2 className="mt-3 text-3xl font-bold tracking-tight text-slate-950">
              Register a new account
            </h2>
            <p className="mt-3 text-sm text-slate-600">
              Fill in your information below. We&apos;ll guide you to the next step after a successful registration.
            </p>
          </div>

          <form className="space-y-5" onSubmit={handleSubmit} noValidate>
            <InputField
              id="fullName"
              label="Full name"
              type="text"
              value={values.fullName}
              placeholder="Jane Doe"
              error={errors.fullName}
              onChange={(value) => updateField('fullName', value)}
              icon={<UserRound className="h-5 w-5" />}
            />

            <InputField
              id="email"
              label="Email address"
              type="email"
              value={values.email}
              placeholder="jane@example.com"
              error={errors.email}
              onChange={(value) => updateField('email', value)}
              icon={<Mail className="h-5 w-5" />}
            />

            <InputField
              id="password"
              label="Password"
              type="password"
              value={values.password}
              placeholder="Minimum 8 characters"
              error={errors.password}
              onChange={(value) => updateField('password', value)}
              icon={<Lock className="h-5 w-5" />}
            />

            <InputField
              id="confirmPassword"
              label="Confirm password"
              type="password"
              value={values.confirmPassword}
              placeholder="Re-enter your password"
              error={errors.confirmPassword}
              onChange={(value) => updateField('confirmPassword', value)}
              icon={<Lock className="h-5 w-5" />}
            />

            {feedbackMessage ? (
              <div
                className={`flex items-start gap-3 rounded-2xl px-4 py-3 text-sm ${
                  submissionState === 'success'
                    ? 'bg-emerald-50 text-emerald-700'
                    : 'bg-red-50 text-red-700'
                }`}
                role="status"
              >
                {submissionState === 'success' ? (
                  <CheckCircle2 className="mt-0.5 h-5 w-5 shrink-0" />
                ) : (
                  <AlertCircle className="mt-0.5 h-5 w-5 shrink-0" />
                )}
                <span>{feedbackMessage}</span>
              </div>
            ) : null}

            <button
              type="submit"
              disabled={isSubmitting}
              className="inline-flex w-full items-center justify-center gap-2 rounded-2xl bg-slate-950 px-4 py-3 text-sm font-semibold text-white transition hover:bg-slate-800 disabled:cursor-not-allowed disabled:bg-slate-400"
            >
              {isSubmitting ? <LoaderCircle className="h-5 w-5 animate-spin" /> : null}
              {isSubmitting ? 'Creating your account...' : 'Create account'}
            </button>
          </form>
        </section>
      </div>
    </div>
  )
}
