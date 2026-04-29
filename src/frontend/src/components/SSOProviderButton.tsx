import type { ButtonHTMLAttributes, ReactNode } from 'react'
import { cn } from '../utils/cn'

export type SSOProvider = 'google' | 'microsoft' | (string & {})

export interface SSOProviderButtonProps
  extends Omit<ButtonHTMLAttributes<HTMLButtonElement>, 'children' | 'disabled' | 'onClick'> {
  provider: SSOProvider
  loading?: boolean
  disabled?: boolean
  onClick?: () => void
}

const PROVIDER_LABELS: Record<string, string> = {
  google: 'Google',
  microsoft: 'Microsoft',
}

function formatProviderName(provider: string) {
  const normalizedProvider = provider.trim()

  if (!normalizedProvider) {
    return 'SSO'
  }

  return (
    PROVIDER_LABELS[normalizedProvider.toLowerCase()] ??
    normalizedProvider.charAt(0).toUpperCase() + normalizedProvider.slice(1)
  )
}

function LoadingSpinner() {
  return (
    <svg
      className="h-5 w-5 animate-spin text-gray-500"
      viewBox="0 0 24 24"
      fill="none"
      aria-hidden="true"
    >
      <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
      <path
        className="opacity-75"
        fill="currentColor"
        d="M4 12a8 8 0 0 1 8-8v4a4 4 0 0 0-4 4H4Z"
      />
    </svg>
  )
}

function GoogleIcon() {
  return (
    <svg className="h-5 w-5 shrink-0" viewBox="0 0 24 24" aria-hidden="true">
      <path
        fill="#4285F4"
        d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09Z"
      />
      <path
        fill="#34A853"
        d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84A11 11 0 0 0 12 23Z"
      />
      <path
        fill="#FBBC05"
        d="M5.84 14.09a6.61 6.61 0 0 1 0-4.18V7.07H2.18a11 11 0 0 0 0 9.86l3.66-2.84Z"
      />
      <path
        fill="#EA4335"
        d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15A10.57 10.57 0 0 0 12 1 11 11 0 0 0 2.18 7.07l3.66 2.84C6.71 7.31 9.14 5.38 12 5.38Z"
      />
    </svg>
  )
}

function MicrosoftIcon() {
  return (
    <svg className="h-5 w-5 shrink-0" viewBox="0 0 24 24" aria-hidden="true">
      <path fill="#F25022" d="M2 2h9.5v9.5H2V2Z" />
      <path fill="#7FBA00" d="M12.5 2H22v9.5h-9.5V2Z" />
      <path fill="#00A4EF" d="M2 12.5h9.5V22H2v-9.5Z" />
      <path fill="#FFB900" d="M12.5 12.5H22V22h-9.5v-9.5Z" />
    </svg>
  )
}

function FallbackIcon() {
  return (
    <span
      className="flex h-5 w-5 shrink-0 items-center justify-center rounded-full border border-gray-300 bg-gray-50 text-[10px] font-bold text-gray-600"
      aria-hidden="true"
    >
      SSO
    </span>
  )
}

function getProviderIcon(provider: string): ReactNode {
  switch (provider.trim().toLowerCase()) {
    case 'google':
      return <GoogleIcon />
    case 'microsoft':
      return <MicrosoftIcon />
    default:
      return <FallbackIcon />
  }
}

export function SSOProviderButton({
  provider,
  loading = false,
  disabled = false,
  onClick,
  className,
  type = 'button',
  ...buttonProps
}: SSOProviderButtonProps) {
  const providerName = formatProviderName(provider)
  const isDisabled = disabled || loading

  return (
    <button
      {...buttonProps}
      type={type}
      disabled={isDisabled}
      aria-busy={loading || undefined}
      onClick={onClick}
      className={cn(
        'flex w-full min-h-11 items-center justify-center gap-3 rounded-lg border border-gray-300 bg-white px-4 py-2.5 text-sm font-semibold text-gray-800 shadow-sm transition',
        'hover:border-gray-400 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-indigo-200 focus:ring-offset-2',
        'disabled:cursor-not-allowed disabled:bg-gray-50 disabled:text-gray-500 disabled:opacity-70',
        'sm:min-h-10 sm:py-2',
        className,
      )}
    >
      {loading ? <LoadingSpinner /> : getProviderIcon(provider)}
      <span className="truncate">{loading ? `Connecting to ${providerName}...` : `Continue with ${providerName}`}</span>
    </button>
  )
}
