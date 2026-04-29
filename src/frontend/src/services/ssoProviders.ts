import api from './api'

export interface SsoProvider {
  id: string
  name: string
  displayName: string
  enabled: boolean
  authorizationUrl?: string
}

interface SsoProviderResponse {
  id?: string
  key?: string
  name?: string
  provider?: string
  displayName?: string
  icon?: string
  enabled?: boolean
  authorizationUrl?: string
  loginUrl?: string
}

function toDisplayName(value: string): string {
  return value
    .split(/[-_\s]+/)
    .filter(Boolean)
    .map((part) => part.charAt(0).toUpperCase() + part.slice(1))
    .join(' ')
}

function normalizeProvider(provider: SsoProviderResponse): SsoProvider | null {
  const rawName = provider.name ?? provider.provider ?? provider.id ?? provider.key
  if (!rawName?.trim()) {
    return null
  }

  const name = rawName.trim()

  return {
    id: (provider.id ?? provider.key ?? name).trim().toLowerCase(),
    name,
    displayName: provider.displayName?.trim() || toDisplayName(name),
    enabled: provider.enabled ?? true,
    authorizationUrl: provider.loginUrl ?? provider.authorizationUrl,
  }
}

export const ssoProvidersService = {
  async getProviders(): Promise<SsoProvider[]> {
    const { data } = await api.get<SsoProviderResponse[]>('/api/auth/sso/providers')
    return data.map(normalizeProvider).filter((provider): provider is SsoProvider => Boolean(provider))
  },
}
