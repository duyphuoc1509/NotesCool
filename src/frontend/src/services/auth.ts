import api from './api'
import { AUTH_STORAGE_KEY } from '../constants/auth'

export interface AuthTokens {
  accessToken: string
  refreshToken: string
}

export interface AuthUser {
  id?: string
  email: string
  fullName?: string
}

export interface AuthResponse extends AuthTokens {
  user?: AuthUser
}

export interface AuthSession extends AuthTokens {
  user?: AuthUser
  expiresAt?: number
}

export interface LoginPayload {
  email: string
  password: string
}

export interface RegisterPayload {
  email: string
  password: string
  fullName: string
}

export interface RefreshPayload {
  refreshToken: string
}

export interface ApiErrorResponse {
  message?: string
  title?: string
  errors?: Record<string, string[]>
}

export const authService = {
  async register(payload: RegisterPayload) {
    const { data } = await api.post<AuthResponse>('/api/auth/register', payload)
    return data
  },

  async login(payload: LoginPayload) {
    const { data } = await api.post<AuthResponse>('/api/auth/login', payload)
    return data
  },

  async logout(refreshToken?: string) {
    await api.post('/api/auth/logout', refreshToken ? { refreshToken } : {})
  },

  async refresh(payload: RefreshPayload) {
    const { data } = await api.post<AuthResponse>('/api/auth/refresh', payload)
    return data
  },
}

export function getStoredSession(): AuthSession | null {
  try {
    const raw = localStorage.getItem(AUTH_STORAGE_KEY)
    if (!raw) return null
    return JSON.parse(raw) as AuthSession
  } catch {
    return null
  }
}

export function storeSession(session: AuthSession) {
  localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(session))
  if (session.accessToken) {
    localStorage.setItem('token', session.accessToken)
  }
}

export function clearStoredSession() {
  localStorage.removeItem(AUTH_STORAGE_KEY)
  localStorage.removeItem('token')
  localStorage.removeItem('refreshToken')
}

export function shouldRefreshSession(session: AuthSession | null): boolean {
  if (!session?.refreshToken || !session?.expiresAt) return false
  const margin = 60 * 1000 // 1 minute
  return session.expiresAt - margin < Date.now()
}
