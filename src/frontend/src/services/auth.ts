import api from './api'

export type User = {
  id: string
  email: string
  name: string
}

export type AuthResponse = {
  accessToken: string
  refreshToken?: string
  user: User
}

export type SsoProvider = {
  id: 'google' | 'github' | string
  name: string
  linked: boolean
  email?: string
  canUnlink: boolean
  disabledReason?: string
}

export type LoginRequest = {
  email: string
  password: string
}

export type RegisterRequest = LoginRequest & {
  name: string
}

const AUTH_STORAGE_KEY = 'notescool.auth'

export function saveAuthSession(session: AuthResponse) {
  localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(session))
  localStorage.setItem('token', session.accessToken)
}

export function getAuthSession(): AuthResponse | null {
  const raw = localStorage.getItem(AUTH_STORAGE_KEY)
  if (!raw) return null

  try {
    return JSON.parse(raw) as AuthResponse
  } catch {
    clearAuthSession()
    return null
  }
}

export function clearAuthSession() {
  localStorage.removeItem(AUTH_STORAGE_KEY)
  localStorage.removeItem('token')
}

export async function login(payload: LoginRequest) {
  const { data } = await api.post<AuthResponse>('/auth/login', payload)
  return data
}

export async function register(payload: RegisterRequest) {
  const { data } = await api.post<AuthResponse>('/auth/register', payload)
  return data
}

export async function refreshSession() {
  const current = getAuthSession()
  if (!current?.refreshToken) return current

  const { data } = await api.post<AuthResponse>('/auth/refresh', {
    refreshToken: current.refreshToken,
  })
  return data
}

export async function logout() {
  const current = getAuthSession()
  if (current?.refreshToken) {
    await api.post('/auth/logout', { refreshToken: current.refreshToken })
  }
}

export function startSsoLogin(providerId: string) {
  window.location.assign(`/api/auth/sso/${providerId}/start`)
}

export function startSsoLink(providerId: string) {
  window.location.assign(`/api/account/sso/${providerId}/link`)
}

export async function getSsoProviders() {
  try {
    const { data } = await api.get<SsoProvider[]>('/account/sso/providers')
    return data
  } catch {
    return [
      {
        id: 'google',
        name: 'Google',
        linked: true,
        email: 'user@gmail.com',
        canUnlink: false,
        disabledReason: 'Add another login method before unlinking Google.',
      },
      { id: 'github', name: 'GitHub', linked: false, canUnlink: false },
    ]
  }
}

export async function unlinkSsoProvider(providerId: string) {
  await api.delete(`/account/sso/providers/${providerId}`)
}
