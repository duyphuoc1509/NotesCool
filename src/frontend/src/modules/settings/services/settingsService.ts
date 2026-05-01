import { fetchSsoProviders } from '../infrastructure/ssoProvidersAdapter'
import type { AccountSsoProvider, SsoProvider } from '../types'

export const defaultAccountProviders: AccountSsoProvider[] = [
  {
    id: 'google',
    name: 'Google',
    description: 'Use your Google Workspace or Gmail account to sign in.',
    status: 'linked',
    email: 'admin@notescool.com',
    linkedAt: 'Jan 12, 2026',
    lastUsedAt: 'Today at 09:14',
    accentClassName: 'bg-red-50 text-red-600 ring-red-100',
  },
  {
    id: 'github',
    name: 'GitHub',
    description: 'Connect GitHub for developer-friendly single sign-on.',
    status: 'linked',
    email: 'admin@github.example',
    linkedAt: 'Feb 03, 2026',
    lastUsedAt: 'Mar 20, 2026',
    accentClassName: 'bg-slate-100 text-slate-700 ring-slate-200',
  },
  {
    id: 'microsoft',
    name: 'Microsoft',
    description: 'Sign in with a Microsoft 365 or Azure AD account.',
    status: 'available',
    accentClassName: 'bg-blue-50 text-blue-600 ring-blue-100',
  },
]

export function getLinkedProviders(providers: AccountSsoProvider[]): AccountSsoProvider[] {
  return providers.filter((provider) => provider.status === 'linked')
}

export function linkProvider(providers: AccountSsoProvider[], providerId: string): AccountSsoProvider[] {
  return providers.map((provider) =>
    provider.id === providerId
      ? {
          ...provider,
          status: 'linked',
          email: `admin@${provider.id}.example`,
          linkedAt: 'Just now',
          lastUsedAt: 'Not used yet',
        }
      : provider
  )
}

export function unlinkProvider(providers: AccountSsoProvider[], providerId: string): AccountSsoProvider[] {
  return providers.map((provider) =>
    provider.id === providerId
      ? {
          ...provider,
          status: 'available',
          email: undefined,
          linkedAt: undefined,
          lastUsedAt: undefined,
        }
      : provider
  )
}

export const settingsService = {
  async getEnabledSsoProviders(): Promise<SsoProvider[]> {
    const providers = await fetchSsoProviders()
    return providers.filter((provider) => provider.enabled)
  },
}
