export type SsoProviderStatus = 'linked' | 'available'

export interface SsoProvider {
  id: string
  name: string
  displayName: string
  enabled: boolean
  authorizationUrl?: string
}

export interface AccountSsoProvider {
  id: string
  name: string
  description: string
  status: SsoProviderStatus
  email?: string
  linkedAt?: string
  lastUsedAt?: string
  accentClassName: string
}

export interface UserProfile {
  id?: string
  email?: string
  fullName?: string
  displayName?: string
}

export interface SettingsState {
  accountProviders: AccountSsoProvider[]
  hasPasswordLogin: boolean
}
